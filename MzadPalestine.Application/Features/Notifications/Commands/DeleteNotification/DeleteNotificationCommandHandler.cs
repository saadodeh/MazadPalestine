using MediatR;
using MzadPalestine.Application.Common.Exceptions;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.Commands.DeleteNotification;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public DeleteNotificationCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<Result<Unit>> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<Unit>.Failure("User not found");

        var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(request.Id);
        if (notification == null)
            throw new NotFoundException(nameof(Notification), request.Id);

        // Verify ownership
        if (notification.UserId != currentUser.Id)
            return Result<Unit>.Failure("You can only delete your own notifications");

        // Begin transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _unitOfWork.Repository<Notification>().Delete(notification);
            await _unitOfWork.CompleteAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
