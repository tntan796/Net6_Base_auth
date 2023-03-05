using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DNBase.DataLayer.Dapper
{
    public interface IDapper
    {
        DbConnection GetDbconnection();
        int Execute(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        T Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        List<T> GetList<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        IEnumerable<T> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task<int> ExecuteAsync(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task<T> GetAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task<List<T>> GetListAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task<IEnumerable<T>> GetAllAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        DynamicParameters AddParam<T>(T model, string storeName, Guid? userId = null);
        Task<DynamicParameters> AddParamAsync<T>(T model, string storeName, Guid? userId = null);
    }

    public class DapperProvider : IDapper
    {
        private readonly IConfiguration _config;
        private readonly string Connectionstring = "Default";
        public DapperProvider(IConfiguration config)
        {
            _config = config;
        }
        public DbConnection GetDbconnection()
        {
            return new SqlConnection(_config.GetConnectionString(Connectionstring));
        }

        public int Execute(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = GetDbconnection();
            return db.Execute(sp, parms, commandType: commandType);
        }
        public T Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = GetDbconnection();
            return db.QueryFirstOrDefault<T>(sp, parms, commandType: commandType);
        }
        public List<T> GetList<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = GetDbconnection();
            return db.Query<T>(sp, parms, commandType: commandType).ToList();
        }
        public IEnumerable<T> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = GetDbconnection();
            var item = db.Query<T>(sp, parms, commandType: commandType);
            return item;
        }
        public async Task<int> ExecuteAsync(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = GetDbconnection();
            return await db.ExecuteAsync(sp, parms, commandType: commandType);
        }
        public async Task<T> GetAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = GetDbconnection();
            return await db.QueryFirstOrDefaultAsync<T>(sp, parms, commandType: commandType);
        }
        public async Task<List<T>> GetListAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = GetDbconnection();
            return (await db.QueryAsync<T>(sp, parms, commandType: commandType)).ToList();
        }
        public async Task<IEnumerable<T>> GetAllAsync<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = GetDbconnection();
            return await db.QueryAsync<T>(sp, parms, commandType: commandType);
        }
        public DynamicParameters AddParam<T>(T model, string storeName, Guid? userId = null)
        {
            DynamicParameters dbparam = new DynamicParameters();
            var lstParam = GetParamFromProc(storeName);

            foreach (string param in lstParam)
            {
                if (param == "TotalCount")
                {
                    dbparam.Add("TotalCount", 0, DbType.Int32, ParameterDirection.Output);
                }
                else if (param == "CreatedBy" || param == "UpdatedBy" || param == "CurrentUserId")
                {
                    dbparam.Add(param, userId, DbType.Guid);
                }
                else
                {
                    var prop = model.GetType().GetProperty(param);
                    if (prop != null)
                    {
                        var typeDB = dbTypeMap.ContainsKey(prop.PropertyType.Name) ? dbTypeMap[prop.PropertyType.Name] : DbType.String;
                        if (prop.PropertyType.Name == typeof(List<Guid>).Name)
                        {
                            List<Guid> listValue = prop.GetValue(model, null) != null ? (List<Guid>)prop.GetValue(model, null) : new List<Guid>();
                            dbparam.Add(param, listValue.Count > 0 ? string.Join(";", listValue) : "", typeDB);
                        }
                        else
                        {
                            dbparam.Add(param, prop.GetValue(model, null), typeDB);
                        }
                    }
                }
            }
            return dbparam;
        }
        public async Task<DynamicParameters> AddParamAsync<T>(T model, string storeName, Guid? userId = null)
        {
            DynamicParameters dbparam = new DynamicParameters();
            var lstParam = await GetParamFromProcAsync(storeName);

            foreach (string param in lstParam)
            {
                switch (param)
                {
                    case "TotalCount":
                        dbparam.Add("TotalCount", 0, DbType.Int32, ParameterDirection.Output);
                        break;
                    case "CreatedBy":
                    case "UpdatedBy":
                    case "CurrentUserId":
                        dbparam.Add(param, userId, DbType.Guid);
                        break;
                    default:
                        AddParamDefault<T>(model, param, ref dbparam);
                        break;
                }
            }
            return dbparam;
        }
        private void AddParamDefault<T>(T model, string param, ref DynamicParameters dbparam)
        {
            var prop = model.GetType().GetProperty(param);
            if (prop != null && prop.PropertyType.Name == typeof(List<Guid>).Name)
            {
                List<Guid> listValue = prop.GetValue(model, null) != null ? (List<Guid>)prop.GetValue(model, null) : new List<Guid>();
                dbparam.Add(param, listValue.Count > 0 ? string.Join(";", listValue) : "", DbType.String);
            }
            else if (prop != null)
            {
                if (prop.PropertyType.Name == "DateTime" && ((DateTime)prop.GetValue(model, null)).Year < 1973)
                {
                    dbparam.Add(param, null, DbType.DateTime);
                }
                else
                {
                    dbparam.Add(param, prop.GetValue(model, null), dbTypeMap.ContainsKey(prop.PropertyType.Name) ? dbTypeMap[prop.PropertyType.Name] : DbType.String);
                }
            }
        }
        private List<string> GetParamFromProc(string storeName)
        {
            DynamicParameters dbparam = new DynamicParameters();
            dbparam.Add("StoreName", storeName, DbType.String);
            return GetList<string>("Proc_GetListParam", dbparam, commandType: CommandType.StoredProcedure);
        }
        private async Task<List<string>> GetParamFromProcAsync(string storeName)
        {
            DynamicParameters dbparam = new DynamicParameters();
            dbparam.Add("StoreName", storeName, DbType.String);
            return await GetListAsync<string>("Proc_GetListParam", dbparam, commandType: CommandType.StoredProcedure);
        }

        private Dictionary<string, DbType> dbTypeMap = new Dictionary<string, DbType>()
        {
            { typeof(bool).Name, DbType.Boolean},
            { typeof(DateTime).Name, DbType.DateTime},
            { typeof(decimal).Name, DbType.Decimal},
            { typeof(double).Name, DbType.Double},
            { typeof(Guid).Name, DbType.Guid},
             { typeof(Guid?).Name, DbType.Guid},
            { typeof(short).Name, DbType.Int16},
            { typeof(int).Name, DbType.Int32},
            { typeof(long).Name, DbType.Int64},
            { typeof(string).Name, DbType.String},
            { typeof(List<Guid>).Name, DbType.String},
        };
    }
}