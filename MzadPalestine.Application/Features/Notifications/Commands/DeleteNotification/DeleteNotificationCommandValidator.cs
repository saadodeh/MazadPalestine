using FluentValidation;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.Commands.DeleteNotification;

public class DeleteNotificationCommandValidator : AbstractValidator<DeleteNotificationCommand>
{
    private readonly IGenericRepository<Notification> _notificationRepository;

    public DeleteNotificationCommandValidator(IGenericRepository<Notification> notificationRepository)
    {
        _notificationRepository = notificationRepository;

        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellation) =>
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                return notification != null;
            })
            .WithMessage("Notification not found");
    }
}
