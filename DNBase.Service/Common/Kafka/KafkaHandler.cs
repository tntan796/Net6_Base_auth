using Dapper;
using DNBase.DataLayer.Dapper;
using DNBase.ViewModel;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DNBase.Services
{
    /// <summary>
    /// Provides mechanism to create Kafka Handler
    /// </summary>
    /// <typeparam name="Tk">Indicates the message's key for Kafka Topic</typeparam>
    /// <typeparam name="Tv">Indicates the message's value for Kafka Topic</typeparam>
    public interface IKafkaHandler<Tk, Tv>
    {
        /// <summary>
        /// Provide mechanism to handle the consumer message from Kafka
        /// </summary>
        /// <param name="key">Indicates the message's key for Kafka Topic</param>
        /// <param name="value">Indicates the message's value for Kafka Topic</param>
        /// <returns></returns>
        Task HandleAsync(Tk key, Tv value);
    }

    public class KafkaHandler : IKafkaHandler<string, NotifyModel>
    {
        private readonly IDapper _dapperRepository;
        private readonly ILogger<KafkaHandler> _logger;

        public KafkaHandler(IDapper dapperRepository , ILogger<KafkaHandler> logger)
        {
            _dapperRepository = dapperRepository;
            _logger = logger;
        }

        public async Task HandleAsync(string key, NotifyModel value)
        {
            try
            {
                //var _params = new DynamicParameters();
                //foreach (var prop in typeof(NotifyModel).GetProperties())
                //{
                //    _params.Add("@" + FirstCharToLowerCase(prop.Name), prop.GetValue(value));
                //}
                //var res = await _dapperRepository.Execute<List<NotifyModel>>("InsertNotification", _params);
                //if (res.Count < 1) _logger.LogDebug("InsertNotification return null");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }

        private string FirstCharToLowerCase(string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
                return str;

            return char.ToLower(str[0]) + str.Substring(1);
        }
    }
}