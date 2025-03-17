using MediatR;
using Microsoft.EntityFrameworkCore;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.Queries.GetUnreadNotificationsCount;

public class GetUnreadNotificationsCountQueryHandler : IRequestHandler<GetUnreadNotificationsCountQuery, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public GetUnreadNotificationsCountQueryHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<Result<int>> Handle(GetUnreadNotificationsCountQuery request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<int>.Failure("User not found");

        var unreadCount = await _unitOfWork.Repository<Notification>()
            .GetQueryable()
            .CountAsync(n => n.UserId == currentUser.Id && !n.IsRead, cancellationToken);

        return Result<int>.Success(unreadCount);
    }
}
