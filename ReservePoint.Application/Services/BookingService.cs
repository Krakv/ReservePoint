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

    public async Task<Result<BookingDto>> CreateAsync(
        Guid userId,
        Guid organizationId,
        string identityId,
        CreateBookingRequest request,
        CancellationToken ct)
    {
        // 1. Проверить право на создание бронирования
        var canCreate = await _userClient.CheckPermissionAsync(
            organizationId, identityId, "BOOKINGS_CREATE", ct);
        if (!canCreate)
            return Result.Fail("Недостаточно прав для создания бронирования");

        // 2. Получить ресурс и политики параллельно
        var resourceTask = _resourcesClient.GetByIdAsync(request.ResourceId, ct);
        var policyTask = _orgClient.GetPoliciesAsync(organizationId, ct);
        await Task.WhenAll(resourceTask, policyTask);

        var resource = resourceTask.Result;
        var policy = policyTask.Result;

        if (resource is null)
            return Result.Fail("Ресурс не найден");

        if (policy is null)
            return Result.Fail("Не удалось получить политики организации");

        // 3. Проверить статус ресурса
        if (resource.Status != "available")
            return Result.Fail("Ресурс недоступен для бронирования");

        // 4. Проверить горизонт бронирования
        var maxDate = DateTime.UtcNow.AddDays(policy.BookingHorizonDays);
        if (request.StartTime > maxDate)
            return Result.Fail($"Нельзя бронировать более чем на {policy.BookingHorizonDays} дней вперёд");

        // 5. Проверить временное окно
        if (TimeOnly.FromDateTime(request.StartTime) < policy.AllowedTimeFrom)
            return Result.Fail($"Бронирование доступно с {policy.AllowedTimeFrom}");

        if (TimeOnly.FromDateTime(request.EndTime) > policy.AllowedTimeTo)
            return Result.Fail($"Бронирование доступно до {policy.AllowedTimeTo}");

        // 6. Проверить длительность
        var duration = request.EndTime - request.StartTime;
        if (duration.TotalHours > resource.BookingRules.MaxDurationHours)
            return Result.Fail($"Максимальная длительность бронирования — {resource.BookingRules.MaxDurationHours} ч.");

        // 7. Проверить лимит броней пользователя
        var activeCount = await _repository.CountActiveAsync(userId, organizationId, ct);
        if (activeCount >= policy.MaxBookingsPerUser)
            return Result.Fail($"Превышен лимит активных броней ({policy.MaxBookingsPerUser})");

        // 8. Проверить конфликт времени
        var hasConflict = await _repository.HasConflictAsync(
            request.ResourceId, request.StartTime, request.EndTime, ct);
        if (hasConflict)
            return Result.Fail("Ресурс уже забронирован на это время");

        // 9. Создать бронирование
        var snapshot = BookingPolicySnapshot.Create(
            resource.BookingRules.MaxDurationHours,
            policy.MaxBookingsPerUser,
            policy.AllowedTimeFrom,
            policy.AllowedTimeTo);

        var booking = Booking.Create(
            request.ResourceId,
            userId,
            organizationId,
            request.StartTime,
            request.EndTime,
            snapshot);

        await _repository.AddAsync(booking, ct);

        return Result.Ok(ToDto(booking));
    }

    public async Task<Result> CancelAsync(
        Guid bookingId,
        Guid userId,
        Guid organizationId,
        string identityId,
        CancellationToken ct)
    {
        var booking = await _repository.GetByIdAsync(bookingId, ct);

        if (booking is null || booking.OrganizationId != organizationId)
            return Result.Fail("Бронирование не найдено");

        if (booking.UserId != userId)
        {
            // Чужая бронь — нужно право BOOKINGS_MANAGE_ANY
            var canManageAny = await _userClient.CheckPermissionAsync(
                organizationId, identityId, "BOOKINGS_MANAGE_ANY", ct);
            if (!canManageAny)
                return Result.Fail("Вы не можете отменить чужое бронирование");
        }
        else
        {
            // Своя бронь — нужно право BOOKINGS_CANCEL_OWN
            var canCancelOwn = await _userClient.CheckPermissionAsync(
                organizationId, identityId, "BOOKINGS_CANCEL_OWN", ct);
            if (!canCancelOwn)
                return Result.Fail("Недостаточно прав для отмены бронирования");
        }

        booking.Cancel();
        await _repository.UpdateAsync(booking, ct);

        return Result.Ok();
    }

    public async Task<IEnumerable<AvailableResourceDto>> GetAvailableResourcesAsync(
        Guid organizationId,
        Guid userId,
        string identityId,
        DateTime from,
        DateTime to,
        CancellationToken ct)
    {
        // Проверить право на поиск
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

    public async Task<IEnumerable<BookingDto>> GetBookingsAsync(
        Guid organizationId,
        Guid userId,
        string identityId,
        Guid? resourceId,
        DateTime? from,
        DateTime? to,
        BookingStatus? status,
        CancellationToken ct)
    {
        // Проверить может ли видеть все брони организации
        var canManageAny = await _userClient.CheckPermissionAsync(
            organizationId, identityId, "BOOKINGS_MANAGE_ANY", ct);

        // Если нет права управлять любыми — показываем только свои
        var filterUserId = canManageAny ? (Guid?)null : userId;

        var bookings = await _repository.GetAllAsync(
            organizationId, filterUserId, resourceId, from, to, status, ct);

        return bookings.Select(ToDto);
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id, Guid organizationId, CancellationToken ct)
    {
        var booking = await _repository.GetByIdAsync(id, ct);

        if (booking is null || booking.OrganizationId != organizationId)
            return null;

        return ToDto(booking);
    }

    private static BookingDto ToDto(Booking booking) => new(
        booking.Id,
        booking.ResourceId,
        booking.UserId,
        booking.OrganizationId,
        booking.StartTime,
        booking.EndTime,
        booking.Status.ToString(),
        booking.CreatedAt,
        new BookingPolicySnapshotDto(
            booking.AppliedPolicy.MaxDurationHours,
            booking.AppliedPolicy.MaxBookingsPerUser,
            booking.AppliedPolicy.AllowedTimeFrom,
            booking.AppliedPolicy.AllowedTimeTo
        )
    );
}
