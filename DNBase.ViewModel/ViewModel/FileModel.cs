using System;

namespace DNBase.ViewModel
{
    public class FileModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public double Size { get; set; }
        public string Path { get; set; }
        public Guid EntityId { get; set; }
        public string EntityName { get; set; }
        public string FileTypeUpload { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class FileResponseModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
    }
}
