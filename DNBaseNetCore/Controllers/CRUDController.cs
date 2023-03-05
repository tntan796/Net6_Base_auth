using DNBase.Services.Interfaces;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DNBase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CRUDController : ControllerBase
    {
        private readonly ICRUDService _crudService;

        public CRUDController(ICRUDService crudService)
        {
            _crudService = crudService;
          
        }

        /// <summary>
        /// DVNHAT: Thêm mới
        /// </summary>
        [HttpPost("Create")]
        [Authorize]
        public async Task<ServiceResponse> Create(CRUDRequestModel data)
        {
            return await _crudService.Create(data);
        }

        /// <summary>
        /// DVNHAT: Cập nhật
        /// </summary>
        [HttpPost("Update")]
        [Authorize]
        public async Task<ServiceResponse> Update(CRUDRequestModel data)
        {
            return await _crudService.Update(data);
        }

        /// <summary>
        /// DVNHAT: Xoá một 
        /// </summary>
        [HttpGet("Delete/{id}")]
        [Authorize]
        public async Task<ServiceResponse> Delete(string id)
        {
            return await _crudService.Delete(id);
        }

        /// <summary>
        /// DVNHAT: Xoá nhiều 
        /// </summary>
        [HttpGet("DeleteMany")]
        [Authorize]
        public async Task<ServiceResponse> DeleteMany(List<Guid> listId)
        {
            return await _crudService.DeleteMany(listId);
        }

        /// <summary>
        /// DVNHAT: Get chi tiết 
        /// </summary>
        [HttpGet("GetDetail/{id}")]
        [Authorize]
        public async Task<ServiceResponse> GetDetail(string id)
        {
            return await _crudService.GetDetail(id);
        }

        /// <summary>
        /// DVNHAT: Get danh sách
        /// </summary>
        [HttpPost("GetList")]
        [Authorize]
        public async Task<ServiceResponse> GetList(PaginatedInputModel paging)
        {
            return await _crudService.GetList(paging);
        }
    }
}