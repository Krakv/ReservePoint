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
        Console.WriteLine($"[CreateAsync] Start. identityId={identityId}");
        Console.WriteLine($"[CreateAsync] Request object: {System.Text.Json.JsonSerializer.Serialize(request)}");

        var organizationId = request.OrganizationId;
        Console.WriteLine($"[CreateAsync] OrganizationId={organizationId}");

        // Проверка ресурсов
        if (!request.ResourceIds.Any())
        {
            Console.WriteLine("[CreateAsync] No resource IDs provided in request");
            return Result.Fail("Выберите хотя бы один ресурс");
        }

        // Проверка прав на создание бронирования
        var canCreate = await _userClient.CheckPermissionAsync(
            organizationId, identityId, "BOOKINGS_CREATE", ct);
        Console.WriteLine($"[CreateAsync] CheckPermissionAsync result: canCreate={canCreate}");
        if (!canCreate)
        {
            Console.WriteLine("[CreateAsync] User does not have permission to create bookings");
            return Result.Fail("Недостаточно прав для создания бронирования");
        }

        // Получение политики организации
        var policy = await _orgClient.GetPoliciesAsync(organizationId, ct);
        Console.WriteLine(policy is null
            ? "[CreateAsync] Organization policy is null"
            : $"[CreateAsync] Organization policy fetched: {System.Text.Json.JsonSerializer.Serialize(policy)}");
        if (policy is null)
            return Result.Fail("Не удалось получить политики организации");

        // Проверка окна бронирования
        var maxDate = DateTime.UtcNow.AddDays(policy.BookingWindowDays);
        Console.WriteLine($"[CreateAsync] Maximum allowed booking date: {maxDate}");
        if (request.StartTime > maxDate)
        {
            Console.WriteLine($"[CreateAsync] Requested StartTime {request.StartTime} exceeds max allowed date {maxDate}");
            return Result.Fail($"Нельзя бронировать более чем на {policy.BookingWindowDays} дней вперёд");
        }

        // Подсчет активных бронирований пользователя
        var activeCount = await _repository.CountActiveAsync(identityId, organizationId, ct);
        Console.WriteLine($"[CreateAsync] Active bookings count for user: {activeCount}");
        if (activeCount >= policy.MaxActiveBookingsPerUser)
        {
            Console.WriteLine($"[CreateAsync] User has exceeded max active bookings ({policy.MaxActiveBookingsPerUser})");
            return Result.Fail($"Превышен лимит активных броней ({policy.MaxActiveBookingsPerUser})");
        }

        // Убираем дубликаты ресурсов
        var resourceIds = request.ResourceIds.Distinct().ToList();
        Console.WriteLine($"[CreateAsync] Distinct resource IDs: [{string.Join(',', resourceIds)}]");

        // Проверка каждого ресурса
        foreach (var resourceId in resourceIds)
        {
            var resource = await _resourcesClient.GetByIdAsync(resourceId, ct);
            Console.WriteLine(resource is null
                ? $"[CreateAsync] Resource {resourceId} not found"
                : $"[CreateAsync] Resource fetched: {System.Text.Json.JsonSerializer.Serialize(resource)}");

            if (resource is null)
                return Result.Fail($"Ресурс {resourceId} не найден");

            if (resource.Status != "available")
            {
                Console.WriteLine($"[CreateAsync] Resource '{resource.Name}' status='{resource.Status}' is not available");
                return Result.Fail($"Ресурс '{resource.Name}' недоступен для бронирования");
            }

            var duration = request.EndTime - request.StartTime;
            Console.WriteLine($"[CreateAsync] Requested booking duration: {duration.TotalHours} hours");

            if (duration.TotalHours > resource.BookingRules.MaxDurationHours)
            {
                Console.WriteLine($"[CreateAsync] Requested duration exceeds max for resource '{resource.Name}' ({resource.BookingRules.MaxDurationHours}h)");
                return Result.Fail($"Максимальная длительность для '{resource.Name}' — {resource.BookingRules.MaxDurationHours} ч.");
            }

            var hasConflict = await _repository.HasConflictAsync(resourceId, request.StartTime, request.EndTime, ct);
            Console.WriteLine($"[CreateAsync] Conflict check for resource '{resource.Name}': {hasConflict}");
            if (hasConflict)
            {
                Console.WriteLine($"[CreateAsync] Resource '{resource.Name}' is already booked during requested time");
                return Result.Fail($"Ресурс '{resource.Name}' уже забронирован на это время");
            }
        }

        // Создание снимка политики для бронирования
        var firstResourceId = resourceIds.Count > 0 ? resourceIds[0] : Guid.Empty;
        var firstResource = firstResourceId != Guid.Empty ? await _resourcesClient.GetByIdAsync(firstResourceId, ct) : null;
        var snapshot = BookingPolicySnapshot.Create(
            firstResource?.BookingRules.MaxDurationHours ?? 0,
            policy.MaxActiveBookingsPerUser
        );
        Console.WriteLine($"[CreateAsync] BookingPolicySnapshot created: {System.Text.Json.JsonSerializer.Serialize(snapshot)}");

        // Создание группы бронирований
        var bookingGroup = BookingGroup.Create(
            identityId,
            organizationId,
            request.StartTime,
            request.EndTime,
            snapshot,
            resourceIds
        );
        Console.WriteLine($"[CreateAsync] BookingGroup created: {System.Text.Json.JsonSerializer.Serialize(new { bookingGroup.Id, bookingGroup.StartTime, bookingGroup.EndTime, Resources = resourceIds })}");

        // Сохранение группы в репозиторий
        await _repository.AddAsync(bookingGroup, ct);
        Console.WriteLine("[CreateAsync] BookingGroup saved to repository");

        // Конвертация в DTO и вывод результата
        var dto = ToDto(bookingGroup);
        Console.WriteLine($"[CreateAsync] BookingGroupDto created: {System.Text.Json.JsonSerializer.Serialize(dto)}");

        Console.WriteLine("[CreateAsync] Completed successfully");
        return Result.Ok(dto);
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
            group.AppliedPolicy.MaxBookingsPerUser
        )
    );
}