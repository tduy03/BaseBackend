using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDCD.Entities.DTO.Request
{
	public class SsoLoginRequest
	{
		public string Code { get; set; }

		public string RedirectUri { get; set; }
	}
}
