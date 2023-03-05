using DNBase.ViewModel;
using System.Threading.Tasks;

namespace DNBase.Services.Interfaces
{
    public interface ICategoryService
    {
        ServiceResponse GetListCategory();

        Task<ServiceResponse> CreateCategory(CategoryCreateRequestModel model);

        Task<ServiceResponse> UpdateCategory(CategoryCreateRequestModel model);

        Task<ServiceResponse> GetHierarchical(CategoryHierarchicalSearchRequest model);

        Task<ServiceResponse> GetTreeItemGroupByCategory(CategoryHierarchicalSearchRequest model);

        Task<ServiceResponse> CreateCategoryItem(CategoryItemCreateRequestModel model);

        Task<ServiceResponse> UpdateCategoryItem(CategoryItemCreateRequestModel model);
    }
}