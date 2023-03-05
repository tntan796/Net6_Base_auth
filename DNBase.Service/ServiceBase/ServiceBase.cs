using DNBase.Common;
using DNBase.DataLayer.EF;
using DNBase.DataLayer.EF.Entities;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Http;

namespace DNBase.Services
{
    public abstract class ServiceBase
    {
        protected PrincipalModel _principal = null;
        private readonly ICurrentPrincipal _currentPrincipal;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public ServiceBase(ICurrentPrincipal currentPrincipal, IHttpContextAccessor httpContextAccessor)
        {
            _currentPrincipal = currentPrincipal;
            _principal = _currentPrincipal.Principal;
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual ServiceResponse Ok(object data = default, string code = "", string message = "") => ServiceResponse.Succeed(StatusCodes.Status200OK, data, code, message);

        public virtual ServiceResponse BadRequest(string code = "", string message = "") => ServiceResponse.Fail(StatusCodes.Status400BadRequest, code, message);

        public virtual ServiceResponse Forbidden(string code = "", string message = "") => ServiceResponse.Fail(StatusCodes.Status403Forbidden, code, message);

        public virtual ServiceResponse Unauthorized(string code = "", string message = "") => ServiceResponse.Fail(StatusCodes.Status401Unauthorized, code, message);

        protected CategoryItem GetCategoryItem(IGenericRepository _repository, string parentCode, string code)
        {
            var category = _repository.FistOrDefault<Category>(o => o.Code == parentCode);
            return category != null ? _repository.FistOrDefault<CategoryItem>(o => o.CategoryId == category.Id && o.Code == code) : null;
        }
    }
}