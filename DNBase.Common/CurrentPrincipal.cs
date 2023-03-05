using DNBase.Common.Constants;
using DNBase.ViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace DNBase.Common
{
    public interface ICurrentPrincipal
    {
        PrincipalModel Principal { get; }

        PrincipalModel PopulatePrincipal();
    }

    public class CurrentPrincipal : ICurrentPrincipal
    {
        private readonly IHttpContextAccessor _context;
        private readonly IJwtHandler _jwtHandler;

        public CurrentPrincipal(IHttpContextAccessor context , IJwtHandler jwtHandler)
        {
            _context = context;
            _jwtHandler = jwtHandler;
        }

        public PrincipalModel Principal { get => PopulatePrincipal(); }

        public PrincipalModel PopulatePrincipal()
        {
            try
            {
                if (_context.HttpContext == null)
                    return null;
                ClaimsPrincipal claims = _jwtHandler.GetPrincipalFromToken(_context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty));
                if (claims == null)
                    return null;

                Guid.TryParse(claims.Claims.FirstOrDefault(c => c.Type == Claims.UserId)?.Value, out Guid userId);
                var userName = claims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

                return new PrincipalModel
                {
                    UserId = userId,
                    UserName = userName
                };
            }
            catch
            {
                return null;
            }
        }
    }
}