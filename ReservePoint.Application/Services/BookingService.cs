using FluentResults;
using ReservePoint.Application.DTOs;
using ReservePoint.Application.Interfaces;
using ReservePoint.Domain.Entities;
using ReservePoint.Domain.Enums;

namespace ReservePoint.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;
    private readonly IResourcesClient _resourcesClient;
    private readonly IOrgClient _orgClient;
    private readonly IUserClient _userClient;

    public BookingService(
        IBookingRepository repository,
        IResourcesClient resourcesClient,
        IOrgClient orgClient,
        IUserClient userClient)
    {
        _repository = repository;
        _resourcesClient = resourcesClient;
        _orgClient = orgClient;
        _userClient = userClient;
    }

    public async Task<Result<BookingGroupDto>> CreateAsync(
        string identityId,
        CreateBookingGroupRequest request,
        CancellationToken ct)
    {
        var organizationId = request.OrganizationId;

        if (!request.ResourceIds.Any())
            return Result.Fail("Выберите хотя бы один ресурс");

        var canCreate = await _userClient.CheckPermissionAsync(
            organizationId, identityId, "BOOKINGS_CREATE", ct);
        if (!canCreate)
            return Result.Fail("Недостаточно прав для создания бронирования");

        var policy = await _orgClient.GetPoliciesAsync(organizationId, ct);
        if (policy is null)
            return Result.Fail("Не удалось получить политики организации");

        var maxDate = DateTime.UtcNow.AddDays(policy.BookingHorizonDays);
        if (request.StartTime > maxDate)
            return Result.Fail($"Нельзя бронировать более чем на {policy.BookingHorizonDays} дней вперёд");

        if (TimeOnly.FromDateTime(request.StartTime) < policy.AllowedTimeFrom)
            return Result.Fail($"Бронирование доступно с {policy.AllowedTimeFrom}");

        if (TimeOnly.FromDateTime(request.EndTime) > policy.AllowedTimeTo)
            return Result.Fail($"Бронирование доступно до {policy.AllowedTimeTo}");

        var activeCount = await _repository.CountActiveAsync(identityId, organizationId, ct);
        if (activeCount >= policy.MaxBookingsPerUser)
            return Result.Fail($"Превышен лимит активных броней ({policy.MaxBookingsPerUser})");

        var resourceIds = request.ResourceIds.Distinct().ToList();
        foreach (var resourceId in resourceIds)
        {
            var resource = await _resourcesClient.GetByIdAsync(resourceId, ct);

            if (resource is null)
                return Result.Fail($"Ресурс {resourceId} не найден");

            if (resource.Status != "available")
                return Result.Fail($"Ресурс '{resource.Name}' недоступен для бронирования");

            var duration = request.EndTime - request.StartTime;
            if (duration.TotalHours > resource.BookingRules.MaxDurationHours)
                return Result.Fail($"Максимальная длительность для '{resource.Name}' — {resource.BookingRules.MaxDurationHours} ч.");

            var hasConflict = await _repository.HasConflictAsync(resourceId, request.StartTime, request.EndTime, ct);
            if (hasConflict)
                return Result.Fail($"Ресурс '{resource.Name}' уже забронирован на это время");
        }

        var snapshot = BookingPolicySnapshot.Create(
            resourceIds.Count > 0
                ? (await _resourcesClient.GetByIdAsync(resourceIds[0], ct))!.BookingRules.MaxDurationHours
                : 0,
            policy.MaxBookingsPerUser,
            policy.AllowedTimeFrom,
            policy.AllowedTimeTo);

        var bookingGroup = BookingGroup.Create(
            identityId,
            organizationId,
            request.StartTime,
            request.EndTime,
            snapshot,
            resourceIds);

        await _repository.AddAsync(bookingGroup, ct);

        return Result.Ok(ToDto(bookingGroup));
    }

    public async Task<Result> CancelAsync(
        Guid bookingGroupId,
        string identityId,
        Guid organizationId,
        CancellationToken ct)
    {
        var group = await _repository.GetByIdAsync(bookingGroupId, ct);

        if (group is null || group.OrganizationId != organizationId)
            return Result.Fail("Бронирование не найдено");

        if (group.IdentityId != identityId)
        {
            var canManageAny = await _userClient.CheckPermissionAsync(
                organizationId, identityId, "BOOKINGS_MANAGE_ANY", ct);
            if (!canManageAny)
                return Result.Fail("Вы не можете отменить чужое бронирование");
        }
        else
        {
            var canCancelOwn = await _userClient.CheckPermissionAsync(
                organizationId, identityId, "BOOKINGS_CANCEL_OWN", ct);
            if (!canCancelOwn)
                return Result.Fail("Недостаточно прав для отмены бронирования");
        }

        group.Cancel();
        await _repository.UpdateAsync(group, ct);

        return Result.Ok();
    }

    public async Task<IEnumerable<AvailableResourceDto>> GetAvailableResourcesAsync(
        Guid organizationId,
        string identityId,
        DateTime from,
        DateTime to,
        CancellationToken ct)
    {
        var canSearch = await _userClient.CheckPermissionAsync(
            organizationId, identityId, "BOOKINGS_SEARCH", ct);
        if (!canSearch)
            return Enumerable.Empty<AvailableResourceDto>();

        var resourcesTask = _resourcesClient.GetAvailableAsync(organizationId, ct);
        var bookedIdsTask = _repository.GetBookedResourceIdsAsync(organizationId, from, to, ct);
        await Task.WhenAll(resourcesTask, bookedIdsTask);

        var resources = resourcesTask.Result;
        var bookedIds = bookedIdsTask.Result.ToHashSet();

        return resources
            .Where(r => !bookedIds.Contains(r.Id))
            .Select(r => new AvailableResourceDto(r.Id, r.Name, r.Type));
    }

    public async Task<IEnumerable<BookingGroupDto>> GetBookingsAsync(
        Guid organizationId,
        string identityId,
        DateTime? from,
        DateTime? to,
        BookingGroupStatus? status,
        CancellationToken ct)
    {
        var canManageAny = await _userClient.CheckPermissionAsync(
            organizationId, identityId, "BOOKINGS_MANAGE_ANY", ct);

        var filterIdentityId = canManageAny ? null : identityId;

        var groups = await _repository.GetAllAsync(
            organizationId, filterIdentityId, from, to, status, ct);

        return groups.Select(ToDto);
    }

    public async Task<IEnumerable<BookingGroupDto>> GetMyBookingsAsync(
        Guid organizationId,
        string identityId,
        DateTime? from,
        DateTime? to,
        BookingGroupStatus? status,
        CancellationToken ct)
    {
        var groups = await _repository.GetAllAsync(
            organizationId, identityId, from, to, status, ct);
        return groups.Select(ToDto);
    }

    public async Task<BookingGroupDto?> GetByIdAsync(Guid id, Guid organizationId, CancellationToken ct)
    {
        var group = await _repository.GetByIdAsync(id, ct);

        if (group is null || group.OrganizationId != organizationId)
            return null;

        return ToDto(group);
    }

    public async Task<IEnumerable<BusySlotDto>> GetBusySlotsAsync(
        Guid resourceId,
        Guid organizationId,
        DateTime from,
        DateTime to,
        CancellationToken ct)
    {
        return await _repository.GetBusySlotsAsync(resourceId, from, to, ct);
    }

    public async Task<IEnumerable<ResourceScheduleDto>> GetResourcesScheduleAsync(
        Guid organizationId,
        string identityId,
        DateTime from,
        DateTime to,
        CancellationToken ct)
    {
        var canSearch = await _userClient.CheckPermissionAsync(
            organizationId, identityId, "BOOKINGS_SEARCH", ct);
        if (!canSearch)
            return Enumerable.Empty<ResourceScheduleDto>();

        var resources = await _resourcesClient.GetAllAsync(organizationId, ct);
        var bookedIds = (await _repository.GetBookedResourceIdsAsync(
            organizationId, from, to, ct)).ToHashSet();

        var result = new List<ResourceScheduleDto>();
        foreach (var resource in resources)
        {
            var busySlots = await _repository.GetBusySlotsAsync(resource.Id, from, to, ct);
            result.Add(new ResourceScheduleDto(
                resource.Id,
                resource.Name,
                resource.Type,
                resource.Status,
                !bookedIds.Contains(resource.Id),
                busySlots
            ));
        }

        return result;
    }

    private static BookingGroupDto ToDto(BookingGroup group) => new(
        group.Id,
        group.IdentityId,
        group.OrganizationId,
        group.StartTime,
        group.EndTime,
        group.Status.ToString(),
        group.CreatedAt,
        group.Bookings.Select(b => new BookingItemDto(b.Id, b.ResourceId, b.Status.ToString())),
        new BookingPolicySnapshotDto(
            group.AppliedPolicy.MaxDurationHours,
            group.AppliedPolicy.MaxBookingsPerUser,
            group.AppliedPolicy.AllowedTimeFrom,
            group.AppliedPolicy.AllowedTimeTo
        )
    );
}
