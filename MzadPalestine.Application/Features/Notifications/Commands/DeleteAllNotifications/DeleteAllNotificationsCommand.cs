using MediatR;
using MzadPalestine.Application.Common.Models;

namespace MzadPalestine.Application.Features.Notifications.Commands.DeleteAllNotifications;

public record DeleteAllNotificationsCommand : IRequest<Result<int>>;
