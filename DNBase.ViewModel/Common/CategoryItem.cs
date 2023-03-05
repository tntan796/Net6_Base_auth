using System;

namespace DNBase.ViewModel
{
    public class CategoryItemRespondModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? ParentId { get; set; }
        public int Order { get; set; } = 0;
    }
}