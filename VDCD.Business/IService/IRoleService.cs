using VDCD.Entities.DTO.Request;
using VDCD.Entities.DTO.Response;

namespace VDCD.Business.IService;

public interface IRoleService
{
    Task<List<RoleResponse>> GetAllAsync();
    Task<RoleResponse> CreateAsync(RoleCreateRequest request);
    Task<RoleResponse> UpdateAsync(int id, RoleUpdateRequest request);
    Task DeleteAsync(int id);
    Task<RolePermissionMatrixResponse> GetPermissionMatrixAsync();
    Task UpdatePermissionsAsync(int roleId, UpdateRolePermissionsRequest request);
    Task<bool> HasPermissionAsync(int userId, string permissionCode);
}
