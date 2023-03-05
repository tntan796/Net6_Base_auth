using System;
using DNBase.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DNBase.Services.Interfaces
{
    public interface IQT_QuyenService
    {
        Task<ServiceResponse> Create(QT_QuyenRequestModel model);
        Task<ServiceResponse> Update(QT_QuyenRequestModel model);
        Task<ServiceResponse> Delete(string id);
        Task<ServiceResponse> DeleteMany(List<Guid> listId);
        Task<ServiceResponse> GetDetail(string id);
        Task<ServiceResponse> GetList(PaginatedInputModel paging);
    }
}