using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Notifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.Queries.GetUserNotifications;

public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, Result<PaginatedList<NotificationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public GetUserNotificationsQueryHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<Result<PaginatedList<NotificationDto>>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<PaginatedList<NotificationDto>>.Failure("User not found");

        // Start with base query
        var query = _unitOfWork.Repository<Notification>()
            .GetQueryable()
            .Where(n => n.UserId == currentUser.Id);

        // Apply filters
        if (request.IsRead.HasValue)
            query = query.Where(n => n.IsRead == request.IsRead.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(n => 
                n.Title.Contains(request.SearchTerm) || 
                n.Message.Contains(request.SearchTerm));

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "createdat" => request.SortDescending 
                ? query.OrderByDescending(n => n.CreatedAt)
                : query.OrderBy(n => n.CreatedAt),
            "readat" => request.SortDescending
                ? query.OrderByDescending(n => n.ReadAt)
                : query.OrderBy(n => n.ReadAt),
            "type" => request.SortDescending
                ? query.OrderByDescending(n => n.Type)
                : query.OrderBy(n => n.Type),
            _ => query.OrderByDescending(n => n.CreatedAt) // Default sort
        };

        // Execute query with pagination
        var notifications = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<NotificationDto>()
            .ToListAsync(cancellationToken);

        // Get total count for pagination
        var totalCount = await query.CountAsync(cancellationToken);

        var paginatedList = new PaginatedList<NotificationDto>(
            notifications,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<NotificationDto>>.Success(paginatedList);
    }
}
