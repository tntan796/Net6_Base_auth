using System;
using System.Threading.Tasks;
using DNBase.ViewModel;

namespace DNBase.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ServiceResponse> Push(NotifyModel notify);
    }
}