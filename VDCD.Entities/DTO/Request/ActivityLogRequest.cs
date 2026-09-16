namespace VDCD.Entities.DTO.Request
{
	/// <summary>
	/// Payload tạo nhật ký hệ thống (client gọi lên sau khi thao tác)
	/// </summary>
	public class CreateActivityLogRequest
	{
		public string? Module { get; set; }

		public string Action { get; set; } = string.Empty;

		public string? EntityId { get; set; }

		public string? Detail { get; set; }
	}

	/// <summary>
	/// Truy vấn nhật ký hệ thống (phân trang + lọc)
	/// </summary>
	public class GetActivityLogsRequest
	{
		private int _pageNumber = 1;
		private int _pageSize = 20;

		public int PageNumber
		{
			get => _pageNumber;
			set => _pageNumber = value > 0 ? value : 1;
		}

		public int PageSize
		{
			get => _pageSize;
			set => _pageSize = value > 0 ? (value > 100 ? 100 : value) : 20;
		}

		public string? Search { get; set; }

		public string? Module { get; set; }

		public string? Action { get; set; }

		public string? UserName { get; set; }

		public DateTime? FromDate { get; set; }

		public DateTime? ToDate { get; set; }
	}

	/// <summary>
	/// Xóa nhật ký theo module (tùy chọn)
	/// </summary>
	public class ClearActivityLogsRequest
	{
		public string? Module { get; set; }
	}
}
