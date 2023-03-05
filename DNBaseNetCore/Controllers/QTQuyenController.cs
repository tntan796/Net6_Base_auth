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
    public class QTQuyenController : ControllerBase
    {
        private readonly IQT_QuyenService _quyenService;

        public QTQuyenController(IQT_QuyenService quyenService) => _quyenService = quyenService;

        /// <summary>
        /// DVNHAT: Thêm mới
        /// </summary>
        [HttpPost("Create")]
        [Authorize]
        public async Task<ServiceResponse> Create([FromBody] QT_QuyenRequestModel model)
        {
            return await _quyenService.Create(model);
        }

        /// <summary>
        /// DVNHAT: Cập nhật
        /// </summary>
        [HttpPost("Update")]
        [Authorize]
        public async Task<ServiceResponse> Update([FromBody] QT_QuyenRequestModel model)
        {
            return await _quyenService.Update(model);
        }

        /// <summary>
        /// DVNHAT: Xoá một 
        /// </summary>
        [HttpGet("Delete/{id}")]
        [Authorize]
        public async Task<ServiceResponse> Delete(string id)
        {
            return await _quyenService.Delete(id);
        }

        /// <summary>
        /// DVNHAT: Xoá nhiều 
        /// </summary>
        [HttpGet("DeleteMany")]
        [Authorize]
        public async Task<ServiceResponse> DeleteMany(List<Guid> listId)
        {
            return await _quyenService.DeleteMany(listId);
        }

        /// <summary>
        /// DVNHAT: Get chi tiết 
        /// </summary>
        [HttpGet("GetDetail/{id}")]
        [Authorize]
        public async Task<ServiceResponse> GetDetail(string id)
        {
            return await _quyenService.GetDetail(id);
        }

        /// <summary>
        /// DVNHAT: Get danh sách
        /// </summary>
        [HttpPost("GetList")]
        [Authorize]
        public async Task<ServiceResponse> GetList(PaginatedInputModel paging)
        {
            return await _quyenService.GetList(paging);
        }
    }
}