using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;

namespace DNBase.Services
{
    public interface IRabbitMQProducer
    {
        void Publish<T>(T message, string exchangeName, string exchangeType, string routeKey) where T : class;
    }

    public class RabbitMQProducer : IRabbitMQProducer
    {
        private readonly DefaultObjectPool<IModel> _objectPool;

        public RabbitMQProducer(IPooledObjectPolicy<IModel> objectPolicy)
        {
            _objectPool = new DefaultObjectPool<IModel>(objectPolicy, Environment.ProcessorCount * 2);
        }

        public void Publish<T>(T message, string exchangeName, string exchangeType, string routeKey) where T : class
        {
            try
            {
                if (message == null)
                {
                    return;
                }
                var channel = _objectPool.Get();
                channel.ExchangeDeclare(exchangeName, exchangeType, true, false, null);
                var sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                channel.BasicPublish(exchangeName, routeKey, properties, sendBytes);
                _objectPool.Return(channel);
            }
            catch
            {
                //do nothing
            }

        }
    }
}
