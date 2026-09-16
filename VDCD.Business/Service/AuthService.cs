using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VDCD.Business.IService;
using VDCD.DataAccess;
using VDCD.Entities.DTO.Request;
using VDCD.Entities.DTO.Response;
using VDCD.Entities.Entities;
using VDCD.Entities.Enums;

namespace VDCD.Business.Service
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IRepository<User> _userRepository;
        private readonly HttpClient _http;
        private AppDbContext _context;

        public AuthService(IConfiguration config, IRepository<User> userRepository, AppDbContext context)
        {
            _config = config;
            _userRepository = userRepository;
            _http = new HttpClient();
            _context = context;
        }

        // Normal (non-SSO) login: authenticate against local DB only and return an app-issued JWT.
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // 1) Validate user exists in local store
            var user = _userRepository.Get(true, x => x.UserName == request.UserName);
            if (user == null)
                throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng");

            // If user was created by SSO and has no password, disallow local login
            if (string.IsNullOrEmpty(user.PasswordHash))
                throw new InvalidOperationException("Tài khoản này được tạo bởi SSO. Vui lòng sử dụng SSO để đăng nhập");

            // 2) Block locked/inactive accounts
            if (user.Status == UserStatus.Inactive)
                throw new UnauthorizedAccessException("Tài khoản đã bị khóa, không thể đăng nhập. Vui lòng liên hệ quản trị viên.");

            // 3) Verify submitted password against stored SHA256 hash
            if (!VerifyPassword(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng");

            // 3) Return locally issued JWT (does not call any SSO/Keycloak endpoint)
            return new LoginResponse
            {
                AccessToken = GenerateJwtToken(user),
                User = new UserInfoResponse
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email
                }
            };
        }

        public async Task<SsoLoginResult> LoginSSOAsync(SsoLoginRequest request)
        {
            var server = _config["Keycloak:ServerUrl"];
            var realm = _config["Keycloak:Realm"];
            var clientId = _config["Keycloak:ClientId"];
            var clientSecret = _config["Keycloak:ClientSecret"];

            var url = $"{server}/realms/{realm}/protocol/openid-connect/token";

            var body = new Dictionary<string, string>
        {
            {"client_id", clientId},
            {"client_secret", clientSecret},
            {"grant_type", "authorization_code"},
            {"code", request.Code},
            {"redirect_uri", request.RedirectUri}
        };

            var response = await _http.PostAsync(url, new FormUrlEncodedContent(body));
            var json = await response.Content.ReadAsStringAsync();

            return await HandleToken(json);
        }

        // Handle response from Keycloak (used only by SSO flow)
        private async Task<SsoLoginResult> HandleToken(string json)
        {
            var tokenJson = JsonDocument.Parse(json);

            var accessToken = tokenJson
                .RootElement
                .GetProperty("access_token")
                .GetString();

            var refreshToken = tokenJson
                .RootElement
                .GetProperty("refresh_token")
                .GetString();

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);

            var username = jwt.Claims
                .First(x => x.Type == "preferred_username")
                .Value;

            var email = jwt.Claims
                .FirstOrDefault(x => x.Type == "email")
                ?.Value;

            var user = _userRepository.Get(
                true,
                x => x.UserName == username
            );

            if (user == null)
            {
                user = new User
                {
                    UserName = username,
                    Email = email,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = string.Empty
                };

                _userRepository.Create(user);
                _context.SaveChanges();
            }

            var systemToken = GenerateJwtToken(user);

            return new SsoLoginResult
            {
                AccessToken = systemToken,
                RefreshToken = refreshToken!,
                User = new UserInfoResponse
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email
                }
            };
        }

        private string GenerateJwtToken(User user)
        {
            // Require a symmetric signing key in configuration: Jwt:Key
            var key = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("Missing Jwt:Key configuration (required to sign tokens).");

            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiresMinutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 60;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            };

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiresMinutes),
                SigningCredentials = creds,
                Issuer = issuer,
                Audience = audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static bool VerifyPassword(string plainPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            var computed = ComputeSha256Hash(plainPassword);
            // For improved security you can use CryptographicOperations.FixedTimeEquals for fixed-time comparison
            return string.Equals(computed, storedHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(rawData);
            var hashBytes = sha256.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public async Task LogoutAsync(string? refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return;

            var server = _config["Keycloak:ServerUrl"];
            var realm = _config["Keycloak:Realm"];
            var clientId = _config["Keycloak:ClientId"];
            var clientSecret = _config["Keycloak:ClientSecret"];

            var url =
                $"{server}/realms/{realm}/protocol/openid-connect/logout";

            var body = new Dictionary<string, string>
        {
            { "client_id", clientId! },
            { "client_secret", clientSecret! },
            { "refresh_token", refreshToken }
        };

            await _http.PostAsync(
                url,
                new FormUrlEncodedContent(body)
            );
        }
    }
}
public class KeycloakUserInfo
{
    public string sub { get; set; }
    public string preferred_username { get; set; }
    public string email { get; set; }
}