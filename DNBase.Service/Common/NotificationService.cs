using DNBase.Services.Interfaces;
using System.Threading.Tasks;
using DNBase.ViewModel;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using DNBase.Common.Constants;
using DNBase.Common;

namespace DNBase.Services
{
    public class NotificationService : ServiceBase, INotificationService
    {
        private readonly IKafkaProducer<string, NotifyModel> _producer;
        private readonly IConfiguration _configuration;
        protected readonly ILogger<INotificationService> _logger;

        public NotificationService(IKafkaProducer<string, NotifyModel> producer, 
                                   IConfiguration configuration, 
                                   ILogger<INotificationService> logger,
                                   IHttpContextAccessor httpContextAccessor,
                                   ICurrentPrincipal currentPrincipal) : base(currentPrincipal, httpContextAccessor)
        {
            _producer = producer;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ServiceResponse> Push(NotifyModel message)
        {
            try
            {
                await PushTopic(message);
                return Ok(message, "", MessageResponseCommon.CREATE_SUCCESSFULL);
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        private async Task PushTopic(NotifyModel _noti)
        {
            try
            {
                await _producer.ProduceAsync(_configuration.GetSection("Kafka:KafkaTopic:Notification").Value, null, _noti);
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
            }
        }
    }
}