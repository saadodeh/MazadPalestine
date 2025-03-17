using FluentValidation;
using MzadPalestine.Application.DTOs.Messages;
using MzadPalestine.Application.Interfaces.Repositories;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.Validators.Messages;

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    private readonly IUserRepository _userRepository;

    public SendMessageRequestValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;

        RuleFor(x => x.ReceiverId)
            .NotEmpty().WithMessage("Receiver ID is required")
            .MustAsync(async (receiverId, cancellation) =>
            {
                var exists = await _userRepository.AnyAsync(u => u.Id == receiverId);
                return exists;
            }).WithMessage("Receiver does not exist")
            .MustAsync(async (receiverId, cancellation) =>
            {
                var user = await _userRepository.GetByIdAsync(receiverId);
                return user?.Status == UserStatus.Active;
            }).WithMessage("Cannot send messages to banned or suspended users")
            .Must((request, receiverId, context) =>
            {
                // Get the current user ID from the context
                var userId = context.RootContextData["CurrentUserId"] as int?;
                return userId != receiverId;
            }).WithMessage("Cannot send messages to yourself");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required")
            .MaximumLength(2000).WithMessage("Message content cannot exceed 2000 characters");
    }
}
