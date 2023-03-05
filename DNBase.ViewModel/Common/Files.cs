using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace DNBase.ViewModel
{
    public class FilesRequestModel
    {
        public Guid Id { get; set; }
        public String Name { get; set; }
        public String Extension { get; set; }
        public Decimal Size { get; set; }
        public String Path { get; set; }
        public Guid? EntityId { get; set; }
        public Int32 EntityType { get; set; }
        public String EntityName { get; set; }
    }

    public class FilesResponeModel
    {
        public Guid Id { get; set; }
        public String Name { get; set; }
        public String Extension { get; set; }
        public Decimal Size { get; set; }
        public String Path { get; set; }
        public Guid? EntityId { get; set; }
        public int? EntityId_DM { get; set; }
        public Int32 EntityType { get; set; }
        public String EntityName { get; set; }
    }

    public class FileAttachResponseModel
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
    }

    public class UploadFileRequestModel
    {
        public Guid? EntityId { get; set; }
        public string FileTypeUpload { get; set; }
        public string EntityName { get; set; }
        public string FileName { get; set; }
        public bool IsPrivate { get; set; } = false;
    }

    public class UploadFileMutileRequestModel
    {
        public IList<IFormFile> Files { get; set; }
        public string JsonLstUploadRequestInfo { get; set; }
    }

    public class DeleteFileUploadModel
    {
        public Guid EntityId { get; set; }
        public string EntityType { get; set; }
        public string FilePath { get; set; }
    }

    public class GetListFileForEntityModel
    {
        public Guid EntityId { get; set; }
        public string EntityName { get; set; }
        public string FileTypeUpload { get; set; }
    }
}