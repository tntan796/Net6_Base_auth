using AutoMapper;
using DNBase.Common;
using DNBase.DataLayer.EF;
using DNBase.DataLayer.EF.Entities;
using DNBase.Services.Interfaces;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNBase.Services
{
    public class SystemService : ServiceBase, ISystemService
    {
        private readonly ILogger<ISystemService> _logger;
        private readonly IMapper _mapper;
        private readonly IGenericRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        //private readonly IRedisClient _redisClient;
        private readonly IMemoryCache _memoryCache;
        private MemoryCacheEntryOptions cacheOptions;

        public SystemService(ILogger<ISystemService> logger
            , IMapper mapper
            , IGenericRepository repository
            , IUnitOfWork unitOfWork
            , ICurrentPrincipal currentPrincipal
            , IHttpContextAccessor httpContextAccessor
            //, IRedisClient redisClient
            , IMemoryCache memoryCache) : base(currentPrincipal, httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _repository = repository;
            _unitOfWork = unitOfWork;
            //_redisClient = redisClient;
            _memoryCache = memoryCache;
            cacheOptions = new MemoryCacheEntryOptions();
            cacheOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(720);  //Cache sẽ hết hạn sau 12h
            cacheOptions.SlidingExpiration = TimeSpan.FromMinutes(60);       //Cache sẽ hết hạn nếu trong vòng 1h mà không có ai truy cập
        }

        public async Task<ServiceResponse> GetListCategoryItem(string code)
        {
            try
            {
                string keyCache = "dnbase_categoryitem_" + code;
                //var resultCache = _redisClient.Get<List<CategoryItemRespondModel>>(keyCache);
                //if (resultCache != null && resultCache.Count > 0)
                //{
                //    return Ok(new PagedResultModel<CategoryItemRespondModel> { Items = resultCache, TotalCount = resultCache.Count });
                //}
                if (_memoryCache.TryGetValue<List<CategoryItemRespondModel>>(keyCache, out List<CategoryItemRespondModel> resultCache))
                {
                    return Ok(new PagedResultModel<CategoryItemRespondModel> { Items = resultCache, TotalCount = resultCache != null ? resultCache.Count : 0 });
                }

                var category = await _repository.FistOrDefaultAsync<Category>(o => o.Code == code);
                var listCategoryItem = category != null ? await _repository.WhereAsync<CategoryItem>(o => o.CategoryId == category.Id) : null;
                if (listCategoryItem != null && listCategoryItem.Count > 0)
                {
                    var result = (from categoryItem in listCategoryItem
                                  select new CategoryItemRespondModel()
                                  {
                                      Id = categoryItem.Id,
                                      Code = categoryItem.Code,
                                      Name = categoryItem.Name,
                                      Description = categoryItem.Description,
                                      Order = categoryItem.Order,
                                      ParentId = categoryItem.ParentId,
                                      CategoryId = categoryItem.CategoryId
                                  }).ToList();

                    if (result != null && result.Count > 0)
                    {
                        //_redisClient.Add<List<CategoryItemRespondModel>>(keyCache, result);
                        _memoryCache.Set<List<CategoryItemRespondModel>>(keyCache, result, cacheOptions);
                        return Ok(new PagedResultModel<CategoryItemRespondModel> { Items = result, TotalCount = result.Count });
                    }
                }
                return Ok(new PagedResultModel<CategoryItemRespondModel> { Items = null, TotalCount = 0 });
            }
            catch (Exception oEx)
            {
                _logger.LogError(oEx.ToString());
                return BadRequest("", oEx.ToString());
            }
        }
    }
}