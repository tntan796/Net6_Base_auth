using System;

namespace DNBase.ViewModel
{
    public class AppLog_RespondtModel
    {
        public string RequestPath { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public Guid? UserId { get; set; }
        public string UserName { get; set; }
    }
}