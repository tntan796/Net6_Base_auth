using DNBase.Services.Interfaces;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DNBase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QTRoleController : ControllerBase
    {
        private readonly IQT_RoleService _roleService;

        public QTRoleController(IQT_RoleService roleService) => _roleService = roleService;

        [HttpPost("Create")]
        [Authorize]
        public async Task<ServiceResponse> Create([FromBody] RoleRequestModel model)
        {
            return await _roleService.Create(model);
        }

        [HttpPost("Update")]
        [Authorize]
        public async Task<ServiceResponse> Update([FromBody] RoleRequestModel model)
        {
            return await _roleService.Update(model);
        }

        /// <summary>
        /// DVNHAT: Get chi tiết 
        /// </summary>
        [HttpGet("GetDetail/{id}")]
        [Authorize]
        public async Task<ServiceResponse> GetDetail(string id)
        {
            return await _roleService.GetDetail(id);
        }

        /// <summary>
        /// DVNHAT: Get danh sách
        /// </summary>
        [HttpPost("GetList")]
        [Authorize]
        public async Task<ServiceResponse> GetList(PaginatedInputModel paging)
        {
            return await _roleService.GetList(paging);
        }
    }
}