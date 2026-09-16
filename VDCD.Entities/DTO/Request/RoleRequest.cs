using System.ComponentModel.DataAnnotations;

namespace VDCD.Entities.DTO.Request;

public class RoleCreateRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = null!;
    [Required, MaxLength(50)] public string Code { get; set; } = null!;
    [MaxLength(500)] public string? Description { get; set; }
}

public class RoleUpdateRequest : RoleCreateRequest { }

public class UpdateRolePermissionsRequest
{
    public List<int> PermissionIds { get; set; } = new();
}
