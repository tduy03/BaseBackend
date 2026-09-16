using Microsoft.EntityFrameworkCore;
using VDCD.Entities.Entities;
using VDCD.Entities.Enums;

namespace VDCD.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<PermissionGroup> PermissionGroups { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<RolePermission> RolePermissions { get; set; }
        public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>()
                .HasIndex(x => x.UserName)
                .IsUnique();
            modelBuilder.Entity<UserRole>().ToTable("UserRoles");
            modelBuilder.Entity<Role>().ToTable("Roles");

            modelBuilder.Entity<UserRole>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Role>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<UserRole>()
                .HasIndex(x => new { x.UserId, x.RoleId })
                .IsUnique();

            modelBuilder.Entity<PermissionGroup>().ToTable("PermissionGroups");
            modelBuilder.Entity<PermissionGroup>().HasIndex(x => x.Code).IsUnique();
            modelBuilder.Entity<Permission>().ToTable("Permissions");
            modelBuilder.Entity<Permission>().HasIndex(x => x.Code).IsUnique();
            modelBuilder.Entity<Permission>()
                .HasOne(x => x.PermissionGroup)
                .WithMany(x => x.Permissions)
                .HasForeignKey(x => x.PermissionGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RolePermission>().ToTable("RolePermissions");
            modelBuilder.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
            modelBuilder.Entity<RolePermission>()
                .HasOne(x => x.Role)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RolePermission>()
                .HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PermissionGroup>().HasData(
                new PermissionGroup { Id = 1, Code = "USER_MANAGEMENT", Name = "Quản lý người dùng", SortOrder = 1 },
                new PermissionGroup { Id = 2, Code = "ROLE_MANAGEMENT", Name = "Phân quyền người dùng", SortOrder = 2 },
                new PermissionGroup { Id = 13, Code = "LOG_MANAGEMENT", Name = "Nhật ký hệ thống", SortOrder = 3 });
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, PermissionGroupId = 1, Code = "USERS_VIEW", Name = "Xem", SortOrder = 1 },
                new Permission { Id = 2, PermissionGroupId = 1, Code = "USERS_CREATE", Name = "Thêm mới", SortOrder = 2 },
                new Permission { Id = 3, PermissionGroupId = 1, Code = "USERS_UPDATE", Name = "Sửa", SortOrder = 3 },
                new Permission { Id = 4, PermissionGroupId = 1, Code = "USERS_DELETE", Name = "Xóa", SortOrder = 4 },
                new Permission { Id = 5, PermissionGroupId = 1, Code = "USERS_STATUS", Name = "Mở/khóa tài khoản", SortOrder = 5 },
                new Permission { Id = 6, PermissionGroupId = 2, Code = "ROLES_VIEW", Name = "Xem", SortOrder = 1 },
                new Permission { Id = 7, PermissionGroupId = 2, Code = "ROLES_CREATE", Name = "Thêm mới", SortOrder = 2 },
                new Permission { Id = 8, PermissionGroupId = 2, Code = "ROLES_UPDATE", Name = "Sửa", SortOrder = 3 },
                new Permission { Id = 9, PermissionGroupId = 2, Code = "ROLES_DELETE", Name = "Xóa", SortOrder = 4 },
                new Permission { Id = 10, PermissionGroupId = 2, Code = "ROLES_ASSIGN_PERMISSIONS", Name = "Cập nhật phân quyền", SortOrder = 5 },
                new Permission { Id = 51, PermissionGroupId = 13, Code = "LOGS_VIEW", Name = "Xem", SortOrder = 1 },
                new Permission { Id = 52, PermissionGroupId = 13, Code = "LOGS_DELETE", Name = "Xóa", SortOrder = 2 });

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Nhân viên", Code = "USER", Description = "Người dùng", CreatedAt = new DateTime(2026, 08, 20, 15, 43, 30, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 08, 29, 02, 05, 26, DateTimeKind.Utc) },
                new Role { Id = 2, Name = "Quản trị viên", Code = "SUPER_ADMIN", Description = "Quản trị viên", CreatedAt = new DateTime(2026, 08, 20, 15, 43, 30, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 08, 29, 02, 05, 14, DateTimeKind.Utc) },
                new Role { Id = 3, Name = "Quản lý", Code = "MANAGER", Description = "Người quản lý", CreatedAt = new DateTime(2026, 08, 20, 19, 12, 14, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 08, 29, 02, 04, 46, DateTimeKind.Utc) },
                new Role { Id = 4, Name = "Bảo vệ", Code = "SECURITY", Description = "Bảo vệ", CreatedAt = new DateTime(2026, 08, 20, 20, 03, 22, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 08, 29, 02, 04, 53, DateTimeKind.Utc) });

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, UserName = "user1", PasswordHash = "d341db7c246c68b31211241e9ba78a6069d0bacff625eaae4bf1ded8275363d2", Email = "user1@gmail.com", FullName = "User 1", Phone = "0123456789", Status = UserStatus.Active, CreatedAt = new DateTime(2026, 03, 30, 01, 21, 12, DateTimeKind.Utc) },
                new User { Id = 2, UserName = "admin", PasswordHash = "d341db7c246c68b31211241e9ba78a6069d0bacff625eaae4bf1ded8275363d2", Email = "admin@gmail.com", FullName = "Quản trị viên", Phone = "0392861103", Status = UserStatus.Active, CreatedAt = new DateTime(2026, 03, 09, 02, 34, 35, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 09, 03, 07, 44, 49, DateTimeKind.Utc) },
                new User { Id = 3, UserName = "user2", PasswordHash = "d341db7c246c68b31211241e9ba78a6069d0bacff625eaae4bf1ded8275363d2", Email = "user1@gmail.com", FullName = "User 1", Phone = "0123456789", Status = UserStatus.Active, CreatedAt = new DateTime(2026, 03, 30, 01, 24, 52, DateTimeKind.Utc) },
                new User { Id = 5, UserName = "tduy03", PasswordHash = "d341db7c246c68b31211241e9ba78a6069d0bacff625eaae4bf1ded8275363d2", Email = "dinhtuanduy01@gmail.com", FullName = "Đinh Tuấn Duy", Phone = "0392861103", Status = UserStatus.Active, CreatedAt = new DateTime(2026, 08, 20, 16, 01, 32, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 09, 08, 11, 31, 48, DateTimeKind.Utc) },
                new User { Id = 6, UserName = "emilys1", PasswordHash = "8867d039d6fd6fe9d68b3fa7500f2f21969b3ff5cb791d195db1353bac1af3f3", Email = "dinhtuanduy01@gmail.com", FullName = "Đinh Tuấn Duy", Phone = "0392861103", Status = UserStatus.Active, CreatedAt = new DateTime(2026, 08, 20, 17, 36, 51, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 09, 08, 11, 25, 47, DateTimeKind.Utc) });

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, UserId = 1, RoleId = 1, CreatedAt = new DateTime(2026, 08, 24, 10, 24, 27, DateTimeKind.Utc) },
                new UserRole { Id = 2, UserId = 3, RoleId = 1, CreatedAt = new DateTime(2026, 08, 24, 10, 23, 43, DateTimeKind.Utc) },
                new UserRole { Id = 4, UserId = 2, RoleId = 2, CreatedAt = new DateTime(2026, 09, 03, 07, 44, 49, DateTimeKind.Utc) },
                new UserRole { Id = 5, UserId = 5, RoleId = 1, CreatedAt = new DateTime(2026, 08, 24, 10, 24, 54, DateTimeKind.Utc) },
                new UserRole { Id = 7, UserId = 6, RoleId = 1, CreatedAt = new DateTime(2026, 09, 08, 11, 25, 47, DateTimeKind.Utc) });

            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RoleId = 1, PermissionId = 1 },
                new RolePermission { RoleId = 1, PermissionId = 3 },
                new RolePermission { RoleId = 1, PermissionId = 6 },
                new RolePermission { RoleId = 2, PermissionId = 1 },
                new RolePermission { RoleId = 2, PermissionId = 2 },
                new RolePermission { RoleId = 2, PermissionId = 3 },
                new RolePermission { RoleId = 2, PermissionId = 4 },
                new RolePermission { RoleId = 2, PermissionId = 5 },
                new RolePermission { RoleId = 2, PermissionId = 6 },
                new RolePermission { RoleId = 2, PermissionId = 7 },
                new RolePermission { RoleId = 2, PermissionId = 8 },
                new RolePermission { RoleId = 2, PermissionId = 9 },
                new RolePermission { RoleId = 2, PermissionId = 10 },
                new RolePermission { RoleId = 2, PermissionId = 51 },
                new RolePermission { RoleId = 2, PermissionId = 52 },
                new RolePermission { RoleId = 3, PermissionId = 1 },
                new RolePermission { RoleId = 4, PermissionId = 1 });

            foreach (var property in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p =>
                    p.ClrType == typeof(DateTime) ||
                    p.ClrType == typeof(DateTime?)))
            {
                property.SetColumnType("timestamptz");
            }
        }
    }
}