using DNBase.Services.Interfaces;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DNBase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ServiceResponse> Login(LoginModel model) => await _authService.Login(new AuthRequestModel
        {
            Password = model.Password,
            UserName = model.UserName,
            //ExtraProps = model.ExtraProps
            //RemoteIpAddress = Request.Headers.ContainsKey("X-Forwarded-For") ? Request.Headers["X-Forwarded-For"].ToString() : HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
        });

        [HttpPost("RefreshToken")]
        [AllowAnonymous]
        public async Task<ServiceResponse> RefreshToken(RefreshTokenModel model) => await _authService.RefreshToken(new RefreshTokenRequest
        {
            AccessToken = model.AccessToken,
            RefreshToken = model.RefreshToken
            //RemoteIpAdderss = Request.Headers.ContainsKey("X-Forwarded-For") ? Request.Headers["X-Forwarded-For"].ToString() : HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
        });

        [HttpGet("Logout")]
        [Authorize]
        public async Task<ServiceResponse> Logout() => await _authService.Logout();

        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<ServiceResponse> ForgotPassword(ForgotPasswordRequestModel model) => await _authService.ForgotPassword(model, Request.Headers["origin"]);

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<ServiceResponse> ResetPassword(ResetPasswordRequestModel model) => await _authService.ResetPassword(model);

        [HttpPost("ChangePass")]
        [Authorize]
        public async Task<ServiceResponse> ChangePass(ChangePassReuqestModel model) => await _authService.ChangePass(model);
    }
}