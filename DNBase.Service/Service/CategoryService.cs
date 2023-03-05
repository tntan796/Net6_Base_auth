using AutoMapper;
using DNBase.Common;
using DNBase.DataLayer.EF;
using DNBase.DataLayer.EF.Entities;
using DNBase.Services.Interfaces;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNBase.Services
{
    public class CategoryService : ServiceBase, ICategoryService
    {
        private readonly IGenericRepository _repository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(
            IGenericRepository repository,
            IMapper mapper,
            IConfiguration config,
            IUnitOfWork unitOfWork
            , ICurrentPrincipal currentPrincipal
            , IHttpContextAccessor httpContextAccessor) : base(currentPrincipal, httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _config = config;
            _unitOfWork = unitOfWork;
        }

        public ServiceResponse GetListCategory()
        {
            var lstCategoryItems = _repository
                .Where<CategoryItem>(x => x.IsDeleted == false);
            var result = (_repository
                .Where<Category>(x => x.IsDeleted == false))
               .OrderBy(x => x.Order).ToList().Select(x => new CategoryResponseModel
               {
                   Id = x.Id,
                   Code = x.Code,
                   Name = x.Name,
                   Order = x.Order,
                   CreatedAt = x.CreatedAt,
                   CreatedBy = x.CreatedBy,
                   UpdatedAt = x.UpdatedAt,
                   UpdatedBy = x.UpdatedBy,
                   CategoryItems = lstCategoryItems.Where(c => c.CategoryId == x.Id).OrderBy(c => c.Order).Select(y => new CategoryItemResponseModel
                   {
                       Id = y.Id,
                       CategoryId = y.CategoryId,
                       Code = y.Code,
                       Name = y.Name,
                       Order = y.Order,
                       Description = y.Description,
                       ParentId = y.ParentId,
                       CreatedAt = y.CreatedAt,
                       CreatedBy = y.CreatedBy,
                       UpdatedAt = y.UpdatedAt,
                       UpdatedBy = y.UpdatedBy
                   }).ToList()
               });
            return Ok(result);
        }

        public async Task<ServiceResponse> CreateCategory(CategoryCreateRequestModel model)
        {
            model.Id = Guid.NewGuid();
            var category = _mapper.Map<Category>(model);
            await _repository.AddAsync<Category>(category);
            await _unitOfWork.SaveChangesAsync();
            return Ok(model, "", "Thêm mới danh mục thành công.");
        }

        public async Task<ServiceResponse> UpdateCategory(CategoryCreateRequestModel model)
        {
            if (!model.Id.HasValue)
            {
                return BadRequest("", "Bạn chưa chọn danh mục sửa.");
            }
            var entity = await _repository.FistOrDefaultAsync<Category>(x => x.Id == model.Id.Value);
            entity.Name = model.Name;
            entity.Order = model.Order;
            entity.Code = model.Code;
            _repository.Update<Category>(entity);
            await _unitOfWork.SaveChangesAsync();
            return Ok(model, "", "Cập nhật danh mục thành công.");
        }

        public async Task<ServiceResponse> GetHierarchical(CategoryHierarchicalSearchRequest model)
        {
            List<CategoryResponseModel> lstCategoryReponse = new List<CategoryResponseModel>();
            var lstCategory = await _repository.WhereAsync<Category>(x => (model != null && model.LstCategoryCode != null) ? model.LstCategoryCode.Contains(x.Code) : 1 == 1);
            var lstCategoryItem = await _repository.WhereAsync<CategoryItem>(x => lstCategory.Select(c => c.Id).Contains(x.CategoryId));
            foreach (var item in lstCategory.OrderBy(x => x.Order))
            {
                var itemInCategory = lstCategoryItem.OrderBy(x => x.Order).Where(x => x.CategoryId == item.Id).Select(y => new CategoryItemResponseModel
                {
                    Id = y.Id,
                    CategoryId = y.CategoryId,
                    Code = y.Code,
                    Name = y.Name,
                    Order = y.Order,
                    Description = y.Description,
                    ParentId = y.ParentId,
                    CreatedAt = y.CreatedAt,
                    CreatedBy = y.CreatedBy,
                    UpdatedAt = y.UpdatedAt,
                    UpdatedBy = y.UpdatedBy
                }).ToList();
                var categoryResponse = new CategoryResponseModel
                {
                    Id = item.Id,
                    Code = item.Code,
                    CreatedAt = item.CreatedAt,
                    CreatedBy = item.CreatedBy,
                    Name = item.Name,
                    Order = item.Order,
                    UpdatedAt = item.UpdatedAt,
                    UpdatedBy = item.UpdatedBy
                };
                foreach (var itemC in itemInCategory)
                {
                    GetTreeCategoryItem(lstCategoryItem, itemC, out CategoryItemResponseModel categoryItemResponse);
                    categoryResponse.CategoryItems.Add(categoryItemResponse);
                }
                lstCategoryReponse.Add(categoryResponse);
            }
            return Ok(lstCategoryReponse);
        }

        public async Task<ServiceResponse> GetTreeItemGroupByCategory(CategoryHierarchicalSearchRequest model)
        {
            Dictionary<string, List<CategoryItemResponseModel>> pairs = new Dictionary<string, List<CategoryItemResponseModel>>();
            var lstCategory = await _repository.WhereAsync<Category>(x => (model != null && model.LstCategoryCode != null) ? model.LstCategoryCode.Contains(x.Code) : 1 == 1);
            var lstCategoryItem = await _repository.WhereAsync<CategoryItem>(x => lstCategory.Select(c => c.Id).Contains(x.CategoryId));
            foreach (var item in lstCategory.OrderBy(x => x.Order))
            {
                var itemInCategory = lstCategoryItem.Where(x => x.CategoryId == item.Id).OrderBy(x => x.Order).Select(y => new CategoryItemResponseModel
                {
                    Id = y.Id,
                    CategoryId = y.CategoryId,
                    Code = y.Code,
                    Name = y.Name,
                    Order = y.Order,
                    Description = y.Description,
                    ParentId = y.ParentId,
                    CreatedAt = y.CreatedAt,
                    CreatedBy = y.CreatedBy,
                    UpdatedAt = y.UpdatedAt,
                    UpdatedBy = y.UpdatedBy
                }).ToList();
                var lstItemResult = new List<CategoryItemResponseModel>();
                foreach (var itemC in itemInCategory)
                {
                    GetTreeCategoryItem(lstCategoryItem, itemC, out CategoryItemResponseModel categoryItemResponse);

                    lstItemResult.Add(categoryItemResponse);
                }
                pairs.Add(item.Code, lstItemResult);
            }
            return Ok(pairs);
        }

        public void GetTreeCategoryItem(List<CategoryItem> lstCategoryItem, CategoryItemResponseModel item, out CategoryItemResponseModel categoryItemResponse)
        {
            var listUnitChilds = lstCategoryItem.Where(x => x.ParentId == item.Id).OrderBy(x => x.Order);
            if (listUnitChilds != null)
            {
                item.Children = listUnitChilds.Select(y => new CategoryItemResponseModel
                {
                    Id = y.Id,
                    CategoryId = y.CategoryId,
                    Code = y.Code,
                    Name = y.Name,
                    Order = y.Order,
                    Description = y.Description,
                    ParentId = y.ParentId,
                    CreatedAt = y.CreatedAt,
                    CreatedBy = y.CreatedBy,
                    UpdatedAt = y.UpdatedAt,
                    UpdatedBy = y.UpdatedBy
                }).ToList();
                foreach (var children in item.Children)
                {
                    GetTreeCategoryItem(lstCategoryItem, children, out categoryItemResponse);
                }
            }
            categoryItemResponse = item;
        }

        public async Task<ServiceResponse> CreateCategoryItem(CategoryItemCreateRequestModel model)
        {
            model.Id = Guid.NewGuid();
            var categoryItem = _mapper.Map<CategoryItem>(model);
            await _repository.AddAsync<CategoryItem>(categoryItem);
            await _unitOfWork.SaveChangesAsync();
            return Ok(model, "", "Thêm mới danh mục thành công.");
        }

        public async Task<ServiceResponse> UpdateCategoryItem(CategoryItemCreateRequestModel model)
        {
            if (!model.Id.HasValue)
            {
                return BadRequest("", "Bạn chưa chọn danh mục sửa.");
            }
            var entity = await _repository.FistOrDefaultAsync<CategoryItem>(x => x.Id == model.Id.Value);
            entity.Name = model.Name;
            entity.Order = model.Order;
            entity.Code = model.Code;
            entity.Description = model.Description;
            entity.CategoryId = model.CategoryId;
            entity.ParentId = model.ParentId;
            _repository.Update<CategoryItem>(entity);
            await _unitOfWork.SaveChangesAsync();
            return Ok(model, "", "Cập nhật danh mục thành công.");
        }
    }
}