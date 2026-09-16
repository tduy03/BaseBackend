using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDCD.Entities.DTO.Response
{
	public class ActivityLogResponse
	{
		public int Id { get; set; }

		public int? UserId { get; set; }

		public string? UserName { get; set; }

		public string? Module { get; set; }

		public string? Action { get; set; }

		public string? EntityId { get; set; }

		public string? Detail { get; set; }

		public string? IpAddress { get; set; }

		public string? UserAgent { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}
