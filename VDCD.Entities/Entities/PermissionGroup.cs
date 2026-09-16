namespace VDCD.Entities.Entities;

public class PermissionGroup
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int SortOrder { get; set; }
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
