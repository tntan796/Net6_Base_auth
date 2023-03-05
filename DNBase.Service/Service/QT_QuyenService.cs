using Dapper;
using DNBase.Common;
using DNBase.Common.Constants;
using DNBase.DataLayer.Dapper;
using DNBase.Services.Interfaces;
using DNBase.Validators;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DNBase.Services
{
    public class QT_QuyenService : ServiceBase, IQT_QuyenService
    {
        private readonly ILogger<IQT_QuyenService> _logger;
        private readonly IDapper _dapper;

        public QT_QuyenService(ILogger<IQT_QuyenService> logger
            , IDapper dapper
            , ICurrentPrincipal currentPrincipal
            , IHttpContextAccessor httpContextAccessor) : base(currentPrincipal, httpContextAccessor)
        {
            _logger = logger;
            _dapper = dapper;
        }

        public async Task<ServiceResponse> Create(QT_QuyenRequestModel model)
        {
            try
            {
                var validate = ValidateModel(model);
                if (!validate.Succeeded)
                {
                    return validate;
                }

                string storeName = "Proc_QT_Quyen_Create";
                var dbparams = await _dapper.AddParamAsync<QT_QuyenRequestModel>(model, storeName, _principal.UserId);
                await _dapper.ExecuteAsync(storeName, dbparams, commandType: CommandType.StoredProcedure);
                return Ok(model, "", MessageResponseCommon.CREATE_SUCCESSFULL);
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        public async Task<ServiceResponse> Update(QT_QuyenRequestModel model)
        {
            try
            {
                if (model.Id == null)
                {
                    return BadRequest("", MessageValidateCommon.NotFound);
                }
                var validate = ValidateModel(model);
                if (!validate.Succeeded)
                {
                    return validate;
                }

                string storeName = "Proc_QT_Quyen_Update";
                var dbparams = await _dapper.AddParamAsync<QT_QuyenRequestModel>(model, storeName, _principal.UserId);
                await _dapper.ExecuteAsync(storeName, dbparams, commandType: CommandType.StoredProcedure);
                return Ok(model, "", MessageResponseCommon.UPDATE_SUCCESSFULL);
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        public async Task<ServiceResponse> Delete(string id)
        {
            try
            {
                DynamicParameters dbparams = new DynamicParameters();
                dbparams.Add("id", Guid.Parse(id), DbType.Guid);
                await _dapper.ExecuteAsync("Proc_QT_Quyen_Delete", dbparams, commandType: CommandType.StoredProcedure);
                return Ok(id, "", MessageResponseCommon.DELETE_SUCCESSFULL);
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        public async Task<ServiceResponse> DeleteMany(List<Guid> listId)
        {
            try
            {
                DynamicParameters dbparams = new DynamicParameters();
                dbparams.Add("listId", listId.Count > 0 ? string.Join(";", listId) : "", DbType.String);
                await _dapper.ExecuteAsync("Proc_QT_Quyen_DeleteMany", dbparams, commandType: CommandType.StoredProcedure);
                return Ok(listId, "", MessageResponseCommon.DELETE_SUCCESSFULL);
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
                DynamicParameters dbparams = new DynamicParameters();
                dbparams.Add("id", Guid.Parse(id), DbType.Guid);
                var result = await _dapper.GetAsync<QT_QuyenRespondModel>("Proc_QT_Quyen_GetDetail", dbparams, commandType: CommandType.StoredProcedure);
                return Ok(result);
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
                string storeName = "Proc_QT_Quyen_GetList";
                var dbparams = await _dapper.AddParamAsync<PaginatedInputModel>(paging, storeName);
                var result = await _dapper.GetListAsync<QT_QuyenRespondModel>(storeName, dbparams, commandType: CommandType.StoredProcedure);
                return Ok(new PagedResultModel<QT_QuyenRespondModel> { Items = result, TotalCount = dbparams.Get<int>("TotalCount") });
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        private ServiceResponse ValidateModel(QT_QuyenRequestModel requestModel)
        {
            var resultOrderInfoValidator = new QT_QuyenValidators().Validate(requestModel);
            if (!resultOrderInfoValidator.IsValid)
            {
                return BadRequest("", JsonConvert.SerializeObject(resultOrderInfoValidator.Errors.Select(x => x.ErrorMessage)));
            }
            return Ok();
        }
    }
}