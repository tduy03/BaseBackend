# vdcd-hotel-backend

Base backend khách sạn "VDCD".

## Thành phần

- **VDCD** — Web API startup (đăng nhập JWT, quản trị user/role/permission, nhật ký hệ thống, Swagger)
- **VDCD.Business** — Services: `AuthService`, `UserService`, `RoleService`, `ActivityLogService`, `CacheService`
- **VDCD.DataAccess** — `AppDbContext` (PostgreSQL/EF Core) + `GenericRepository`
- **VDCD.Entities** — Entities, DTOs, enums

## Database

- PostgreSQL `vdcd_hotel` (xem `VDCD/appsettings.json`)
- Migration: `dotnet ef database update`