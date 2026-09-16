using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDCD.Entities.Enums;

namespace VDCD.Entities.DTO.Response
{
	public class UserRespone
	{
		public int Id { get; set; } = 0;
		public string? UserName { get; set; } // required for create
		public string? Password { get; set; } // plain password for set/update
		public string? Email { get; set; }
		public string? FullName { get; set; }
		public string? Phone { get; set; }
		public UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<RoleResponse> Roles { get; set; } = new();
		public List<string> Permissions { get; set; } = new();

    }
}
