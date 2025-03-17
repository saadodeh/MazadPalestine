using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Notifications;
using MzadPalestine.Application.Features.Notifications.Commands.DeleteAllNotifications;
using MzadPalestine.Application.Features.Notifications.Commands.DeleteNotification;
using MzadPalestine.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using MzadPalestine.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using MzadPalestine.Application.Features.Notifications.Queries.GetUnreadNotificationsCount;
using MzadPalestine.Application.Features.Notifications.Queries.GetUserNotifications;

namespace MzadPalestine.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get paginated list of notifications for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PaginatedList<NotificationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<PaginatedList<NotificationDto>>>> GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isRead = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        var query = new GetUserNotificationsQuery(
            pageNumber,
            pageSize,
            isRead,
            searchTerm,
            sortBy,
            sortDescending);

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get count of unread notifications for the current user
    /// </summary>
    [HttpGet("unread/count")]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<int>>> GetUnreadCount()
    {
        var result = await _mediator.Send(new GetUnreadNotificationsCountQuery());
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPut("{id}/read")]
    [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<Unit>>> MarkAsRead(int id)
    {
        var result = await _mediator.Send(new MarkNotificationAsReadCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Mark all notifications as read for the current user
    /// </summary>
    [HttpPut("read/all")]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<int>>> MarkAllAsRead()
    {
        var result = await _mediator.Send(new MarkAllNotificationsAsReadCommand());
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<Unit>>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteNotificationCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete all notifications for the current user
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<int>>> DeleteAll()
    {
        var result = await _mediator.Send(new DeleteAllNotificationsCommand());
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
