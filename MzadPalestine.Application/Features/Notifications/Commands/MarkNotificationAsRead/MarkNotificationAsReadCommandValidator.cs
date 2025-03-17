using FluentValidation;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    private readonly IGenericRepository<Notification> _notificationRepository;

    public MarkNotificationAsReadCommandValidator(IGenericRepository<Notification> notificationRepository)
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
