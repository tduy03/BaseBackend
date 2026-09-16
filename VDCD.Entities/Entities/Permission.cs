namespace VDCD.Entities.Entities;

public class Permission
{
    public int Id { get; set; }
    public int PermissionGroupId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int SortOrder { get; set; }
    public PermissionGroup PermissionGroup { get; set; } = null!;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
