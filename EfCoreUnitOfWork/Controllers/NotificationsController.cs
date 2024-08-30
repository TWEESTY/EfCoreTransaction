using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EfCoreUnitOfWork.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("api/notification/{id}")]
        [SwaggerOperation(
            Summary = "Get a notification",
            Description = "Get a notification",
            OperationId = "Notifications_Get",
            Tags = new[] { "NotificationsEndpoint" })]
        public async Task<ActionResult<NotificationEntity>> GetAsync([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            Result<NotificationEntity> result = await _notificationService.GetAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost("api/notification")]
        [SwaggerOperation(
            Summary = "Create a notification",
            Description = "Create a notification",
            OperationId = "Notifications_Create",
            Tags = new[] { "NotificationsEndpoint" })]
        public async Task<ActionResult<NotificationEntity>> CreateAsync([FromBody] string text, CancellationToken cancellationToken = default)
        {
            Result<NotificationEntity> result = await _notificationService.AddAsync(text, cancellationToken);
            return result.ToActionResult(this);
        }
    }
}
