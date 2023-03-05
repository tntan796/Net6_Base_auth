using Confluent.Kafka;
using DNBase.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DNBase.Services
{
    public class KafkaComsumerHostedService : BackgroundService
    {
        private readonly IKafkaConsumer<string, NotifyModel> _consumer;
        private readonly IConfiguration _configuration;
        private readonly ClientConfig _clientKafkaConfig;

        public KafkaComsumerHostedService(ClientConfig clientKafkaConfig, IKafkaConsumer<string, NotifyModel> consumer, IConfiguration configuration)
        {
            _clientKafkaConfig = clientKafkaConfig;
            _consumer = consumer;
            _configuration = configuration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _consumer.Consume(_configuration.GetSection("Kafka:Topic:Notification").Value, stoppingToken, _clientKafkaConfig);
            }
            catch (Exception ex)
            {
                //Log.Error(ex.Message);
            }
        }
        public override void Dispose()
        {
            _consumer.Close();
            _consumer.DisposeObject();

            base.Dispose();
        }
    }
}