using DNBase.Common;
using DNBase.DataLayer.EF.Entities;
using DNBase.Services;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DNBase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public partial class FilesController : ControllerBase
    {
        private readonly IFilesService _filesService;
        private readonly IJwtHandler _jwtHandler;

        public FilesController(IFilesService itemService, IJwtHandler jwtHandler)
        {
            _filesService = itemService;
            _jwtHandler = jwtHandler;
        }

        [HttpPost("UploadMotFile")]
        [Authorize]
        public async Task<ServiceResponse> UploadMotFile(IFormFile file, Guid? entityId, string entityName, bool isPrivate = false)
        {
            var result = await _filesService.UploadMotFile(file, entityId, entityName, isPrivate);
            return result;
        }

        [HttpPost("UploadNhieuFile")]
        [Authorize]
        public async Task<ServiceResponse> UploadNhieuFile([FromForm] UploadFileMutileRequestModel model)
        {
            var result = await _filesService.UploadNhieuFile(model);
            return result;
        }

        [HttpGet("HienThiFile/{id}/{token}")]
        public async Task<IActionResult> HienThiFile(string id, string token)
        {
            var principal = _jwtHandler.GetPrincipalFromToken(token);
            if (principal != null && !String.IsNullOrEmpty(principal.Identity.Name) && !string.IsNullOrEmpty(id))
            {
                return await _filesService.HienThiFile(Guid.Parse(id));
            }
            else
            {
                return null;
            }
        }

        [HttpPost("XoaNhieuFile")]
        [Authorize]
        public async Task<ServiceResponse> XoaNhieuFile(List<Guid> files)
        {
            return files.Count == 0 ? null : await _filesService.XoaNhieuFile(files);
        }

        [HttpDelete("XoaMotFile/{id}")]
        [Authorize]
        public async Task<ServiceResponse> XoaMotFile(string id)
        {
            return string.IsNullOrEmpty(id) ? null : await _filesService.XoaMotFile(Guid.Parse(id));
        }
    }
}