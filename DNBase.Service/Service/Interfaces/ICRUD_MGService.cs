using DNBase.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DNBase.Services.Interfaces
{
    public interface ICRUD_MGService
    {
        Task<ServiceResponse> Create(CRUD_MGRequestModel model);
        Task<ServiceResponse> Update(CRUD_MGRequestModel model);
        Task<ServiceResponse> Delete(string id);
        Task<ServiceResponse> GetDetail(string id);
        Task<ServiceResponse> GetList(PaginatedInputModel paging);
    }
}