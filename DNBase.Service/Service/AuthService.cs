using AutoMapper;
using DNBase.Common;
using DNBase.Common.Constants;
using DNBase.DataLayer.EF;
using DNBase.DataLayer.EF.Entities;
using DNBase.Services.Interfaces;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DNBase.Services
{
    public class AuthService<TUser> : ServiceBase, IAuthService where TUser : AppUser
    {
        private readonly ILogger<IAuthService> _logger;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IJwtHandler _jwtHandler;
        private readonly IAuthLDValidator _authValidator;
        private readonly AuthConfigModel _authConfig;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IGenericRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(ILogger<IAuthService> logger
             , IMapper mapper
             , IConfiguration config
             , IJwtHandler jwtHandler
             , IAuthLDValidator authValidator
             , IOptions<AuthConfigModel> authConfig
             , UserManager<AppUser> userManager
             , IEmailService emailService
             , IGenericRepository repository
             , IUnitOfWork unitOfWork
             , ICurrentPrincipal currentPrincipal
             , IHttpContextAccessor httpContextAccessor) : base(currentPrincipal, httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _config = config;
            _jwtHandler = jwtHandler;
            _authValidator = authValidator;
            _authConfig = authConfig.Value;
            _userManager = userManager;
            _emailService = emailService;
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse> Login(AuthRequestModel request)
        {
            var user = await _repository.FistOrDefaultAsync<AppUser>(u => u.UserName == request.UserName);
            if (user == null)
            {
                return BadRequest("login_failure", "Invalid username or password.");
            }
            //validate against ldap
            if (bool.Parse(_config.GetSection(AppsettingConstants.VALIDATE_LDAP).Value))
            {
                var authenticated = _authValidator.ValidateUserLive(request.UserName, request.Password, _authConfig.LdapAddress1, _authConfig.LdapAddress2);
                if (!authenticated)
                {
                    return BadRequest("login_failure", "Invalid username or password.");
                }
            }
            else if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return BadRequest("login_failure", "Invalid username or password.");
            }
            var refreshToken = _jwtHandler.GenerateRefreshToken();
            _repository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new AuthResponseModel
            {
                AccessToken = _jwtHandler.GenerateAccessToken(user.Id, user.UserName, null),
                RefreshToken = refreshToken
            });
        }

        public async Task<ServiceResponse> Logout()
        {
            return Ok(null);
        }

        public async Task<ServiceResponse> RefreshToken(RefreshTokenRequest request)
        {
            var failedResponse = BadRequest("refresh_token_failure", "Invalid token.");
            var claimsPrincipal = _jwtHandler.GetPrincipalFromToken(request.AccessToken);
            if (claimsPrincipal == null)
            {
                return failedResponse;
            }

            var userIdClaim = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var user = await _repository.FistOrDefaultAsync<AppUser>(x => x.Id == new Guid(userIdClaim.Value));

            return Ok(new AuthResponseModel
            {
                AccessToken = _jwtHandler.GenerateAccessToken(user.Id, user.UserName, null)
            });
        }

        public async Task<ServiceResponse> ForgotPassword(ForgotPasswordRequestModel model, string origin)
        {
            var account = await _repository.FistOrDefaultAsync<AppUser>(x => x.Email == model.Email);

            // always return ok response to prevent email enumeration
            if (account == null) return BadRequest("Không tồn tại email này trong hệ thống.");

            // create reset token that expires after 1 day
            account.ResetToken = RandomTokenString();
            account.ResetTokenExpires = DateTime.UtcNow.AddDays(1);

            _repository.Update(account);
            await _unitOfWork.SaveChangesAsync();

            // send email
            SendPasswordResetEmail(account, origin);
            return Ok();
        }

        public async Task<ServiceResponse> ResetPassword(ResetPasswordRequestModel model)
        {
            if (model.Password != model.ConfirmPassword) return BadRequest("Mật khẩu comfirn không khớp.");
            var user = _repository.FistOrDefault<AppUser>(x => x.ResetToken == model.Token && x.ResetTokenExpires > DateTime.UtcNow);
            if (user == null) return BadRequest("InvalidToken");

            user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, model.Password);
            user.ResetToken = null;
            user.ResetTokenExpires = null;
            _repository.Update<AppUser>(user);

            await _unitOfWork.SaveChangesAsync();
            return Ok(true);
        }

        public async Task<ServiceResponse> ChangePass(ChangePassReuqestModel model)
        {
            var user = _repository.FistOrDefault<AppUser>(o => o.UserName == model.UserName);
            if (user == null) return BadRequest("", "Tài khoản không tồn tại.");
            if (await _userManager.CheckPasswordAsync(user, model.PasswordOld)) return BadRequest("", "Mật khẩu cũ không đúng.");
            if (model.Password != model.RepeatPassword) return BadRequest("", "Mật khẩu comfirn không khớp.");

            await _userManager.ChangePasswordAsync(user, model.PasswordOld, model.Password);
            //user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, model.Password);
            //user.SecurityStamp = Guid.NewGuid().ToString();
            _repository.Update<AppUser>(user);

            await _unitOfWork.SaveChangesAsync();

            return Ok(user);
        }

        #region private method

        private string RandomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        private void SendPasswordResetEmail(AppUser account, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var resetUrl = $"{origin}/account/reset-password?token={account.ResetToken}";
                message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
                             <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to reset your password with the <code>/accounts/reset-password</code> api route:</p>
                             <p><code>{account.ResetToken}</code></p>";
            }

            _emailService.Send(
                to: account.Email,
                subject: "Sign-up Verification API - Reset Password",
                html: $@"<h4>Reset Password Email</h4>
                         {message}"
            );
        }

        #endregion private method
    }
}