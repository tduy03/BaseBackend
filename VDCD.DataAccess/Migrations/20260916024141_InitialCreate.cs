using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VDCD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PermissionGroupId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_PermissionGroups_PermissionGroupId",
                        column: x => x.PermissionGroupId,
                        principalTable: "PermissionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PermissionGroups",
                columns: new[] { "Id", "Code", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "USER_MANAGEMENT", "Quản lý người dùng", 1 },
                    { 2, "ROLE_MANAGEMENT", "Phân quyền người dùng", 2 },
                    { 3, "MARTYR_MANAGEMENT", "Hồ sơ liệt sĩ", 3 },
                    { 4, "CEMETERY_MANAGEMENT", "Nghĩa trang & phần mộ", 4 },
                    { 7, "POST_MANAGEMENT", "Quản lý tin tức", 5 },
                    { 8, "GALLERY_MANAGEMENT", "Thư viện ảnh", 8 },
                    { 9, "OFFERING_MANAGEMENT", "Quản lý dâng hương", 9 },
                    { 10, "CONTACT_MANAGEMENT", "Quản lý liên hệ", 10 },
                    { 11, "SETTINGS_MANAGEMENT", "Cấu hình hệ thống", 11 }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Name", "PermissionGroupId", "SortOrder" },
                values: new object[,]
                {
                    { 1, "USERS_VIEW", "Xem", 1, 1 },
                    { 2, "USERS_CREATE", "Thêm mới", 1, 2 },
                    { 3, "USERS_UPDATE", "Sửa", 1, 3 },
                    { 4, "USERS_DELETE", "Xóa", 1, 4 },
                    { 5, "USERS_STATUS", "Mở/khóa tài khoản", 1, 5 },
                    { 6, "ROLES_VIEW", "Xem", 2, 1 },
                    { 7, "ROLES_CREATE", "Thêm mới", 2, 2 },
                    { 8, "ROLES_UPDATE", "Sửa", 2, 3 },
                    { 9, "ROLES_DELETE", "Xóa", 2, 4 },
                    { 10, "ROLES_ASSIGN_PERMISSIONS", "Cập nhật phân quyền", 2, 5 },
                    { 11, "MARTYRS_VIEW", "Xem", 3, 1 },
                    { 12, "MARTYRS_CREATE", "Thêm mới", 3, 2 },
                    { 13, "MARTYRS_UPDATE", "Sửa", 3, 3 },
                    { 14, "MARTYRS_DELETE", "Xóa", 3, 4 },
                    { 15, "CEMETERIES_VIEW", "Xem", 4, 1 },
                    { 16, "CEMETERIES_CREATE", "Thêm mới", 4, 2 },
                    { 17, "CEMETERIES_UPDATE", "Sửa", 4, 3 },
                    { 18, "CEMETERIES_DELETE", "Xóa", 4, 4 },
                    { 27, "POSTS_VIEW", "Xem", 7, 1 },
                    { 28, "POSTS_CREATE", "Thêm mới", 7, 2 },
                    { 29, "POSTS_UPDATE", "Sửa", 7, 3 },
                    { 30, "POSTS_DELETE", "Xóa", 7, 4 },
                    { 31, "GALLERIES_VIEW", "Xem", 8, 1 },
                    { 32, "GALLERIES_CREATE", "Thêm mới", 8, 2 },
                    { 33, "GALLERIES_UPDATE", "Sửa", 8, 3 },
                    { 34, "GALLERIES_DELETE", "Xóa", 8, 4 },
                    { 35, "OFFERINGS_VIEW", "Xem", 9, 1 },
                    { 36, "OFFERINGS_CREATE", "Thêm mới", 9, 2 },
                    { 37, "OFFERINGS_UPDATE", "Sửa", 9, 3 },
                    { 38, "OFFERINGS_DELETE", "Xóa", 9, 4 },
                    { 39, "CONTACTS_VIEW", "Xem", 10, 1 },
                    { 40, "CONTACTS_CREATE", "Thêm mới", 10, 2 },
                    { 41, "CONTACTS_UPDATE", "Sửa", 10, 3 },
                    { 42, "CONTACTS_DELETE", "Xóa", 10, 4 },
                    { 43, "SETTINGS_VIEW", "Xem", 11, 1 },
                    { 44, "SETTINGS_CREATE", "Thêm mới", 11, 2 },
                    { 45, "SETTINGS_UPDATE", "Sửa", 11, 3 },
                    { 46, "SETTINGS_DELETE", "Xóa", 11, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissionGroups_Code",
                table: "PermissionGroups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionGroupId",
                table: "Permissions",
                column: "PermissionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Code",
                table: "Roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "PermissionGroups");
        }
    }
}
