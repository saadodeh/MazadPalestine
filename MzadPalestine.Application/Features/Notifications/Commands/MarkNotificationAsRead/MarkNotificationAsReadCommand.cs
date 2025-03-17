using MediatR;
using MzadPalestine.Application.Common.Models;

namespace MzadPalestine.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public record MarkNotificationAsReadCommand(int Id) : IRequest<Result<Unit>>;
