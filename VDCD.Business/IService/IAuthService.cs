using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDCD.Entities.DTO.Request;
using VDCD.Entities.DTO.Response;

namespace VDCD.Business.IService
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<SsoLoginResult> LoginSSOAsync(SsoLoginRequest request);
        Task LogoutAsync(string? refreshToken);
    }
}
