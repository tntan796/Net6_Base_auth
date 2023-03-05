using DNBase.Services.Interfaces;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DNBase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QTUserController : ControllerBase
    {
        private readonly IQT_UserService _userService;

        public QTUserController(IQT_UserService userService) => _userService = userService;

        [HttpPost("Create")]
        [Authorize]
        public async Task<ServiceResponse> Create([FromBody] UserCreateRequestModel model)
        {
            return await _userService.Create(model);
        }

        [HttpPost("Update")]
        [Authorize]
        public async Task<ServiceResponse> Update([FromBody] UserCreateRequestModel model)
        {
            return await _userService.Update(model);
        }

        /// <summary>
        /// DVNHAT: Get chi tiết 
        /// </summary>
        [HttpGet("GetDetail/{id}")]
        [Authorize]
        public async Task<ServiceResponse> GetDetail(string id)
        {
            return await Task.FromResult(_userService.GetDetail(id));
        }

        /// <summary>
        /// DVNHAT: Get danh sách
        /// </summary>
        [HttpPost("GetList")]
        [Authorize]
        public async Task<ServiceResponse> GetList(PaginatedInputModel paging)
        {
            return await _userService.GetList(paging);
        }
    }
}