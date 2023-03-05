using System;
using AutoMapper;
using System.Data;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DNBase.DataLayer.EF.Mongo.Entities;
using System.Linq;
using DNBase.Services.Interfaces;
using DDTH.DataLayer.MongoProvider;
using MongoDB.Driver;
using DNBase.Validators;
using Newtonsoft.Json;
using System.Threading.Tasks;
using DNBase.Common.Constants;
using DNBase.Common;

namespace DNBase.Services
{
    public class CRUD_MGService : ServiceBase, ICRUD_MGService
    {
        protected readonly ILogger<ICRUD_MGService> _logger;
        private readonly IMapper _mapper;
        private readonly IMongo _mongoProvider;

        public CRUD_MGService(ILogger<ICRUD_MGService> logger,
                                        IMapper mapper,
                                        IHttpContextAccessor httpContextAccessor,
                                        ICurrentPrincipal currentPrincipal,
                                        IMongo mongoProvider) : base(currentPrincipal, httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _mongoProvider = mongoProvider;
        }

        public async Task<ServiceResponse> Create(CRUD_MGRequestModel model)
        {
            try
            {
                var validate = ValidateModel(model);
                if (!validate.Succeeded)
                    return validate;

                CRUD_MG entity = _mapper.Map<CRUD_MG>(model);
                await _mongoProvider.GetCollection<CRUD_MG>().InsertOneAsync(entity);
                return Ok(model, "", MessageResponseCommon.CREATE_SUCCESSFULL);
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        public async Task<ServiceResponse> Update(CRUD_MGRequestModel model)
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

                var entity = _mongoProvider.GetCollection<CRUD_MG>().Find(o => o.IsDeleted == false && o.Id == model.Id).FirstOrDefault();
                _mapper.Map<CRUD_MGRequestModel, CRUD_MG>(model, entity);
                await _mongoProvider.GetCollection<CRUD_MG>().ReplaceOneAsync(o => o.Id == model.Id, entity);
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
                await _mongoProvider.GetCollection<CRUD_MG>().DeleteOneAsync(x => x.Id == id);
                return Ok(id, "", MessageResponseCommon.DELETE_SUCCESSFULL);
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
                var result = await _mongoProvider.GetCollection<CRUD_MG>().Find(o => o.IsDeleted == false && o.Id == id).FirstOrDefaultAsync();
                CRUD_MGRespondModel resultModel = new CRUD_MGRespondModel()
                {

                };
                return Ok(resultModel);
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
                var result = await _mongoProvider.GetCollection<CRUD_MG>().Find(o => o.IsDeleted == false).SortByDescending(o => o.CreatedAt).Skip((paging.PageIndex - 1) * paging.PageSize).Limit(paging.PageSize).ToListAsync();
                var resultModel = (from model in result
                                   select new CRUD_MGRespondModel()
                                   {

                                   }).ToList();
                return Ok(new PagedResultModel<CRUD_MGRespondModel> { Items = resultModel, TotalCount = resultModel.Count });
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }

        private ServiceResponse ValidateModel(CRUD_MGRequestModel requestModel)
        {
            var resultOrderInfoValidator = new CRUD_MGValidators().Validate(requestModel);
            if (!resultOrderInfoValidator.IsValid)
            {
                return BadRequest("", JsonConvert.SerializeObject(resultOrderInfoValidator.Errors.Select(x => x.ErrorMessage)));
            }
            return Ok();
        }
    }
}