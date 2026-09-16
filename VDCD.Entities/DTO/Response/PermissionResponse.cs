namespace VDCD.Entities.DTO.Response;

public class PermissionResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class PermissionGroupResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<PermissionResponse> Permissions { get; set; } = new();
}

public class RolePermissionMatrixResponse
{
    public List<RoleResponse> Roles { get; set; } = new();
    public List<PermissionGroupResponse> Groups { get; set; } = new();
    public Dictionary<int, List<int>> PermissionIdsByRole { get; set; } = new();
}
