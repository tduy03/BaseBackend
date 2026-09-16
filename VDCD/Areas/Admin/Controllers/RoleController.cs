using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VDCD.Business.IService;
using VDCD.Business.Helper;
using VDCD.Entities.DTO.Request;

namespace VDCD.Areas.Admin.Controllers;

[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roles;
    private readonly ICurrentUserService _currentUser;
    public RoleController(IRoleService roles, ICurrentUserService currentUser) { _roles = roles; _currentUser = currentUser; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            await Demand("ROLES_VIEW");
            return Ok(await _roles.GetAllAsync());
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbidden(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(RoleCreateRequest request)
    {
        try
        {
            await Demand("ROLES_CREATE");
            return StatusCode(201, await _roles.CreateAsync(request));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbidden(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, RoleUpdateRequest request)
    {
        try
        {
            await Demand("ROLES_UPDATE");
            return Ok(await _roles.UpdateAsync(id, request));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbidden(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await Demand("ROLES_DELETE");
            await _roles.DeleteAsync(id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbidden(ex.Message);
        }
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions()
    {
        try
        {
            await Demand("ROLES_VIEW");
            return Ok(await _roles.GetPermissionMatrixAsync());
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbidden(ex.Message);
        }
    }

    [HttpPut("{id:int}/permissions")]
    public async Task<IActionResult> UpdatePermissions(int id, UpdateRolePermissionsRequest request)
    {
        try
        {
            await Demand("ROLES_ASSIGN_PERMISSIONS");
            await _roles.UpdatePermissionsAsync(id, request);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbidden(ex.Message);
        }
    }

    private async Task Demand(string permission)
    {
        if (!_currentUser.UserId.HasValue || !await _roles.HasPermissionAsync(_currentUser.UserId.Value, permission))
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện chức năng này.");
    }

    private IActionResult Forbidden(string message) => StatusCode(403, new
    {
        status = 403,
        message
    });
}
