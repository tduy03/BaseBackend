using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDCD.Entities.Enums;

namespace VDCD.Entities.DTO.Request
{
    public class UserCreateRequest
    {
        [Required(ErrorMessage = "Username không được để trống")]
        [StringLength(50, MinimumLength = 3,
        ErrorMessage = "Username phải từ 3 đến 50 ký tự")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(20)]
        public string Phone { get; set; } = null!;

        public UserStatus Status { get; set; } = UserStatus.Active;

        [Required(ErrorMessage = "RoleCode không được để trống")]
        public string RoleCode { get; set; } = null!;

        /// <summary>
        /// Danh sách mã vai trò (hỗ trợ gán nhiều role cho 1 user).
        /// Ưu tiên hơn RoleCode; nếu để trống sẽ dùng lại RoleCode.
        /// </summary>
        public List<string>? RoleCodes { get; set; }
    }

    public class UserUpdateRequest
    {
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Username phải từ 3 đến 50 ký tự")]
        public string? UserName { get; set; }

        [StringLength(100,
            ErrorMessage = "Mật khẩu không được quá 100 ký tự")]
        public string? Password { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email không được quá 255 ký tự")]
        public string? Email { get; set; }

        [StringLength(100,
            ErrorMessage = "Họ tên không được quá 100 ký tự")]
        public string? FullName { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        public string? Phone { get; set; }

        public UserStatus? Status { get; set; }

        public string? RoleCode { get; set; }

        /// <summary>
        /// Danh sách mã vai trò. Gửi null/thiếu => không đổi quyền;
        /// gửi mảng rỗng => xóa toàn bộ vai trò của user.
        /// </summary>
        public List<string>? RoleCodes { get; set; }
    }
}
