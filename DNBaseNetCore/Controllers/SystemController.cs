using DNBase.Services.Interfaces;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DNBase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly ISystemService _systemService;

        public SystemController(ISystemService systemService) => _systemService = systemService;

        /// <summary>
        /// DVNHAT: Lấy danh mục
        /// </summary>
        [HttpPost("GetListCategoryItem")]
        [Authorize]
        public async Task<ServiceResponse> GetListCategoryItem(string code)
        {
            return await _systemService.GetListCategoryItem(code);
        }

    }
}