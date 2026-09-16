using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace VDCD.Business.Helper
{
	public class CurrentUserService : ICurrentUserService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public CurrentUserService(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User;

		public int? UserId
		{
			get
			{
				var id = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
					  ?? User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

				return int.TryParse(id, out var result) ? result : null;
			}
		}

		public string Username =>
			User?.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;

		public string Email =>
			User?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
	}
}
