using DNBase.ViewModel;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace DDTH.DataLayer.MongoProvider
{
    public interface IMongo
    {
        IMongoCollection<TEntity> GetCollection<TEntity>() where TEntity : class;
    }

    public class MongoProvider : IMongo
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;

        public MongoProvider(IOptions<MongoDBSetting> mongoDBSetting)
        {
            _mongoClient = new MongoClient(mongoDBSetting.Value.ConnectionString);
            _mongoDatabase = _mongoClient.GetDatabase(mongoDBSetting.Value.DatabaseName);
        }

        IMongoCollection<TEntity> IMongo.GetCollection<TEntity>()
        {
            return _mongoDatabase.GetCollection<TEntity>(typeof(TEntity).Name);
        }
    }
}