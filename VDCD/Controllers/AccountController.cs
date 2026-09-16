using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VDCD.Business.Helper;
using VDCD.Business.IService;
using VDCD.Entities.DTO.Request;
using VDCD.Entities.DTO.Response;

namespace VDCD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);

                return Ok(new
                {
                    status = 200,
                    message = "Đăng nhập thành công",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    status = 401,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    status = 400,
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    status = 500,
                    message = "Có lỗi xảy ra trên server"
                });
            }
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            var request = new SsoLoginRequest
            {
                Code = code,
                RedirectUri = "https://localhost:7196/api/account/callback"
            };

            var result = await _authService.LoginSSOAsync(request);

            Response.Cookies.Append(
                "sso_refresh_token",
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

            return Ok(new ApiResponse<LoginResponse>
            {
                Status = StatusCodes.Status200OK,
                Message = "Login successful",
                Data = new LoginResponse
                {
                    AccessToken = result.AccessToken,
                    User = result.User
                }
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["sso_refresh_token"];

            await _authService.LogoutAsync(refreshToken);

            Response.Cookies.Delete(
                "sso_refresh_token",
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

            return Ok(new ApiResponse<object>
            {
                Status = StatusCodes.Status200OK,
                Message = "Đăng xuất thành công",
                Data = null
            });
        }
    }
}
