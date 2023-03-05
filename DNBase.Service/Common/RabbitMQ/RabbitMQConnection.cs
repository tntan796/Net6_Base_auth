using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace DNBase.Services
{
    public class RabbitModelPooledObjectPolicy : IPooledObjectPolicy<IModel>
    {

        private IConnection _connection;
        private readonly IConfiguration _configuration;

        public RabbitModelPooledObjectPolicy(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IConnection GetConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration.GetSection("RabbitMQ:Host").Value,
                UserName = _configuration.GetSection("RabbitMQ:Username").Value,
                Password = _configuration.GetSection("RabbitMQ:Password").Value,
                Port = int.Parse(_configuration.GetSection("RabbitMQ:Port").Value),
            };
            _connection = factory.CreateConnection();
            return _connection;
        }

        public IModel Create()
        {
            if (_connection == null)
            {
                GetConnection();
            }
            var _chanel = _connection.CreateModel();


            return _chanel;
        }

        public bool Return(IModel obj)
        {
            if (obj.IsOpen)
            {
                return true;
            }
            else
            {
                obj?.Dispose();
                return false;
            }
        }
    }

}
