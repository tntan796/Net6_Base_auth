using AutoMapper;
using DNBase.Common;
using DNBase.Common.Constants;
using DNBase.DataLayer.Dapper;
using DNBase.DataLayer.EF;
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

namespace DNBase.Services
{
    public class QT_UserService<TUser> : ServiceBase, IQT_UserService where TUser : AppUser
    {
        private readonly ILogger<ISystemService> _logger;
        private readonly IMapper _mapper;
        private readonly IDapper _dapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IGenericRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public QT_UserService(ILogger<ISystemService> logger
            , IMapper mapper
            , IDapper dapper
            , UserManager<AppUser> userManager
            , IGenericRepository repository
            , IUnitOfWork unitOfWork
            , ICurrentPrincipal currentPrincipal
            , IHttpContextAccessor httpContextAccessor) : base(currentPrincipal, httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _dapper = dapper;
            _userManager = userManager;
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse> Create(UserCreateRequestModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.UserName))
                {
                    return BadRequest("", String.Format(MessageValidateCommon.Required, "Tên đăng nhập"));
                }
                if (string.IsNullOrEmpty(model.Email))
                {
                    return BadRequest("", String.Format(MessageValidateCommon.Required, "Email"));
                }
                if (string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest("", String.Format(MessageValidateCommon.Required, "Mật khẩu"));
                }
                AppUser user = _mapper.Map<AppUser>(model);
                user.Id = Guid.NewGuid();
                await _userManager.CreateAsync(user, model.Password);
                //Gắn người dùng với vai trò
                if (model.listRole.Count > 0)
                {
                    foreach (Guid roleId in model.listRole)
                    {
                        QT_VaiTro_NguoiSuDung qT_VaiTro_NguoiSuDung = new QT_VaiTro_NguoiSuDung() { NguoiDungId = user.Id, VaiTroId = roleId, CreatedBy = _principal.UserId };
                        await _repository.AddAsync<QT_VaiTro_NguoiSuDung>(qT_VaiTro_NguoiSuDung);
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

        public async Task<ServiceResponse> Update(UserCreateRequestModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.UserName))
                {
                    return BadRequest("", String.Format(MessageValidateCommon.Required, "Tên đăng nhập"));
                }
                if (string.IsNullOrEmpty(model.Email))
                {
                    return BadRequest("", String.Format(MessageValidateCommon.Required, "Email"));
                }
                if (string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest("", String.Format(MessageValidateCommon.Required, "Mật khẩu"));
                }
                var user = await _repository.FistOrDefaultAsync<AppUser>(o => o.UserName == model.UserName && o.IsDeleted == false);

                if (user == null)
                {
                    return BadRequest("", MessageValidateCommon.NotFound);
                }
                _mapper.Map<UserCreateRequestModel, AppUser>(model, user);

                _repository.Update<AppUser>(user);

                //Cập nhật gắn người dùng với vai trò
                if (model.listRole.Count > 0)
                {
                    var listRole = _repository.Where<QT_VaiTro_NguoiSuDung>(o => o.NguoiDungId == user.Id);
                    if (listRole != null && listRole.Count > 0)
                    {
                        listRole.ForEach(o => o.IsDeleted = true);
                    }
                    foreach (Guid roleId in model.listRole)
                    {
                        QT_VaiTro_NguoiSuDung qT_VaiTro_NguoiSuDung = await _repository.FistOrDefaultAsync<QT_VaiTro_NguoiSuDung>(o => o.NguoiDungId == user.Id && o.VaiTroId == roleId);
                        if (qT_VaiTro_NguoiSuDung != null)
                        {//Nếu là cập nhật
                            qT_VaiTro_NguoiSuDung.IsDeleted = false;
                            qT_VaiTro_NguoiSuDung.UpdatedBy = _principal.UserId;
                            _repository.Update<QT_VaiTro_NguoiSuDung>(qT_VaiTro_NguoiSuDung);
                        }
                        else
                        {//Nếu là thêm mới
                            qT_VaiTro_NguoiSuDung = new QT_VaiTro_NguoiSuDung() { NguoiDungId = user.Id, VaiTroId = roleId, CreatedBy = _principal.UserId };
                            await _repository.AddAsync<QT_VaiTro_NguoiSuDung>(qT_VaiTro_NguoiSuDung);
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

        public ServiceResponse GetDetail(string id)
        {
            try
            {
                var userId = String.IsNullOrEmpty(id) ? _principal.UserId : Guid.Parse(id);
                var user = _repository.FistOrDefault<AppUser>(u => u.Id == userId);
                if (user == null)
                {
                    return BadRequest("", "not found.");
                }
                UserRespondModel userRespond = _mapper.Map<UserRespondModel>(user);
                //Lấy danh mục vai trò
                var listRole = _repository.Where<AppRole>(o => o.IsDeleted == false).Select(o => new { o.Id, o.Name }).ToList();
                //Lấy danh sách vai trò của người dùng
                var listRoleUser = _repository.Where<QT_VaiTro_NguoiSuDung>(o => o.IsDeleted == false && o.NguoiDungId == user.Id).Select(o => o.VaiTroId).ToList();
                //Lấy danh mục quyền
                var listPermission = _repository.Where<QT_Quyen>(o => o.IsDeleted == false).Select(o => new { o.Id, o.MaQuyen }).ToList();
                //Lấy danh sách quyền của người dùng
                var listRolePermission = _repository.Where<QT_VaiTroQuyen>(o => o.IsDeleted == false && listRoleUser.Contains(o.VaiTroId)).Select(o => o.QuyenId).ToList();
                //Lấy danh mục giới tính
                var categoryGioiTinh = _repository.FistOrDefault<Category>(o => o.Code == "gioitinh");
                var categoryGioiTinhItem = categoryGioiTinh != null ? _repository.Where<CategoryItem>(o => o.CategoryId == categoryGioiTinh.Id).ToList() : null;

                userRespond.GioiTinh = user.GioiTinh != null && categoryGioiTinhItem != null ? categoryGioiTinhItem.FirstOrDefault(o => o.Code == user.GioiTinh).Name : "";
                userRespond.DanhSachVaiTro = listRoleUser.Count > 0 ? (from userRole in listRoleUser join role in listRole on userRole equals role.Id select role.Name).ToList() : null;
                userRespond.DanhSachQuyen = listRolePermission.Count > 0 ? (from userPermission in listRolePermission join permission in listPermission on userPermission equals permission.Id select permission.MaQuyen).ToList() : null;

                return Ok(userRespond);
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
                string storeName = "Proc_AppUsers_GetList";
                var dbparams = await _dapper.AddParamAsync<PaginatedInputModel>(paging, storeName);
                var result = await _dapper.GetListAsync<UserRespondModel>(storeName, dbparams, commandType: CommandType.StoredProcedure);
                return Ok(new PagedResultModel<UserRespondModel> { Items = result, TotalCount = dbparams.Get<int>("TotalCount") });
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }
    }
}