using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Notifications;

namespace MzadPalestine.Application.Features.Notifications.Queries.GetUserNotifications;

public record GetUserNotificationsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool? IsRead = null,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = true) : IRequest<Result<PaginatedList<NotificationDto>>>;
