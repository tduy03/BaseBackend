using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDCD.Entities.DTO.Response
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public UserInfoResponse User { get; set; } = new();
    }

    public class UserInfoResponse
    {
        public long Id { get; set; }

        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
    }

    public class SsoLoginResult
    {
        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public UserInfoResponse User { get; set; } = null!;
    }
}
