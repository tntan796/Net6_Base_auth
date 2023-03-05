using AutoMapper;
using Dapper;
using DNBase.Common;
using DNBase.Common.Constants;
using DNBase.DataLayer.Dapper;
using DNBase.DataLayer.EF;
using DNBase.DataLayer.EF.Entities;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DNBase.Services
{
    public partial interface IFilesService
    {
        Task<ServiceResponse> UploadMotFile(IFormFile files, Guid? entityId, string entityName, bool isPrivate);

        Task<ServiceResponse> UploadNhieuFile(UploadFileMutileRequestModel model);

        Task<FileContentResult> HienThiFile(Guid fileId);

        Task<ServiceResponse> XoaNhieuFile(List<Guid> files);

        Task<ServiceResponse> XoaMotFile(Guid fileId);

        Task<ServiceResponse> LuuThongTinFile(string name, string extension, decimal size, string path, Guid? entityId, string entityName, string fileTypeUpload, bool isPrivate);

        Task<FileModel> GetThongTinFile(Guid fileId);

        Task<bool> UpdateIsDeleteFile(Guid? fileId);
    }

    public partial class FilesService : ServiceBase, IFilesService
    {
        private readonly IGenericRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IDapper _dapper;
        private readonly IMapper _mapper;
        private readonly ILogger<IFilesService> _logger;

        public FilesService(
              ILogger<IFilesService> logger
            , IGenericRepository repository
            , IConfiguration config
            , IDapper dapper
            , IMapper mapper
            , IUnitOfWork unitOfWork
            , ICurrentPrincipal currentPrincipal
            , IHttpContextAccessor httpContextAccessor) : base(currentPrincipal, httpContextAccessor)
        {
            _logger = logger;
            _repository = repository;
            _config = config;
            _dapper = dapper;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse> UploadMotFile(IFormFile file, Guid? entityId, string entityName, bool isPrivate)
        {
            try
            {
                Guid result = Guid.Empty;
                if (file != null && file.Length > AppsettingConstants.MAX_VALUES_FILE)
                {
                    return Forbidden("", MessageValidateCommon.MessageUploadFile);
                }
                result = await LuuFile(file, entityId, entityName, isPrivate);
                return Ok(result, "", "Upload thành công!");
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        public async Task<ServiceResponse> UploadNhieuFile(UploadFileMutileRequestModel model)
        {
            try
            {
                List<Guid> result = new List<Guid>(); ;
                if (!model.Files.Any())
                {
                    return BadRequest("", "Bạn chưa đính kèm file upload.");
                }
                var lstUploadInfoModel = JsonConvert.DeserializeObject<List<UploadFileRequestModel>>(model.JsonLstUploadRequestInfo);
                if (!lstUploadInfoModel.Any())
                {
                    return BadRequest("", "Bạn chưa điền thông tin file upload.");
                }
                foreach (var item in model.Files)
                {
                    if (item != null && item.Length > AppsettingConstants.MAX_VALUES_FILE)
                    {
                        return Forbidden("", MessageValidateCommon.MessageUploadFile);
                    }
                    var fileInfo = lstUploadInfoModel.FirstOrDefault(x => x.FileName == item.FileName);
                    if (fileInfo != null)
                    {
                        Guid fileId = await LuuFile(item, fileInfo.EntityId, fileInfo.EntityName, fileInfo.IsPrivate);
                        if (fileId != Guid.Empty)
                        {
                            result.Add(fileId);
                        }
                        else
                        {
                            return BadRequest("", "Upload file lỗi");
                        }
                    }
                }
                await _unitOfWork.SaveChangesAsync();
                return Ok("", "Upload file thành công!");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("", e.Message);
            }
        }

        private async Task<Guid> LuuFile(IFormFile file, Guid? entityId, string entityName, bool isPrivate)
        {
            Guid result = Guid.Empty;
            try
            {
                string targetPath = Path.Combine(_config.GetSection(AppsettingConstants.UPLOAD_FOLDER).Value, entityName != null ? entityName : "").Replace("\\", "/");
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                if (file != null && file.Length > 0)
                {
                    string fileName = file.FileName;
                    string filePath = Path.Combine(targetPath, fileName);
                    string fileNameWithoutEx = Path.GetFileNameWithoutExtension(filePath);

                    var tmpGuid = Guid.NewGuid();
                    filePath = filePath.Replace(fileNameWithoutEx, $"{fileNameWithoutEx}-{tmpGuid}");

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    var entity = new Files
                    {
                        Id = Guid.NewGuid(),
                        EntityId = entityId,
                        Size = file.Length,
                        Path = filePath.Replace(_config.GetSection(AppsettingConstants.UPLOAD_FOLDER).Value, "").Replace("\\", "/"),
                        Name = file.FileName,
                        Extension = Path.GetExtension(file.FileName),
                        EntityName = entityName,
                        IsPrivate = isPrivate,
                        IsDeleted = true
                    };
                    await _repository.AddAsync(entity);
                    await _unitOfWork.SaveChangesAsync();
                    result = entity.Id;
                }
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.Message);
            }
            return result;
        }

        public async Task<FileContentResult> HienThiFile(Guid fileId)
        {
            var file = _repository.FistOrDefaultAsync<Files>(x => x.Id == fileId && (x.IsDeleted == false || x.IsDeleted == null)).Result;
            if (file == null)
            {
                return null;
            }
            if (file.IsPrivate && (_principal == null || _principal.UserId != file.CreatedBy))
            {
                return null;
            }
            string folderUploadPath = _config.GetSection(AppsettingConstants.UPLOAD_FOLDER).Value;
            string fileFullPath = folderUploadPath + file.Path;
            if (!File.Exists(fileFullPath))
            {
                return null;
            }
            FileContentResult fileResult = new FileContentResult(await File.ReadAllBytesAsync(fileFullPath), GetMimeTypeByFileExtension(file.Extension));
            fileResult.FileDownloadName = file.Name;
            return fileResult;
        }

        public async Task<ServiceResponse> XoaNhieuFile(List<Guid> files)
        {
            foreach (var item in files)
            {
                var file = _repository.FistOrDefaultAsync<Files>(x => x.Id == item && (x.IsDeleted == false || x.IsDeleted == null)).Result;
                if (file != null)
                {
                    string targetPath = Path.Combine(_config.GetSection(AppsettingConstants.UPLOAD_FOLDER).Value, file.EntityName, file.Path);
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                        _repository.Delete<Files>(file);
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();
            return Ok("Xóa thành công");
        }

        public async Task<ServiceResponse> XoaMotFile(Guid id)
        {
            var file = await _repository.FistOrDefaultAsync<Files>(x => x.Id == id);
            string targetPath = Path.Combine(_config.GetSection(AppsettingConstants.UPLOAD_FOLDER).Value, file.EntityName, file.Path);
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            _repository.Delete<Files>(file);
            await _unitOfWork.SaveChangesAsync();
            return Ok("", "", "Xóa dữ liệu thành công");
        }

        public async Task<bool> UpdateIsDeleteFile(Guid? fileId)
        {
            var file = _repository.FistOrDefaultAsync<Files>(x => x.Id == fileId && (x.IsDeleted == true || x.IsDeleted == null)).Result;
            if (file == null)
            {
                return false;
            }
            file.IsDeleted = false;
            _repository.Update(file);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result <= 0)
            {
                return false;
            }
            return true;
        }

        public async Task<ServiceResponse> LuuThongTinFile(string name, string extension, decimal size, string path, Guid? entityId, string entityName, string fileTypeUpload, bool isPrivate)
        {
            try
            {
                string storeName = "Proc_Files_Create";
                DynamicParameters dbparams = new DynamicParameters();
                dbparams.Add("Name", name, DbType.String);
                dbparams.Add("Extension", extension, DbType.String);
                dbparams.Add("Size", size, DbType.Decimal);
                dbparams.Add("Path", path, DbType.String);
                dbparams.Add("EntityId", entityId, DbType.Guid);
                dbparams.Add("EntityName", entityName, DbType.String);
                dbparams.Add("FileTypeUpload", fileTypeUpload, DbType.String);
                dbparams.Add("IsPrivate", isPrivate, DbType.Boolean);
                dbparams.Add("CreatedBy", _principal.UserId, DbType.Guid);
                await _dapper.ExecuteAsync(storeName, dbparams, commandType: CommandType.StoredProcedure);
                return Ok(entityId);
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.Message);
                return BadRequest("", oEx.ToString());
            }
        }

        public async Task<FileModel> GetThongTinFile(Guid fileId)
        {
            try
            {
                const string storeName = "Proc_Files_GetDetail";
                DynamicParameters dbparams = new DynamicParameters();
                dbparams.Add("Id", fileId, DbType.Guid);
                return await _dapper.GetAsync<FileModel>(storeName, dbparams, commandType: CommandType.StoredProcedure);
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return null;
            }
        }

        private string GetMimeTypeByFileExtension(string fileExt)
        {
            fileExt = fileExt.ToLower().Replace(".", "");
            switch (fileExt)
            {
                case "doc":
                    return "application/msword";
                case "docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "xls":
                    return "application/vnd.ms-excel";
                case "xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "jpg":
                    return "image/jpeg";
                case "png":
                    return "image/png";
                default:
                    return "application/pdf";
            }
        }
    }
}