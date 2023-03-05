using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using DNBase.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DNBase.ViewModel;
using System.Linq;
using DNBase.Hubs;

namespace DNBase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _notiHubContext;
        private readonly INotificationService _notificationService;
        public NotificationController(IHubContext<NotificationHub> notiHubContext, INotificationService notificationService)
        {
            _notiHubContext = notiHubContext;
            _notificationService = notificationService;
        }

        [HttpPost("push")]
        public async Task<IActionResult> Push(NotifyModel notify)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Some field are required");
            }

            var res = await _notificationService.Push(notify);

            if (res.StatusCode != 200)
            {
                return Problem(res.Message);
            }

            var clients = ListClientsModel.Instance();
            await _notiHubContext.Clients.Clients(clients["KeyClient"].Select(t => t.ConnectionId).ToList()).SendAsync("Send", res.Data);

            return Ok();
        }
    }
}