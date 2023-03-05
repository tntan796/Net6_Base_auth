using DNBase.ViewModel;
using System.Threading.Tasks;

namespace DNBase.Services.Interfaces
{
    public interface IQT_RoleService
    {
        Task<ServiceResponse> Create(RoleRequestModel model);

        Task<ServiceResponse> Update(RoleRequestModel model);

        Task<ServiceResponse> GetDetail(string id);

        Task<ServiceResponse> GetList(PaginatedInputModel paging);
    }
}