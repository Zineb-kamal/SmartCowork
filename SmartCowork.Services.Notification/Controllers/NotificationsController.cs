
using Microsoft.AspNetCore.Mvc;
using SmartCowork.Services.Notification.Models.DTOs;
using SmartCowork.Services.Notification.Models;
using SmartCowork.Services.Notification.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Notification>> Create(CreateNotificationDto dto)
    {
        try
        {
            var notification = await _notificationService.CreateNotificationAsync(dto);
            return CreatedAtAction(
                nameof(GetById),
                new { id = notification.Id },
                notification
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return StatusCode(500, "An error occurred while creating the notification");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Notification>> GetById(Guid id)
    {
        var notification = await _notificationService.GetNotificationAsync(id);
        if (notification == null) return NotFound();
        return Ok(notification);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetByUser(Guid userId)
    {
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(notifications);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var success = await _notificationService.MarkAsReadAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _notificationService.DeleteNotificationAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
