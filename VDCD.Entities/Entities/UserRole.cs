using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDCD.Entities.Entities
{
    public class UserRole
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int RoleId { get; set; }

        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;

        public Role Role { get; set; } = null!;

    }
}
