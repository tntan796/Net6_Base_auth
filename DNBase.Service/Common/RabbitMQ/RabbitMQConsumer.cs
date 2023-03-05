using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace DNBase.Services
{
    public interface IRabbitMQConsumer
    {
        bool Start_Consumer();
    }

    public class RabbitMQConsumer : IRabbitMQConsumer
    {
        private readonly DefaultObjectPool<IModel> _objectPool;
        private readonly IConfiguration _configuration;

        public RabbitMQConsumer(IPooledObjectPolicy<IModel> objectPolicy, IConfiguration configuration)
        {
            _configuration = configuration;
            _objectPool = new DefaultObjectPool<IModel>(objectPolicy, Environment.ProcessorCount * 2);
        }

        public bool Start_Consumer()
        {
            string queueName = _configuration.GetSection("RabbitMQ:QueueName:Queue1").Value;
            var rabbitMqChannel = _objectPool.Get();
            rabbitMqChannel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            var consumer = new EventingBasicConsumer(rabbitMqChannel);
            consumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
            };
            rabbitMqChannel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            return true;
        }
    }
}
