using DNBase.ViewModel;
using System.Threading.Tasks;

namespace DNBase.Services.Interfaces
{
    public interface IQT_UserService
    {
        Task<ServiceResponse> Create(UserCreateRequestModel model);

        Task<ServiceResponse> Update(UserCreateRequestModel model);

        ServiceResponse GetDetail(string id);

        Task<ServiceResponse> GetList(PaginatedInputModel paging);
    }
}