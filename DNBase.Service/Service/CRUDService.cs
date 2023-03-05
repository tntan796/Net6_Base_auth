using AutoMapper;
using Dapper;
using DNBase.DataLayer.Dapper;
using DNBase.Services.Interfaces;
using DNBase.ViewModel;
using DNBase.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DNBase.Common.Constants;
using DNBase.Common;

namespace DNBase.Services
{
    public class CRUDService : ServiceBase, ICRUDService
    {
        protected readonly ILogger<ICRUDService> _logger;
        private readonly IDapper _dapper;
        private readonly IMapper _mapper;

        public CRUDService(ILogger<ICRUDService> logger,
                                      IDapper dapper,
                                      IMapper mapper,
                                      IHttpContextAccessor httpContextAccessor,
                                      ICurrentPrincipal currentPrincipal) : base(currentPrincipal, httpContextAccessor)
        {
            _logger = logger;
            _dapper = dapper;
            _mapper = mapper;
        }

        public async Task<ServiceResponse> Create(CRUDRequestModel model)
        {
            try
            {
                var validate = ValidateModel(model);
                if (!validate.Succeeded)
                    return validate;

                string storeName = "Proc_CRUD_Create";
                var dbparams = await _dapper.AddParamAsync<CRUDRequestModel>(model, storeName, _principal.UserId);
                await _dapper.ExecuteAsync(storeName, dbparams, commandType: CommandType.StoredProcedure);
                return Ok(model, "", MessageResponseCommon.CREATE_SUCCESSFULL);
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        public async Task<ServiceResponse> Update(CRUDRequestModel model)
        {
            try
            {
                if (model.Id == null)
                {
                    return BadRequest("", MessageValidateCommon.NotFound);
                }
                var validate = ValidateModel(model);
                if (!validate.Succeeded)
                    return validate;

                string storeName = "Proc_CRUD_Update";
                var dbparams = await _dapper.AddParamAsync<CRUDRequestModel>(model, storeName, _principal.UserId);
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
                await _dapper.ExecuteAsync("Proc_CRUD_Delete", dbparams, commandType: CommandType.StoredProcedure);
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
                await _dapper.ExecuteAsync("Proc_CRUD_DeleteMany", dbparams, commandType: CommandType.StoredProcedure);
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
                var result = await _dapper.GetAsync<CRUDRequestModel>("Proc_CRUD_GetDetail", dbparams, commandType: CommandType.StoredProcedure);
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
                string storeName = "Proc_CRUD_GetList";
                var dbparams =  await _dapper.AddParamAsync<PaginatedInputModel>(paging, storeName);
                var result = await _dapper.GetListAsync<CRUDRequestModel>(storeName, dbparams, commandType: CommandType.StoredProcedure);
                return Ok(new PagedResultModel<CRUDRequestModel> { Items = result, TotalCount = dbparams.Get<int>("TotalCount") });
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        private ServiceResponse ValidateModel(CRUDRequestModel requestModel)
        {
            var resultOrderInfoValidator = new CRUDValidators().Validate(requestModel);
            if (!resultOrderInfoValidator.IsValid)
            {
                return BadRequest("", JsonConvert.SerializeObject(resultOrderInfoValidator.Errors.Select(x => x.ErrorMessage)));
            }
            return Ok();
        }
    }
}