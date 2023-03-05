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
    public class QTChucNangController : ControllerBase
    {
        private readonly IQT_ChucNangService _chucNangService;

        public QTChucNangController(IQT_ChucNangService chucNangService) => _chucNangService = chucNangService;

        /// <summary>
        /// DVNHAT: Thêm mới
        /// </summary>
        [HttpPost("Create")]
        [Authorize]
        public async Task<ServiceResponse> Create([FromBody] QT_ChucNangRequestModel model)
        {
            return await _chucNangService.Create(model);
        }

        /// <summary>
        /// DVNHAT: Cập nhật
        /// </summary>
        [HttpPost("Update")]
        [Authorize]
        public async Task<ServiceResponse> Update([FromBody] QT_ChucNangRequestModel model)
        {
            return await _chucNangService.Update(model);
        }

        /// <summary>
        /// DVNHAT: Xoá một 
        /// </summary>
        [HttpGet("Delete/{id}")]
        [Authorize]
        public async Task<ServiceResponse> Delete(string id)
        {
            return await _chucNangService.Delete(id);
        }

        /// <summary>
        /// DVNHAT: Xoá nhiều 
        /// </summary>
        [HttpGet("DeleteMany")]
        [Authorize]
        public async Task<ServiceResponse> DeleteMany(List<Guid> listId)
        {
            return await _chucNangService.DeleteMany(listId);
        }

        /// <summary>
        /// DVNHAT: Get chi tiết 
        /// </summary>
        [HttpGet("GetDetail/{id}")]
        [Authorize]
        public async Task<ServiceResponse> GetDetail(string id)
        {
            return await _chucNangService.GetDetail(id);
        }

        /// <summary>
        /// DVNHAT: Get danh sách
        /// </summary>
        [HttpPost("GetList")]
        [Authorize]
        public async Task<ServiceResponse> GetList(PaginatedInputModel paging)
        {
            return await _chucNangService.GetList(paging);
        }
    }
}