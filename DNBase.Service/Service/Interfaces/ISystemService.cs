using DNBase.ViewModel;
using System.Threading.Tasks;

namespace DNBase.Services.Interfaces
{
    public interface ISystemService
    {
        Task<ServiceResponse> GetListCategoryItem(string code);
    }
}