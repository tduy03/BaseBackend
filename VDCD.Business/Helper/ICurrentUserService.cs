using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDCD.Business.Helper
{
	public interface ICurrentUserService
	{
		int? UserId { get; }
		string Username { get; }
		string Email { get; }
	}
}
