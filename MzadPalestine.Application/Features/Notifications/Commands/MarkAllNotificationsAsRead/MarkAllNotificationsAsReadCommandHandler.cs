using MediatR;
using Microsoft.EntityFrameworkCore;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public MarkAllNotificationsAsReadCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<Result<int>> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<int>.Failure("User not found");

        // Begin transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Get all unread notifications for the user
            var unreadNotifications = await _unitOfWork.Repository<Notification>()
                .GetQueryable()
                .Where(n => n.UserId == currentUser.Id && !n.IsRead)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;

            // Update all notifications
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
                _unitOfWork.Repository<Notification>().Update(notification);
            }

            await _unitOfWork.CompleteAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            return Result<int>.Success(unreadNotifications.Count);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
