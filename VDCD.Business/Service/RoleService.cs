using Microsoft.EntityFrameworkCore;
using VDCD.Business.IService;
using VDCD.DataAccess;
using VDCD.Entities.DTO.Request;
using VDCD.Entities.DTO.Response;
using VDCD.Entities.Entities;

namespace VDCD.Business.Service;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;
    public RoleService(AppDbContext context) => _context = context;

    public Task<List<RoleResponse>> GetAllAsync() => _context.Roles.AsNoTracking()
        .OrderBy(x => x.Name).Select(x => ToResponse(x)).ToListAsync();

    public async Task<RoleResponse> CreateAsync(RoleCreateRequest request)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _context.Roles.AnyAsync(x => x.Code == code)) throw new InvalidOperationException("Mã role đã tồn tại.");
        var role = new Role { Name = request.Name.Trim(), Code = code, Description = request.Description?.Trim(), CreatedAt = DateTime.UtcNow };
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return ToResponse(role);
    }

    public async Task<RoleResponse> UpdateAsync(int id, RoleUpdateRequest request)
    {
        var role = await _context.Roles.FindAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy role.");
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _context.Roles.AnyAsync(x => x.Id != id && x.Code == code)) throw new InvalidOperationException("Mã role đã tồn tại.");
        role.Name = request.Name.Trim(); role.Code = code; role.Description = request.Description?.Trim(); role.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(); return ToResponse(role);
    }

    public async Task DeleteAsync(int id)
    {
        var role = await _context.Roles.Include(x => x.UserRoles).FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException("Không tìm thấy role.");
        if (role.UserRoles.Count > 0)
            throw new InvalidOperationException(
                "Không thể xóa vai trò này vì vẫn còn người dùng được gán vai trò. Hãy chuyển người dùng sang vai trò khác trước khi xóa.");

        _context.Roles.Remove(role); await _context.SaveChangesAsync();
    }

    public async Task<RolePermissionMatrixResponse> GetPermissionMatrixAsync()
    {
        var roles = await GetAllAsync();
        var groups = await _context.PermissionGroups.AsNoTracking().OrderBy(x => x.SortOrder).Select(x => new PermissionGroupResponse
        { Id = x.Id, Code = x.Code, Name = x.Name, Permissions = x.Permissions.OrderBy(p => p.SortOrder).Select(p => new PermissionResponse { Id = p.Id, Code = p.Code, Name = p.Name }).ToList() }).ToListAsync();
        var mapping = await _context.RolePermissions.AsNoTracking().GroupBy(x => x.RoleId).ToDictionaryAsync(x => x.Key, x => x.Select(p => p.PermissionId).ToList());
        var superAdmin = roles.FirstOrDefault(x => x.Code == "SUPER_ADMIN");
        if (superAdmin != null) mapping[superAdmin.Id] = groups.SelectMany(x => x.Permissions).Select(x => x.Id).ToList();
        return new RolePermissionMatrixResponse { Roles = roles, Groups = groups, PermissionIdsByRole = mapping };
    }

    public async Task UpdatePermissionsAsync(int roleId, UpdateRolePermissionsRequest request)
    {
        var role = await _context.Roles.FindAsync(roleId) ?? throw new KeyNotFoundException("Không tìm thấy role.");
        var ids = request.PermissionIds.Distinct().ToList();
        if (ids.Count != 0 && await _context.Permissions.CountAsync(x => ids.Contains(x.Id)) != ids.Count) throw new ArgumentException("Có quyền không hợp lệ.");
        _context.RolePermissions.RemoveRange(_context.RolePermissions.Where(x => x.RoleId == roleId));
        _context.RolePermissions.AddRange(ids.Select(id => new RolePermission { RoleId = roleId, PermissionId = id }));
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode) =>
        await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && (ur.Role.Code == "SUPER_ADMIN" || ur.Role.RolePermissions.Any(rp => rp.Permission.Code == permissionCode)));

    private static RoleResponse ToResponse(Role role) => new() { Id = role.Id, Name = role.Name, Code = role.Code, Description = role.Description };
}
