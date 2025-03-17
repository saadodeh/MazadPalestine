using MediatR;
using MzadPalestine.Application.Common.Exceptions;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public MarkNotificationAsReadCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<Result<Unit>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<Unit>.Failure("User not found");

        var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(request.Id);
        if (notification == null)
            throw new NotFoundException(nameof(Notification), request.Id);

        // Verify ownership
        if (notification.UserId != currentUser.Id)
            return Result<Unit>.Failure("You can only mark your own notifications as read");

        // Update notification
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        _unitOfWork.Repository<Notification>().Update(notification);
        await _unitOfWork.CompleteAsync();

        return Result<Unit>.Success(Unit.Value);
    }
}
