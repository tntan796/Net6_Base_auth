using DNBase.ViewModel;
using System.Threading.Tasks;

namespace DNBase.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse> Login(AuthRequestModel request);

        Task<ServiceResponse> Logout();

        Task<ServiceResponse> RefreshToken(RefreshTokenRequest request);

        Task<ServiceResponse> ForgotPassword(ForgotPasswordRequestModel model, string origin);

        Task<ServiceResponse> ResetPassword(ResetPasswordRequestModel model);

        Task<ServiceResponse> ChangePass(ChangePassReuqestModel model);
    }
}