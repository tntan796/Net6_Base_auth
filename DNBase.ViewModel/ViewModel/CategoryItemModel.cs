using System;
using System.Collections.Generic;

namespace DNBase.ViewModel
{
    public class CategoryItemResponseModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? ParentId { get; set; }
        public List<CategoryItemResponseModel> Children { get; set; }
        public int Order { get; set; } = 0;
        public DateTime? CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }

    public class CategoryItemCreateRequestModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? ParentId { get; set; }
        public int Order { get; set; } = 0;
    }

    public class CategoryCreateRequestModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public int Order { get; set; } = 0;
    }

    public class CategoryResponseModel
    {
        public CategoryResponseModel()
        {
            CategoryItems = new List<CategoryItemResponseModel>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int Order { get; set; } = 0;
        public DateTime? CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public List<CategoryItemResponseModel> CategoryItems { get; set; }
    }

    public class CategoryHierarchicalSearchRequest
    {
        public List<string> LstCategoryCode { get; set; }
    }
}