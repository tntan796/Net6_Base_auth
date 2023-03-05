using Dapper;
using System;
using Moq;
using System.Data;
using DNBase.Common;
using DNBase.ViewModel;
using DNBase.DataLayer.Dapper;
using DNBase.Services.Interfaces;

namespace DNBase.UnitTest.Service.Mock
{
    public class DapperMock : Mock<IDapper>
    {
        public DapperMock AddParamAsyncMock(CRUDRequestModel model, string storeName, DynamicParameters result)
        {
            Setup(x => x.AddParamAsync<CRUDRequestModel>(model, storeName, Guid.Empty)).ReturnsAsync(result);
            return this;
        }
        public DapperMock GetAsyncMock(DynamicParameters parameters, string storeName, int result)
        {
            Setup(x => x.GetAsync<int>(storeName, parameters, CommandType.StoredProcedure)).ReturnsAsync(result);
            return this;
        }
    }
    //public class SystemServiceMock : Mock<ISystemService>
    //{
    //    public SystemServiceMock GhiNhanLichSuThaoTacMock()
    //    {
    //        Setup(x => x.GhiNhanLichSuThaoTac("", "", null, null));
    //        return this;
    //    }
    //}
    public class CurrentPrincipalMock: Mock<ICurrentPrincipal>
    {
        public CurrentPrincipalMock PrincipalMock()
        {
            Setup(x => x.Principal).Returns(new PrincipalModel
            {
                UserId = Guid.Empty,
                UserName = "MockTest"
            });
            return this;
        }
    }
}
