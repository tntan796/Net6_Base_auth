using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DNBase.UnitTest.Service.Mock;
using System.Data;
using System.IO;
using System;
using Dapper;
using Xunit;
using DNBase.Services;
using DNBase.DataLayer.Dapper;
using DNBase.Services.Interfaces;
using DNBase.ViewModel;

namespace DNBase.UnitTest.Service.TestCase
{
    public class CRUDServiceTest
    {
        private readonly IDapper _dapper;
        private readonly CurrentPrincipalMock _currentPrincipalMock;
        private DapperMock _dapperMock;

        public CRUDServiceTest()
        {
            IConfiguration config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                              .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                                                              .AddEnvironmentVariables()
                                                              .Build();
            _dapper = new DapperProvider(config);
            //_systemServiceMock = new SystemServiceMock().GhiNhanLichSuThaoTacMock();
            _currentPrincipalMock = new CurrentPrincipalMock().PrincipalMock();
        }

        [Fact]
        public void TestCreateSucces()
        {
            CRUDRequestModel model = new CRUDRequestModel()
            {
            };
            string storeName = "Proc_CRUD_Create";
            DynamicParameters param = new DynamicParameters();
            param.Add("MockTest", "MockTest", DbType.String);
            _dapperMock = new DapperMock().AddParamAsyncMock(model, storeName, param).GetAsyncMock(param, storeName, 1);

            CRUDService cRUDService = new CRUDService(new Logger<ICRUDService>(new LoggerFactory()), _dapperMock.Object, null, null, _currentPrincipalMock.Object);
            var results = cRUDService.Create(model);
            Assert.Equal(200, results.Result.StatusCode);
        }

        [Fact]
        public void TestCreateFail()
        {
            CRUDRequestModel model = new CRUDRequestModel()
            {
            };
            string storeName = "Proc_CRUD_Create";
            DynamicParameters param = new DynamicParameters();
            param.Add("MockTest", "MockTest", DbType.String);

            _dapperMock = new DapperMock().AddParamAsyncMock(model, storeName, param).GetAsyncMock(param, storeName, 2);

            CRUDService cRUDService = new CRUDService(new Logger<ICRUDService>(new LoggerFactory()), _dapperMock.Object, null, null, _currentPrincipalMock.Object);
            var results = cRUDService.Create(model);
            Assert.Equal(400, results.Result.StatusCode);
        }

        [Fact]
        public void TestStoreCreateSucces()
        {
            CRUDRequestModel model = new CRUDRequestModel()
            {

            };
            CRUDService cRUDService = new CRUDService(new Logger<ICRUDService>(new LoggerFactory()), _dapper, null, null, _currentPrincipalMock.Object);
            var results = cRUDService.Create(model);
            Assert.Equal(200, results.Result.StatusCode);
        }
    }
}
