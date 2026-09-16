using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VDCD.Business.Helper;
using VDCD.Business.IService;
using VDCD.Entities.DTO.Request;
using VDCD.Entities.DTO.Response;

namespace VDCD.Areas.Admin.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/admin/[controller]")]
	public class ActivityLogsController : ControllerBase
	{
		private readonly IActivityLogService _logService;
		private readonly ICurrentUserService _currentUser;
		private readonly IRoleService _roleService;

		public ActivityLogsController(
			IActivityLogService logService,
			ICurrentUserService currentUser,
			IRoleService roleService)
		{
			_logService = logService;
			_currentUser = currentUser;
			_roleService = roleService;
		}

		private async Task<IActionResult?> Demand(string permission)
		{
			if (!_currentUser.UserId.HasValue ||
				!await _roleService.HasPermissionAsync(_currentUser.UserId.Value, permission))
			{
				return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail("Bạn không có quyền thực hiện thao tác này.", 403));
			}

			return null;
		}

		/// <summary>
		/// Ghi một nhật ký từ client (được gọi sau mỗi thao tác quản trị)
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateActivityLogRequest request)
		{
			if (request == null)
			{
				return BadRequest(new { status = 400, message = "Dữ liệu không hợp lệ.", data = (object?)null });
			}

			try
			{
				var result = await _logService.LogFromClientAsync(
					_currentUser.UserId,
					request,
					HttpContext.Connection.RemoteIpAddress?.ToString(),
					Request.Headers["User-Agent"].ToString());

				return Ok(new
				{
					status = StatusCodes.Status200OK,
					message = "Đã ghi nhật ký.",
					data = result
				});
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new
				{
					status = 500,
					message = "Có lỗi xảy ra khi ghi nhật ký.",
					data = (object?)null
				});
			}
		}

		/// <summary>
		/// Lấy danh sách nhật ký (phân trang + lọc)
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> GetAll([FromQuery] GetActivityLogsRequest request)
		{
			var deny = await Demand("LOGS_VIEW");
			if (deny != null) return deny;

			var result = await _logService.GetPagedAsync(request);

			return Ok(new
			{
				status = StatusCodes.Status200OK,
				message = "Lấy danh sách nhật ký thành công.",
				data = result
			});
		}

		/// <summary>
		/// Xóa toàn bộ nhật ký (hoặc theo module)
		/// </summary>
		[HttpDelete]
		public async Task<IActionResult> Clear([FromQuery] ClearActivityLogsRequest request)
		{
			var deny = await Demand("LOGS_DELETE");
			if (deny != null) return deny;

			var count = await _logService.ClearAsync(request);

			return Ok(new
			{
				status = StatusCodes.Status200OK,
				message = $"Đã xóa {count} bản ghi nhật ký.",
				data = count
			});
		}
	}
}
