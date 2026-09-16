using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VDCD.Business.IService;
using VDCD.Entities.DTO.Request;
using VDCD.Entities.DTO.Response;
using VDCD.Entities.Entities;
using VDCD.DataAccess;

namespace VDCD.Business.Service
{
	public class ActivityLogService : IActivityLogService
	{
		private readonly AppDbContext _context;

		public ActivityLogService(AppDbContext context)
		{
			_context = context;
		}

		public async Task LogAsync(int? userId,
								   string action,
								   string? entityName = null,
								   string? entityId = null,
								   string? description = null,
								   string? ipAddress = null,
								   string? userAgent = null)
		{
			if (string.IsNullOrWhiteSpace(action))
				throw new ArgumentException("action is required", nameof(action));

			try
			{
				var log = new ActivityLog
				{
					UserId = userId,
					Action = action,
					EntityName = entityName,
					EntityId = entityId,
					Description = description,
					IpAddress = ipAddress,
					UserAgent = userAgent,
					CreatedAt = DateTime.UtcNow
				};

				_context.Set<ActivityLog>().Add(log);
				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{
				// Ignore logging errors to prevent blocking business logic
			}
		}

		public async Task<ActivityLogResponse> LogFromClientAsync(int? userId, CreateActivityLogRequest request, string? ipAddress, string? userAgent)
		{
			var action = request.Action?.Trim();
			if (string.IsNullOrWhiteSpace(action))
			{
				action = "Truy cập";
			}

			var log = new ActivityLog
			{
				UserId = userId,
				Action = action,
				EntityName = string.IsNullOrWhiteSpace(request.Module) ? "Hệ thống" : request.Module.Trim(),
				EntityId = request.EntityId,
				Description = request.Detail,
				IpAddress = ipAddress,
				UserAgent = userAgent,
				CreatedAt = DateTime.UtcNow
			};

			_context.ActivityLogs.Add(log);
			await _context.SaveChangesAsync();

			return ToResponse(log, null);
		}

		public async Task<PagedResult<ActivityLogResponse>> GetPagedAsync(GetActivityLogsRequest request)
		{
			// Left join với Users để lấy userName; giữ lại cả log có UserId null (trước khi đăng nhập)
			var query = _context.ActivityLogs.AsNoTracking()
				.Select(log => new ActivityLogResponse
				{
					Id = log.Id,
					UserId = log.UserId,
					UserName = log.UserId != null
						? _context.Users
							.Where(u => u.Id == log.UserId)
							.Select(u => u.UserName)
							.FirstOrDefault()
						: null,
					Module = log.EntityName,
					Action = log.Action,
					EntityId = log.EntityId,
					Detail = log.Description,
					IpAddress = log.IpAddress,
					UserAgent = log.UserAgent,
					CreatedAt = log.CreatedAt
				});

			query = ApplyFilters(query, request);

			var totalCount = await query.CountAsync();
			var items = await query
				.OrderByDescending(x => x.CreatedAt)
				.Skip((request.PageNumber - 1) * request.PageSize)
				.Take(request.PageSize)
				.ToListAsync();

			return new PagedResult<ActivityLogResponse>(items, totalCount, request.PageNumber, request.PageSize);
		}

		public async Task<int> ClearAsync(ClearActivityLogsRequest request)
		{
			var query = _context.ActivityLogs.AsQueryable();

			if (!string.IsNullOrWhiteSpace(request?.Module))
			{
				query = query.Where(x => x.EntityName == request.Module.Trim());
			}

			var items = await query.ToListAsync();
			int count = items.Count;
			_context.ActivityLogs.RemoveRange(items);
			await _context.SaveChangesAsync();

			return count;
		}

		private static IQueryable<ActivityLogResponse> ApplyFilters(
			IQueryable<ActivityLogResponse> query,
			GetActivityLogsRequest request)
		{
			if (!string.IsNullOrWhiteSpace(request.Search))
			{
				var keyword = request.Search.Trim();
				query = query.Where(x =>
					(x.Detail != null && x.Detail.Contains(keyword)) ||
					(x.Action != null && x.Action.Contains(keyword)) ||
					(x.Module != null && x.Module.Contains(keyword)) ||
					(x.UserName != null && x.UserName.Contains(keyword)));
			}

			if (!string.IsNullOrWhiteSpace(request.Module))
			{
				var module = request.Module.Trim();
				query = query.Where(x => x.Module == module);
			}

			if (!string.IsNullOrWhiteSpace(request.Action))
			{
				var action = request.Action.Trim();
				query = query.Where(x => x.Action == action);
			}

			if (!string.IsNullOrWhiteSpace(request.UserName))
			{
				var userName = request.UserName.Trim();
				query = query.Where(x => x.UserName == userName);
			}

			if (request.FromDate.HasValue)
			{
				var from = request.FromDate.Value.ToUniversalTime();
				query = query.Where(x => x.CreatedAt >= from);
			}

			if (request.ToDate.HasValue)
			{
				var to = request.ToDate.Value.ToUniversalTime();
				query = query.Where(x => x.CreatedAt <= to);
			}

			return query;
		}

		private static ActivityLogResponse ToResponse(ActivityLog log, string? userName)
		{
			return new ActivityLogResponse
			{
				Id = log.Id,
				UserId = log.UserId,
				UserName = userName,
				Module = log.EntityName,
				Action = log.Action,
				EntityId = log.EntityId,
				Detail = log.Description,
				IpAddress = log.IpAddress,
				UserAgent = log.UserAgent,
				CreatedAt = log.CreatedAt
			};
		}
	}
}
