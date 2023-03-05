using System;
using System.Collections.Generic;

namespace DNBase.ViewModel
{
    public class CRUD_MGRequestModel 
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string Content { get; set; }
        public int Status { get; set; }
        public List<Guid> OrderIds { get; set; }
    }
    public class CRUD_MGRespondModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string Content { get; set; }
        public int Status { get; set; }
        public List<Guid> OrderIds { get; set; }
    }
}
