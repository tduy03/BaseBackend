using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDCD.Entities.Enums;

namespace VDCD.Entities.Entities
{
	public class User
	{
		public int Id { get; set; }
		public string UserName { get; set; } = null!;
		public string PasswordHash { get; set; } = null!;
		public string? Email { get; set; }
		public string? FullName { get; set; }
		public string? Phone { get; set; }
		public UserStatus Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
        = new List<UserRole>();
    }
}
