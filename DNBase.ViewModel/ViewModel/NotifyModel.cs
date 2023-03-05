using System;
using System.ComponentModel.DataAnnotations;

namespace DNBase.ViewModel
{
    public class NotifyModel
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
    }
}