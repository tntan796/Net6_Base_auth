using AutoMapper;
using DNBase.DataLayer.Dapper;
using DNBase.DataLayer.EF.Entities;
using DNBase.Services.Interfaces;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Linq.Expressions;
using DNBase.Common.Constants;
using DNBase.Common;
using DNBase.DataLayer.EF;

namespace DNBase.Services
{
    public class QT_RoleService<TRole> : ServiceBase, IQT_RoleService where TRole : AppRole
    {
        private readonly ILogger<ISystemService> _logger;
        private readonly IMapper _mapper;
        private readonly IDapper _dapper;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IGenericRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public QT_RoleService(ILogger<ISystemService> logger
            , IMapper mapper
            , IDapper dapper
            , RoleManager<AppRole> roleManager
            , IGenericRepository repository
            , IUnitOfWork unitOfWork
            , ICurrentPrincipal currentPrincipal
            , IHttpContextAccessor httpContextAccessor) : base(currentPrincipal, httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _dapper = dapper;
            _roleManager = roleManager;
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse> Create(RoleRequestModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    return BadRequest("", String.Format(MessageValidateCommon.Required, "Tên vai trò"));
                }
                AppRole role = _mapper.Map<AppRole>(model);
                role.Id = Guid.NewGuid();
                await _roleManager.CreateAsync(role);
                //Gắn quyền với vai trò
                if (model.listChucNangQuyen.Count > 0)
                {
                    foreach (ChucNangQuyenRequestModel chucNangQuyen in model.listChucNangQuyen)
                    {
                        QT_VaiTroQuyen qT_VaiTroQuyen = new QT_VaiTroQuyen() { VaiTroId = role.Id, ChucNangId = chucNangQuyen.ChucNangId, QuyenId = chucNangQuyen.QuyenId, CreatedBy = _principal.UserId };
                        await _repository.AddAsync<QT_VaiTroQuyen>(qT_VaiTroQuyen);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
                return Ok(model, "", MessageResponseCommon.CREATE_SUCCESSFULL);
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        public async Task<ServiceResponse> Update(RoleRequestModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    return BadRequest("", String.Format(MessageValidateCommon.Required, "Tên vai trò"));
                }
                var role = _repository.FistOrDefault<AppRole>(o => o.Id == null && o.IsDeleted == false);

                if (role == null)
                {
                    return BadRequest("", MessageValidateCommon.NotFound);
                }

                _mapper.Map<RoleRequestModel, AppRole>(model, role);

                _repository.Update<AppRole>(role);

                //Cập nhật gắn người dùng với vai trò
                if (model.listChucNangQuyen.Count > 0)
                {
                    var listRole = _repository.Where<QT_VaiTroQuyen>(o => o.VaiTroId == role.Id);
                    if (listRole != null && listRole.Count > 0)
                    {
                        listRole.ForEach(o => o.IsDeleted = true);
                    }
                    foreach (ChucNangQuyenRequestModel chucNangQuyen in model.listChucNangQuyen)
                    {
                        QT_VaiTroQuyen qT_VaiTroQuyen = _repository.FistOrDefault<QT_VaiTroQuyen>(o => o.VaiTroId == role.Id && o.ChucNangId == chucNangQuyen.ChucNangId && o.QuyenId == chucNangQuyen.QuyenId);
                        if (qT_VaiTroQuyen != null)
                        {//Nếu là cập nhật
                            qT_VaiTroQuyen.IsDeleted = false;
                            qT_VaiTroQuyen.UpdatedBy = _principal.UserId;
                            _repository.Update<QT_VaiTroQuyen>(qT_VaiTroQuyen);
                        }
                        else
                        {//Nếu là thêm mới
                            qT_VaiTroQuyen = new QT_VaiTroQuyen() { VaiTroId = role.Id, ChucNangId = chucNangQuyen.ChucNangId, QuyenId = chucNangQuyen.QuyenId, CreatedBy = _principal.UserId };
                            await _repository.AddAsync<QT_VaiTroQuyen>(qT_VaiTroQuyen);
                        }
                    }
                }
                await _unitOfWork.SaveChangesAsync();
                return Ok(model, "", MessageResponseCommon.UPDATE_SUCCESSFULL);
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        public async Task<ServiceResponse> GetDetail(string id)
        {
            try
            {
                var gId = Guid.Parse(id);
                var role = _repository.FistOrDefault<AppRole>(u => u.Id == gId);
                if (role == null)
                {
                    return BadRequest("", "not found.");
                }
                RoleRespondModel roleRespond = _mapper.Map<RoleRespondModel>(role);
                //Lấy danh mục chức năng
                var listChucNang = await _repository.WhereAsync<QT_ChucNang>(o => o.IsDeleted == false);
                //Lấy danh mục quyền
                var listQuyen = await _repository.WhereAsync<QT_Quyen>(o => o.IsDeleted == false);
                //Lấy danh sách chức năng quyền của vai trò
                var listChucNangQuyen = await _repository.WhereAsync<QT_VaiTroQuyen>(o => o.IsDeleted == false && o.VaiTroId == gId);

                roleRespond.listChucNangQuyen = listChucNangQuyen.Count > 0 ? (from chucNangQuyen in listChucNangQuyen
                                                                               join chucNang in listChucNang on chucNangQuyen.ChucNangId equals chucNang.Id
                                                                               join quyen in listQuyen on chucNangQuyen.QuyenId equals quyen.Id
                                                                               select new ChucNangQuyenRespondModel { ChucNangId = chucNang.Id, MaChucNang = chucNang.MaChucNang, QuyenId = quyen.Id, MaQuyen = quyen.MaQuyen }
                                                                               ).ToList() : null;
                return Ok(roleRespond);
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        public async Task<ServiceResponse> GetList(PaginatedInputModel paging)
        {
            try
            {
                string storeName = "Proc_AppRoles_GetList";
                var dbparams = await _dapper.AddParamAsync<PaginatedInputModel>(paging, storeName);
                var result = await _dapper.GetListAsync<RoleRespondModel>(storeName, dbparams, commandType: CommandType.StoredProcedure);
                return Ok(new PagedResultModel<RoleRespondModel> { Items = result, TotalCount = dbparams.Get<int>("TotalCount") });
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }
    }
}