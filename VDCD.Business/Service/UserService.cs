
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.RegularExpressions;
using VDCD.Business.Helper;
using VDCD.Business.IService;
using VDCD.DataAccess;
using VDCD.Entities.DTO.Request;
using VDCD.Entities.DTO.Response;
using VDCD.Entities.Entities;

namespace VDCD.Business.Service
{
	public class UserService : IUserService
	{
		private readonly AppDbContext _context;
		private readonly IDistributedCache _cache;
		private readonly IActivityLogService _activityLogService;
		private const int CacheSeconds = 3600;
		private readonly ICurrentUserService _currentUserService;
		public UserService(AppDbContext context,
						   IDistributedCache cache,
						   IActivityLogService activityLogService,
						   ICurrentUserService currentUserService)
		{
			_context = context;
			_cache = cache;
			_activityLogService = activityLogService;
			_currentUserService = currentUserService;
		}

        public async Task<UserRespone?> GetAsync(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new UserRespone
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    FullName = x.FullName,
                    Phone = x.Phone,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Roles = x.UserRoles
                    .Select(ur => new RoleResponse
                    {
                        Id = ur.Role.Id,
                        Code = ur.Role.Code,
                        Name = ur.Role.Name
                    })
                    .ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return null;

            //await _activityLogService.LogAsync(
            //    null,
            //    "GetUser",
            //    "User",
            //    id.ToString(),
            //    $"Fetched user {user.UserName}"
            //);

            return user;
        }

        public async Task<PagedResult<UserRespone>> GetAllAsync(UserFilterRequest? filter = null)
        {
            //if (!await HasPermisionsAdmin())
            //{
            //    throw new UnauthorizedAccessException("Forbidden");
            //}

            filter ??= new UserFilterRequest();

            var query = _context.Users
                .AsNoTracking()
                .AsQueryable();

            // Search by keyword (UserName, FullName, Email, Phone, Role)
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();
                query = query.Where(x =>
                    (x.UserName != null && x.UserName.ToLower().Contains(keyword)) ||
                    (x.FullName != null && x.FullName.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    (x.Phone != null && x.Phone.ToLower().Contains(keyword)) ||
                    x.UserRoles.Any(ur => ur.Role.Name.ToLower().Contains(keyword) || ur.Role.Code.ToLower().Contains(keyword))
                );
            }

            // Filter by status
            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            // Filter by roleCode
            if (!string.IsNullOrWhiteSpace(filter.RoleCode))
            {
                var roleCode = filter.RoleCode.Trim().ToUpper();
                query = query.Where(x => x.UserRoles.Any(ur => ur.Role.Code.ToUpper() == roleCode));
            }

            var totalCount = await query.CountAsync();

            // Sorting
            query = (filter.SortBy?.ToLower()) switch
            {
                "username" => filter.IsDescending ? query.OrderByDescending(x => x.UserName) : query.OrderBy(x => x.UserName),
                "fullname" => filter.IsDescending ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
                "email" => filter.IsDescending ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
                "phone" => filter.IsDescending ? query.OrderByDescending(x => x.Phone) : query.OrderBy(x => x.Phone),
                "status" => filter.IsDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                "id" => filter.IsDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                _ => filter.IsDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
            };

            // Paging
            var skip = (filter.PageNumber - 1) * filter.PageSize;
            var users = await query
                .Skip(skip)
                .Take(filter.PageSize)
                .Select(x => new UserRespone
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    FullName = x.FullName,
                    Phone = x.Phone,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Roles = x.UserRoles
                    .Select(ur => new RoleResponse
                    {
                        Id = ur.Role.Id,
                        Code = ur.Role.Code,
                        Name = ur.Role.Name
                    })
                    .ToList()
                })
                .ToListAsync();

            await _activityLogService.LogAsync(
                null,
                "GetAllUsers",
                "User",
                null,
                $"Fetched {users.Count} users (Page {filter.PageNumber}, PageSize {filter.PageSize}, Total {totalCount})"
            );

            return new PagedResult<UserRespone>(users, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<UserRespone> CreateAsync(UserCreateRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var username = request.UserName.Trim();

            // =========================
            // Validate Username
            // =========================

            var exists = await _context.Users
                .AnyAsync(x => x.UserName == username);

            if (exists)
                throw new InvalidOperationException("Username đã tồn tại");

            ValidatePhoneFormat(request.Phone);

            // =========================
            // Validate Role
            // =========================

            // Ưu tiên danh sách vai trò (nhiều role), fallback về RoleCode đơn
            var roleCodes = request.RoleCodes?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToUpper())
                .Distinct()
                .ToList()
                ?? new List<string>();

            if (roleCodes.Count == 0 && !string.IsNullOrWhiteSpace(request.RoleCode))
            {
                roleCodes.Add(request.RoleCode.Trim().ToUpper());
            }

            var resolvedRoles = new List<Role>();
            if (roleCodes.Count > 0)
            {
                resolvedRoles = await _context.Roles
                    .Where(x => roleCodes.Contains(x.Code))
                    .ToListAsync();

                if (resolvedRoles.Count != roleCodes.Count)
                {
                    var missing = string.Join(", ", roleCodes.Except(resolvedRoles.Select(r => r.Code)));
                    throw new KeyNotFoundException($"Role '{missing}' không tồn tại");
                }
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;

                // =========================
                // Create User
                // =========================

                var user = new User
                {
                    UserName = username,
                    PasswordHash = ComputeSha256Hash(request.Password),
                    Email = request.Email?.Trim(),
                    FullName = request.FullName?.Trim(),
                    Phone = request.Phone?.Trim(),
                    Status = request.Status,
                    CreatedAt = now
                };

                _context.Users.Add(user);

                await _context.SaveChangesAsync();

                // =========================
                // Assign Roles
                // =========================

                foreach (var role in resolvedRoles)
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                        CreatedAt = now
                    });
                }

                if (resolvedRoles.Count > 0)
                {
                    await _context.SaveChangesAsync();
                }

                await _cache.RemoveAsync("users:all");

                // =========================
                // Activity Log
                // =========================

                await _activityLogService.LogAsync(
                    null,
                    "CreateUser",
                    "User",
                    user.Id.ToString(),
                    $"Created user {user.UserName}"
                );

                await transaction.CommitAsync();

                return new UserRespone
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Status = user.Status,
                    Roles = resolvedRoles
                        .Select(role => new RoleResponse
                        {
                            Id = role.Id,
                            Code = role.Code,
                            Name = role.Name
                        })
                        .ToList()
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UserRespone> UpdateAsync(int id, UserUpdateRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id    );

            if (user == null)
                throw new KeyNotFoundException("User not found");

            ValidatePhoneFormat(request.Phone);

            var now = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.UserName))
                user.UserName = request.UserName.Trim();

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email.Trim();

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(request.Phone))
                user.Phone = request.Phone.Trim();

            if (request.Status.HasValue)
                user.Status = request.Status.Value;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = ComputeSha256Hash(request.Password);
            }

            // Cập nhật vai trò (thay toàn bộ). Chỉ khi khách gửi RoleCodes
            // (mảng rỗng = xóa hết role) hoặc RoleCode (fallback).
            if (request.RoleCodes != null || !string.IsNullOrWhiteSpace(request.RoleCode))
            {
                var roleCodes = request.RoleCodes?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim().ToUpper())
                    .Distinct()
                    .ToList()
                    ?? new List<string>();

                if (roleCodes.Count == 0 && !string.IsNullOrWhiteSpace(request.RoleCode))
                {
                    roleCodes.Add(request.RoleCode.Trim().ToUpper());
                }

                var resolvedRoles = new List<Role>();
                if (roleCodes.Count > 0)
                {
                    resolvedRoles = await _context.Roles
                        .Where(x => roleCodes.Contains(x.Code))
                        .ToListAsync();

                    if (resolvedRoles.Count != roleCodes.Count)
                    {
                        var missing = string.Join(", ", roleCodes.Except(resolvedRoles.Select(r => r.Code)));
                        throw new KeyNotFoundException($"Role '{missing}' không tồn tại");
                    }
                }

                var existingRoles = await _context.UserRoles
                    .Where(x => x.UserId == user.Id)
                    .ToListAsync();

                _context.UserRoles.RemoveRange(existingRoles);

                foreach (var role in resolvedRoles)
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                        CreatedAt = now
                    });
                }
            }

            user.UpdatedAt = now;
            await _context.SaveChangesAsync();

            try
            {
                await _cache.RemoveAsync($"user:{user.Id}");
                await _cache.RemoveAsync("users:all");
            }
            catch (Exception)
            {
                // Ignore cache errors
            }

            await _activityLogService.LogAsync(
                null,
                "UpdateUser",
                "User",
                user.Id.ToString(),
                $"Updated user {user.UserName}"
            );

            var roles = await _context.UserRoles
                .Where(x => x.UserId == user.Id)
                .Include(x => x.Role)
                .Select(x => new RoleResponse
                {
                    Id = x.Role.Id,
                    Code = x.Role.Code,
                    Name = x.Role.Name
                })
                .ToListAsync();

            return new UserRespone
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Status = user.Status,
                Roles = roles
            };
        }

        public async Task DeleteAsync(int id)
		{
			await using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var user = await _context.Users
					.FirstOrDefaultAsync(x => x.Id == id)
					?? throw new KeyNotFoundException("User not found");

				// 👉 Xóa user_role trước (tránh FK constraint)
				var roles = await _context.UserRoles
					.Where(x => x.UserId == id)
					.ToListAsync();
				if (roles.Any())
				{
					foreach (var role in roles)
					{
						_context.UserRoles.Remove(role);
					}
				}

				_context.Users.Remove(user);

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				// 👉 ngoài transaction
				await _cache.RemoveAsync($"user:{id}");
				await _activityLogService.LogAsync(
					null,
					"DeleteUser",
					"User",
					id.ToString(),
					$"Deleted user {user.UserName}"
				);
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		private static readonly Regex PhoneRegex = new(
			@"^0\d{9}$",
			RegexOptions.Compiled);

		private static void ValidatePhoneFormat(string? phone)
		{
			if (!string.IsNullOrWhiteSpace(phone))
			{
				var digits = new string(phone.Where(char.IsDigit).ToArray());
				var normalized = digits.StartsWith("84") ? "0" + digits.Substring(2) : digits;

				if (normalized.StartsWith("0") && PhoneRegex.IsMatch(normalized)) return;

				throw new InvalidOperationException(
					"Số điện thoại không đúng định dạng (VD: 0392861103 hoặc +84392861103)");
			}
		}

		private static string ComputeSha256Hash(string rawData)
		{
			using var sha256 = System.Security.Cryptography.SHA256.Create();
			var bytes = System.Text.Encoding.UTF8.GetBytes(rawData);
			var hashBytes = sha256.ComputeHash(bytes);
			var sb = new System.Text.StringBuilder();
			foreach (var b in hashBytes)
				sb.Append(b.ToString("x2"));

			return sb.ToString();
		}

		public async Task<UserRespone> GetCurrentUser()
		{
			var userId = _currentUserService.UserId;
			// remove sensitive data before returning
			var user = await GetAsync(userId!.Value);
			var userRoles = await _context.UserRoles
				.Where(x => x.UserId == userId.Value)
				.Include(x => x.Role)
				.Select(x => new RoleResponse
				{
					Id = x.Role.Id,
					Name = x.Role.Name,
					Code = x.Role.Code,
					Description = x.Role.Description
				})
				.ToListAsync();
            var roleCodes = userRoles.Select(x => x.Code).ToList();
            var permissions = roleCodes.Contains("SUPER_ADMIN")
                ? await _context.Permissions.Select(x => x.Code).ToListAsync()
                : await _context.UserRoles
                    .Where(x => x.UserId == userId.Value)
                    .SelectMany(x => x.Role.RolePermissions.Select(rp => rp.Permission.Code))
                    .Distinct()
                    .ToListAsync();
			var response = new UserRespone
			{
				Id = user!.Id,
				UserName = user.UserName,
				Email = user.Email,
				FullName = user.FullName,
				Phone = user.Phone,
                Status = user.Status,
                Roles = userRoles,
                Permissions = permissions
            };
			return response;
		}

        public async Task<bool> HasPermisionsAdmin()
        {
            var userId = _currentUserService.UserId;

            if (userId == null)
                return false;

            var roleCodes = await _context.UserRoles
                .Where(x => x.UserId == userId.Value)
                .Include(x => x.Role)
                .Select(x => x.Role.Code)
                .ToListAsync();

            return roleCodes.Contains("SUPER_ADMIN")
                || roleCodes.Contains("MANAGER");
        }
    }
}