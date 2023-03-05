using AutoMapper;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNBase.Hubs
{
    public interface INotificationHubs
    {
        public void Send(NotifyModel notify);
    }

    [Authorize]
    public class NotificationHub : Hub<INotificationHubs>
    {
        private readonly IConfiguration _configuration;
        public NotificationHub(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public override Task OnConnectedAsync()
        {
            var userId = Context.User.Claims.FirstOrDefault(t => t.Type == "UserId");
            var clients = ListClientsModel.Instance();
            if (clients.ContainsKey(userId.Value))
            {
                if (clients[userId.Value].Count >= int.Parse(_configuration.GetSection("HubSettings:MaxClientDevice").Value))
                {
                    clients[userId.Value] = clients[userId.Value].OrderBy(t => t.ConnectedTime).ToList();
                    clients[userId.Value][0] = new ClientModel()
                    {
                        ConnectionId = Context.ConnectionId
                    };
                }
                else
                {
                    clients[userId.Value].Add(new ClientModel()
                    {
                        ConnectionId = Context.ConnectionId
                    });
                }
            }
            else clients.Add(userId.Value, new List<ClientModel>() {
                new ClientModel() {ConnectionId = Context.ConnectionId }
            });
            return base.OnConnectedAsync();
        }
        //public void Notify(MessageModel message)
        //{
        //    var clients = ListClientsModel.Instance();
        //    var userId = Context.User.Claims.FirstOrDefault(t => t.Type == "UserId");
        //    var insertUserNotifies = new List<UserNotification>();
        //    var notification = _mapper.Map<Notification>(message);
        //    notification.CreatedBy = System.Guid.Parse(userId.Value);
        //    foreach (var item in message.ReceiceUserIds.Split(','))
        //    {
        //        var response = _mapper.Map<ResponseMessageModel>(message);
        //        insertUserNotifies.Add(new UserNotification()
        //        {
        //            NotificationId = notification.Id,
        //            ReceiceId = item,
        //            Seen = false,
        //            SeenDate = null
        //        });
        //        if (clients.ContainsKey(item))
        //            Clients.Clients(clients[item].Select(t => t.ConnectionId).ToList()).Send(response);
        //    }

        //    _userNotiRepository.Insert(insertUserNotifies);
        //    _unitOfWork.SaveChanges();
        //}
    }
}