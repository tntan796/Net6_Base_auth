using DNBase.Common.Constants;
using DNBase.ViewModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DNBase.Common
{
    public interface IJwtHandler
    {
        string GenerateAccessToken(Guid userId, string username, object permission);

        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromToken(string token);
    }

    public class JwtHandler : IJwtHandler
    {
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly ILogger<JwtHandler> _logger;
        private readonly JwtOptionsModel _options;

        public JwtHandler(ILogger<JwtHandler> logger, IOptions<JwtOptionsModel> options)
        {
            _jwtSecurityTokenHandler ??= new JwtSecurityTokenHandler();
            _logger = logger;
            _options = options.Value;
        }

        public string GenerateAccessToken(Guid userId, string username, object permission)
        {
            var claims = new[] {
                new Claim(Claims.UserId, userId.ToString(), ClaimValueTypes.String),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(Claims.Permissions, Newtonsoft.Json.JsonConvert.SerializeObject(permission)),
            };
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)), SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_options.ExpiresInMinutes),
                SigningCredentials = signingCredentials
            };

            return _jwtSecurityTokenHandler.WriteToken(_jwtSecurityTokenHandler.CreateToken(tokenDescriptor));
        }

        public string GenerateRefreshToken()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();

            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
                    ValidateLifetime = true
                };
                var principal = _jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (Exception e)
            {
                _logger.LogError("Token validation failed: {@e}", e);

                return null;
            }
        }
    }
}