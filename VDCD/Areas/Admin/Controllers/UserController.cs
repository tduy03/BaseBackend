using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VDCD.Business.Helper;
using VDCD.Business.IService;
using VDCD.DataAccess;
using VDCD.Entities.DTO.Request;
using VDCD.Entities.Entities;
namespace VDCD.Areas.Admin.Controllers
{
	[Authorize]
	[Route("api/admin/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ICurrentUserService _currentUser;

        public UserController(
            IUserService userService,
            IRoleService roleService,
            ICurrentUserService currentUser)
		{
			_userService = userService;
            _roleService = roleService;
            _currentUser = currentUser;
		}

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] UserFilterRequest filter)
        {
            try
            {
                await Demand("USERS_VIEW");
                var result = await _userService.GetAllAsync(filter);

                return Ok(new
                {
                    status = 200,
                    message = "Lấy danh sách user thành công",
                    data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new
                {
                    status = 403,
                    message = "Forbidden"
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

        [HttpGet("{id:int}")]
		public async Task<IActionResult> Get(int id)
		{
			try
			{
                await Demand("USERS_VIEW");
				var user = await _userService.GetAsync(id);

				if (user == null)
					return NotFound();

                return Ok(new
                {
                    status = 200,
                    message = "Lấy danh sách user thành công",
                    data = user
                });
            }
			catch (UnauthorizedAccessException)
			{
                return StatusCode(403, new
                {
                    status = 403,
                    message = "Forbidden"
                });
            }
            catch (Exception ex)
			{
                return StatusCode(500, new
                {
                    status = 500,
                    message = "Có lỗi xảy ra trên server"
                });
            }
		}

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateRequest dto)
        {
            if (dto == null)
                return BadRequest();

            try
            {
                await Demand("USERS_CREATE");
                var response = await _userService.CreateAsync(dto);

                return StatusCode(201, new
                {
                    status = 201,
                    message = "Tạo user thành công",
                    data = response
                });

            } catch (UnauthorizedAccessException) {

                return StatusCode(403, new
                {
                    status = 403,
                    message = "Forbidden"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new
                {
                    status = 400,
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
            catch (Exception ex)
            {
                Console.WriteLine($"[UserController.Create] ERROR: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new
                {
                    status = 500,
                    message = "Có lỗi xảy ra trên server"
                });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UserUpdateRequest dto)
        {
            if (dto == null)
                return BadRequest();

            try
            {
                await Demand(HasOnlyStatusChange(dto) ? "USERS_STATUS" : "USERS_UPDATE");
                var response = await _userService.UpdateAsync(id, dto);

                return StatusCode(200, new
                {
                    status = 200,
                    message = "Cập nhật user thành công",
                    data = response
                });
            }
            catch (UnauthorizedAccessException)
            {

                return StatusCode(403, new
                {
                    status = 403,
                    message = "Forbidden"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new
                {
                    status = 400,
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

        [HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
                await Demand("USERS_DELETE");
				await _userService.DeleteAsync(id);
				return NoContent();
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new
                {
                    status = 403,
                    message = "Forbidden"
                });
            }
		}
		[HttpGet("GetCurrentUser")]
		[Authorize]
		public async Task<IActionResult> GetCurrentUser()
		{
			var users = await _userService.GetCurrentUser();
			return Ok(users);
		}

        private async Task Demand(string permission)
        {
            if (!_currentUser.UserId.HasValue ||
                !await _roleService.HasPermissionAsync(_currentUser.UserId.Value, permission))
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện chức năng này.");
            }
        }

        private static bool HasOnlyStatusChange(UserUpdateRequest request)
        {
            return request.Status.HasValue &&
                string.IsNullOrWhiteSpace(request.UserName) &&
                string.IsNullOrWhiteSpace(request.Password) &&
                string.IsNullOrWhiteSpace(request.Email) &&
                string.IsNullOrWhiteSpace(request.FullName) &&
                string.IsNullOrWhiteSpace(request.Phone) &&
                string.IsNullOrWhiteSpace(request.RoleCode);
        }
	}
}
