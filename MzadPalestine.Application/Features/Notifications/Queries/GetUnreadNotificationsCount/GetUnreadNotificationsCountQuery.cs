using MediatR;
using MzadPalestine.Application.Common.Models;

namespace MzadPalestine.Application.Features.Notifications.Queries.GetUnreadNotificationsCount;

public record GetUnreadNotificationsCountQuery : IRequest<Result<int>>;
