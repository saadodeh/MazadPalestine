using MediatR;
using MzadPalestine.Application.Common.Models;

namespace MzadPalestine.Application.Features.Notifications.Commands.DeleteNotification;

public record DeleteNotificationCommand(int Id) : IRequest<Result<Unit>>;
