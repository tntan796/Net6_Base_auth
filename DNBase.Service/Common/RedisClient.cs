using DNBase.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNBase.Services
{
    public interface IRedisClient
    {
        bool Add(string key, string value);

        bool Add(string key, string value, DateTime expiresAt);

        bool Add<T>(string key, T document) where T : class, new();

        bool Add<T>(string key, T document, DateTime expiresAt) where T : class, new();

        bool Add<T>(string key, List<T> documents) where T : class, new();

        bool Add<T>(string key, List<T> documents, long expiresTime) where T : class, new();

        bool AddInHash<T>(string hashId, string key, T document) where T : class, new();

        bool AddInHash<T>(string key, T document) where T : class, new();

        bool AddInHash<T>(string hashId, IEnumerable<KeyValuePair<string, T>> data) where T : class, new();

        bool AddInHash<T>(IEnumerable<KeyValuePair<string, T>> data) where T : class, new();

        bool AddInHashCustom<T>(string extHashName, string key, T document) where T : class, new();

        bool AddInHashWithCustomName(string hashName, string key, string value);

        bool AddInHashWithCustomName(string[] hashName, IEnumerable<KeyValuePair<string, string>> data);

        bool AddInHashWithCustomName<T>(string[] hashName, IEnumerable<KeyValuePair<string, T>> data);

        bool SetExpireKey(string key, TimeSpan expireTime);

        bool AddInSortedSet(string key, string value, double score);

        bool AddInSortedSet<T>(string value, double score) where T : class, new();

        T Get<T>(string key) where T : class, new();

        List<T> Gets<T>(string key) where T : class, new();

        T GetFromHash<T>(string hashId, string key) where T : class, new();

        T GetFromHash<T>(string key) where T : class, new();

        Task<T> GetFromHashAsync<T>(string hashId, string key) where T : class, new();

        Task<T> GetFromHashAsync<T>(string key) where T : class, new();

        double? GetScoreFromSortedSet(string key, string value);

        double? GetScoreFromSortedSet<T>(string value) where T : class, new();

        List<T> GetsFromHash<T>(string hashId, List<RedisValue> keys) where T : class, new();

        List<T> GetsFromHash<T>(List<string> keys) where T : class, new();

        List<T> GetsFromHash<T>(string hashId, List<string> keys) where T : class, new();

        List<T> GetsFromHash<T>(string hashId) where T : class, new();

        List<T> GetsFromHash<T>() where T : class, new();

        string GetsFromHashWithCustomKey(string hashId, string key);

        List<T> GetKeysFromHash<T>(string hashId, List<RedisValue> keys) where T : class, new();

        List<T> GetKeysFromHash<T>(List<string> keys) where T : class, new();

        List<T> GetKeysFromHash<T>(string hashId, List<string> keys) where T : class, new();

        List<T> GetKeysFromHash<T>(string hashId) where T : class, new();

        bool Remove(string key);

        bool Remove<T>(string key) where T : class, new();

        bool RemoveFromHash<T>(string hashId, string key) where T : class, new();

        bool RemoveFromHash<T>(string key) where T : class, new();

        bool RemoveMuiltiFromHash<T>(List<string> keys) where T : class, new();

        bool RemoveFromSortedSet(string key, string value);

        bool RemoveFromSortedSet<T>(string value) where T : class, new();

        bool RemoveScoreFromSortedSet(string key, double score);

        bool RemoveScoreFromSortedSet<T>(double score) where T : class, new();
    }

    public class RedisClient : IRedisClient
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<RedisClient> _logger;
        private static readonly object _synRoot = new object();
        private ConnectionMultiplexer _conn;
        private static Dictionary<string, ConnectionMultiplexer> dicConn = new Dictionary<string, ConnectionMultiplexer>();
        private IDatabase _db { get; set; }
        private string _namespace = string.Empty;

        public RedisClient(IConfiguration configuration, ILogger<RedisClient> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private IDatabase GetDBInstance()
        {
            try
            {
                lock (_synRoot)
                {
                    if (_db == null)
                    {
                        _db = GetConnection().GetDatabase();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new ArgumentException(ex.Message, ex);
            }
            return _db;
        }

        private ConfigurationOptions GetRedisConfiguration()
        {
            var options = ConfigurationOptions.Parse(String.Format("{0}:{1}", _configuration.GetSection("Redis")["Host"], _configuration.GetSection("Redis")["Port"]));
            options.AbortOnConnectFail = false;
            options.ConnectRetry = 3;
            options.ConnectTimeout = 7000;
            options.SyncTimeout = 7000;
            return options;
        }

        protected string NameOf(object entity)
        {
            var name = "";
            if (entity != null)
            {
                if (string.IsNullOrEmpty(Namespace))
                    name = entity.GetType().Name.ToLower();
                else
                    name = (Namespace + ":" + entity.GetType().Name).ToLower();
            }
            return name;
        }

        protected string NameOf(params string[] values)
        {
            var name = "";

            if (null != values)
            {
                var strValue = string.Join(":", values);
                if (!string.IsNullOrEmpty(strValue))
                {
                    if (string.IsNullOrEmpty(Namespace))
                        name = strValue.ToLower();
                    else
                        name = (Namespace + ":" + strValue).ToLower();
                }
            }
            return name;
        }

        protected ConnectionMultiplexer GetConnection()
        {
            try
            {
                lock (_synRoot)
                {
                    if (!dicConn.TryGetValue(Namespace, out _conn) || _conn == null || !_conn.IsConnected)
                    {
                        _conn = ConnectionMultiplexer.Connect(GetRedisConfiguration());

                        if (dicConn.ContainsKey(Namespace))
                        {
                            dicConn[Namespace] = _conn;
                        }
                        else
                        {
                            dicConn.Add(Namespace, _conn);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new ArgumentException(ex.Message, ex);
            }
            return _conn;
        }

        public string Namespace
        {
            get
            {
                return _namespace;
            }
            set
            {
                _namespace = value;
                if (!string.IsNullOrEmpty(_namespace))
                {
                    _namespace = _namespace.Trim();
                }
            }
        }

        #region Create

        public bool Add(string key, string value)
        {
            try
            {
                var db = GetDBInstance();
                return db.StringSet(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool Add(string key, string value, DateTime expiresAt)
        {
            try
            {
                var db = GetDBInstance();
                return db.StringSet(nameof(key), value, new TimeSpan(expiresAt.Ticks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool Add<T>(string key, T document) where T : class, new()
        {
            try
            {
                _logger.LogError("Add<T>(string key, T document) " + key, document);
                var stringId = NameOf(typeof(T).Name, key);
                var valueJson = JsonConvert.SerializeObject(document);
                var db = GetDBInstance();
                return db.StringSet(stringId, valueJson);
            }
            catch (Exception ex)
            {
                _logger.LogError("Add<T>(string key, T document) " + ex.Message, ex);
                return false;
            }
        }

        public bool Add<T>(string key, T document, DateTime expiresAt) where T : class, new()
        {
            try
            {
                var stringId = NameOf(typeof(T).Name, key);
                var valueJson = JsonConvert.SerializeObject(document);
                var db = GetDBInstance();
                return db.StringSet(stringId, valueJson, new TimeSpan(expiresAt.Ticks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool Add<T>(string key, List<T> documents) where T : class, new()
        {
            try
            {
                var stringId = NameOf(typeof(T).Name, key);
                var valueJson = JsonConvert.SerializeObject(documents);
                var db = GetDBInstance();
                return db.StringSet(stringId, valueJson);
            }
            catch
            {
                return false;
            }
        }

        public bool Add<T>(string key, List<T> documents, long expiresTime) where T : class, new()
        {
            try
            {
                var stringId = NameOf(typeof(T).Name, key);
                var valueJson = JsonConvert.SerializeObject(documents);

                var db = GetDBInstance();
                return db.StringSet(stringId, valueJson, TimeSpan.FromSeconds(expiresTime));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool AddInHash<T>(string hashId, string key, T document) where T : class, new()
        {
            try
            {
                var valueJson = JsonConvert.SerializeObject(document);

                var db = GetDBInstance();
                return db.HashSet(hashId, key, valueJson);
            }
            catch (Exception ex)
            {
                _logger.LogError("AddInHash<T>(string hashId, string key, T document) " + ex.Message, ex);
                return false;
            }
        }

        public bool AddInHash<T>(string key, T document) where T : class, new()
        {
            var hashId = typeof(T).Name;
            return AddInHash<T>(hashId, key, document);
        }

        public bool AddInHash<T>(string hashId, IEnumerable<KeyValuePair<string, T>> data) where T : class, new()
        {
            try
            {
                if (data == null) return false;

                var meta = new List<HashEntry>();
                meta.AddRange(data.Select(s => new HashEntry(s.Key, JsonConvert.SerializeObject(s.Value))).ToList());

                var db = GetDBInstance();
                db.HashSet(hashId, meta.ToArray());

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool AddInHash<T>(IEnumerable<KeyValuePair<string, T>> data) where T : class, new()
        {
            var hashId = typeof(T).Name;
            return AddInHash<T>(hashId, data);
        }

        public bool AddInHashCustom<T>(string extHashName, string key, T document) where T : class, new()
        {
            var hashId = NameOf(typeof(T).Name, extHashName);
            return AddInHash<T>(hashId, key, document);
        }

        public bool AddInHashWithCustomName(string hashName, string key, string value)
        {
            try
            {
                var db = GetDBInstance();
                return db.HashSet(hashName, key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool AddInHashWithCustomName(string[] hashName, IEnumerable<KeyValuePair<string, string>> data)
        {
            try
            {
                var meta = new List<HashEntry>();
                meta.AddRange(data.Select(s => new HashEntry(s.Key, s.Value)).ToList());

                var db = GetDBInstance();
                db.HashSet(NameOf(hashName), meta.ToArray());

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool AddInHashWithCustomName<T>(string[] hashName, IEnumerable<KeyValuePair<string, T>> data)
        {
            try
            {
                var meta = new List<HashEntry>();
                meta.AddRange(data.Select(s => new HashEntry(s.Key, JsonConvert.SerializeObject(s.Value))).ToList());

                var db = GetDBInstance();
                db.HashSet(NameOf(hashName), meta.ToArray());

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool SetExpireKey(string key, TimeSpan expireTime)
        {
            try
            {
                var db = GetDBInstance();
                return db.KeyExpire(key, expireTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool AddInSortedSet(string key, string value, double score)
        {
            try
            {
                var db = GetDBInstance();
                return db.SortedSetAdd(key, value, score);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool AddInSortedSet<T>(string value, double score) where T : class, new()
        {
            var setId = typeof(T).Name;
            return AddInSortedSet(setId, value, score);
        }

        #endregion Create

        #region Get

        public T Get<T>(string key) where T : class, new()
        {
            try
            {
                var stringId = NameOf(typeof(T).Name, key);
                var db = GetDBInstance();
                return JsonConvert.DeserializeObject<T>(db.StringGet(stringId));
            }
            catch (Exception ex)
            {
                _logger.LogError("Get<T>(string key): " + ex.Message, ex);
                return default(T);
            }
        }

        public List<T> Gets<T>(string key) where T : class, new()
        {
            var results = new List<T>();
            try
            {
                var stringId = NameOf(typeof(T).Name, key);

                var db = GetDBInstance();
                var res = db.StringGet(stringId);
                if (res.HasValue)
                    results = JsonConvert.DeserializeObject<List<T>>(res);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return results;
            }
        }

        public T GetFromHash<T>(string hashId, string key) where T : class, new()
        {
            try
            {
                var db = GetDBInstance();
                var valueReply = db.HashGet(hashId, key);
                if (valueReply.HasValue)
                {
                    return JsonConvert.DeserializeObject<T>(valueReply);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return default(T);
            }
        }

        public T GetFromHash<T>(string key) where T : class, new()
        {
            var hashId = typeof(T).Name;
            return GetFromHash<T>(hashId, key);
        }

        public async Task<T> GetFromHashAsync<T>(string key) where T : class, new()
        {
            var hashId = typeof(T).Name;
            return await GetFromHashAsync<T>(hashId, key);
        }

        public async Task<T> GetFromHashAsync<T>(string hashId, string key) where T : class, new()
        {
            try
            {
                var db = GetDBInstance();
                var valueReply = await db.HashGetAsync(hashId, key);
                if (valueReply.HasValue)
                {
                    return JsonConvert.DeserializeObject<T>(valueReply);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return default(T);
            }
        }

        public double? GetScoreFromSortedSet(string key, string value)
        {
            try
            {
                var db = GetDBInstance();
                return db.SortedSetScore(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public double? GetScoreFromSortedSet<T>(string value) where T : class, new()
        {
            var setId = typeof(T).Name;
            return GetScoreFromSortedSet(setId, value);
        }

        public List<T> GetsFromHash<T>(string hashId, List<RedisValue> keys) where T : class, new()
        {
            var results = new List<T>();
            try
            {
                var valueReplies = new List<string>();

                var db = GetDBInstance();
                foreach (var item in keys)
                {
                    var val = db.HashGet(hashId, item).ToString();
                    valueReplies.Add(val);
                }

                foreach (var item in valueReplies)
                {
                    results.AddRange(JsonConvert.DeserializeObject<List<T>>(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return default(List<T>);
            }
            return results;
        }

        public List<T> GetsFromHash<T>(List<string> keys) where T : class, new()
        {
            var hashId = typeof(T).Name;
            var valueReplies = new List<RedisValue>();
            valueReplies.AddRange(keys.Select(s => (RedisValue)s));

            return GetsFromHash<T>(hashId, valueReplies);
        }

        public List<T> GetsFromHash<T>(string hashId, List<string> keys) where T : class, new()
        {
            var valueReplies = new List<RedisValue>();
            valueReplies.AddRange(keys.Select(s => (RedisValue)s));

            return GetsFromHash<T>(hashId, valueReplies);
        }

        public List<T> GetsFromHash<T>(string hashId) where T : class, new()
        {
            var results = new List<T>();
            try
            {
                List<HashEntry> valueReplies;

                var db = GetDBInstance();
                valueReplies = db.HashGetAll(hashId).ToList();

                if (null != valueReplies)
                {
                    results.AddRange(valueReplies.Select(s => JsonConvert.DeserializeObject<T>(s.Value)).ToList());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<T>();
            }
            return results;
        }

        public List<T> GetsFromHash<T>() where T : class, new()
        {
            var hashId = typeof(T).Name;
            return GetsFromHash<T>(hashId);
        }

        public List<T> GetKeysFromHash<T>(string hashId, List<RedisValue> keys) where T : class, new()
        {
            var results = new List<T>();
            try
            {
                List<RedisValue> valueReplies;

                var db = GetDBInstance();
                valueReplies = db.HashGet(hashId, keys.ToArray()).ToList();

                results.AddRange(valueReplies.Where(w => w.HasValue).Select(s => JsonConvert.DeserializeObject<T>(s)).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<T>();
            }
            return results;
        }

        public List<T> GetKeysFromHash<T>(List<string> keys) where T : class, new()
        {
            var hashId = typeof(T).Name;
            var valueReplies = new List<RedisValue>();
            valueReplies.AddRange(keys.Select(s => (RedisValue)s));

            return GetKeysFromHash<T>(hashId, valueReplies);
        }

        public List<T> GetKeysFromHash<T>(string hashId, List<string> keys) where T : class, new()
        {
            var valueReplies = new List<RedisValue>();
            valueReplies.AddRange(keys.Select(s => (RedisValue)s));

            return GetKeysFromHash<T>(hashId, valueReplies);
        }

        public List<T> GetKeysFromHash<T>(string hashId) where T : class, new()
        {
            var results = new List<T>();
            try
            {
                List<HashEntry> valueReplies;

                var db = GetDBInstance();
                valueReplies = db.HashGetAll(hashId).ToList();

                if (null != valueReplies)
                {
                    results.AddRange(valueReplies.Select(s => JsonConvert.DeserializeObject<T>(s.Value)).ToList());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return default(List<T>);
            }
            return results;
        }

        public string GetsFromHashWithCustomKey(string hashId, string key)
        {
            var valueReply = string.Empty;
            try
            {
                var db = GetDBInstance();
                valueReply = db.HashGet(hashId, key);
                return valueReply;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return valueReply;
            }
        }

        #endregion Get

        #region Remove

        public bool Remove(string key)
        {
            try
            {
                var db = GetDBInstance();
                return db.KeyDelete(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool Remove<T>(string key) where T : class, new()
        {
            try
            {
                var stringId = NameOf(typeof(T).Name, key);
                var db = GetDBInstance();
                return db.KeyDelete(stringId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool RemoveFromHash<T>(string hashId, string key) where T : class, new()
        {
            try
            {
                var db = GetDBInstance();
                return db.HashDelete(hashId, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool RemoveFromHash<T>(string key) where T : class, new()
        {
            var hashId = typeof(T).Name;
            return RemoveFromHash<T>(hashId, key);
        }

        public bool RemoveMuiltiFromHash<T>(List<string> keys) where T : class, new()
        {
            try
            {
                var hashId = typeof(T).Name;
                var db = GetDBInstance();

                var valueReplies = new List<RedisValue>();
                valueReplies.AddRange(keys.Select(s => (RedisValue)s));

                return db.HashDelete(hashId, valueReplies.ToArray()) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool RemoveFromSortedSet(string key, string value)
        {
            try
            {
                var db = GetDBInstance();
                return db.SortedSetRemove(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool RemoveFromSortedSet<T>(string value) where T : class, new()
        {
            var setId = typeof(T).Name;
            return RemoveFromSortedSet(setId, value);
        }

        public bool RemoveScoreFromSortedSet(string key, double score)
        {
            try
            {
                var db = GetDBInstance();
                return db.SortedSetRemoveRangeByScore(key, score, score) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool RemoveScoreFromSortedSet<T>(double score) where T : class, new()
        {
            var setId = typeof(T).Name;
            return RemoveScoreFromSortedSet(setId, score);
        }

        #endregion Remove
    }
}