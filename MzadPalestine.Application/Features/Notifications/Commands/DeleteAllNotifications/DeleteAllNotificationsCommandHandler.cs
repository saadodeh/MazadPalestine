using MediatR;
using Microsoft.EntityFrameworkCore;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.Commands.DeleteAllNotifications;

public class DeleteAllNotificationsCommandHandler : IRequestHandler<DeleteAllNotificationsCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public DeleteAllNotificationsCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<Result<int>> Handle(DeleteAllNotificationsCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<int>.Failure("User not found");

        // Begin transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Get all notifications for the user
            var notifications = await _unitOfWork.Repository<Notification>()
                .GetQueryable()
                .Where(n => n.UserId == currentUser.Id)
                .ToListAsync(cancellationToken);

            // Delete all notifications
            foreach (var notification in notifications)
            {
                _unitOfWork.Repository<Notification>().Delete(notification);
            }

            await _unitOfWork.CompleteAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            return Result<int>.Success(notifications.Count);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
