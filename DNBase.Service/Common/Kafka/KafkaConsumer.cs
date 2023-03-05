using Confluent.Kafka;
using DNBase.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DNBase.Services
{
    /// <summary>
    ///  Provides mechanism to create Kafka Consumer
    /// </summary>
    public interface IKafkaConsumer<TKey, TValue> where TValue : class
    {
        /// <summary>
        ///  Triggered when the service is ready to consume the Kafka topic.
        /// </summary>
        /// <param name="topic">Indicates the message's key for Kafka Topic</param>
        /// <param name="stoppingToken">Indicates cancellation token</param>
        /// <returns></returns>
        Task Consume(string topic, CancellationToken stoppingToken, ClientConfig clientConfig);
        /// <summary>
        /// This will close the consumer, commit offsets and leave the group cleanly.
        /// </summary>
        void Close();
        /// <summary>
        /// Releases all resources used by the current instance of the consumer
        /// </summary>
        void DisposeObject();
    }

    /// <summary>
    /// Base class for implementing Kafka Consumer.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue> where TValue : class
    {
        private IKafkaHandler<TKey, TValue> _handler;
        private IConsumer<TKey, TValue> _consumer;
        private string _topic;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public KafkaConsumer(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Triggered when the service is ready to consume the Kafka topic.
        /// </summary>
        /// <param name="topic">Indicates Kafka Topic</param>
        /// <param name="stoppingToken">Indicates stopping token</param>
        /// <returns></returns>
        public async Task Consume(string topic, CancellationToken stoppingToken, ClientConfig clientConfig)
        {
            var consumerConfig = new ConsumerConfig(clientConfig)
            {
                GroupId = "SourceApp",
                EnableAutoCommit = true,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                ApiVersionRequestTimeoutMs = 5000,
                SocketTimeoutMs = 5000
            };

            using var scope = _serviceScopeFactory.CreateScope();

            _handler = scope.ServiceProvider.GetRequiredService<IKafkaHandler<TKey, TValue>>();
            _consumer = new ConsumerBuilder<TKey, TValue>(consumerConfig).SetValueDeserializer(new KafkaDeserializer<TValue>()).Build();
            _topic = topic;

            await Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        /// <summary>
        /// This will close the consumer, commit offsets and leave the group cleanly.
        /// </summary>
        public void Close()
        {
            _consumer.Close();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the consumer
        /// </summary>
        public void DisposeObject()
        {
            _consumer.Dispose();
        }

        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken);

                    if (result != null)
                    {
                        await _handler.HandleAsync(result.Message.Key, result.Message.Value);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException e)
                {
                    // Consumer errors should generally be ignored (or logged) unless fatal.
                    Console.WriteLine($"Consume error: {e.Error.Reason}");

                    if (e.Error.IsFatal)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected error: {e}");
                    break;
                }
            }
        }

    }
}
