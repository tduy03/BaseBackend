using System;

namespace VDCD.Entities.Entities
{
	public class ActivityLog
	{
		public int Id { get; set; }
		public int? UserId { get; set; }
		public string Action { get; set; } = null!;
		public string? EntityName { get; set; }
		public string? EntityId { get; set; }
		public string? Description { get; set; }
		public string? IpAddress { get; set; }
		public string? UserAgent { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}