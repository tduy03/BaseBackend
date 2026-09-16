using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDCD.Entities.DTO.Request;
using VDCD.Entities.DTO.Response;

namespace VDCD.Business.IService
{
	public interface IActivityLogService
	{
		Task LogAsync(int? userId,
					  string action,
					  string? entityName = null,
					  string? entityId = null,
					  string? description = null,
					  string? ipAddress = null,
					  string? userAgent = null);

		/// <summary>
		/// Ghi nhật ký từ client (module = EntityName)
		/// </summary>
		Task<ActivityLogResponse> LogFromClientAsync(int? userId, CreateActivityLogRequest request, string? ipAddress, string? userAgent);

		Task<PagedResult<ActivityLogResponse>> GetPagedAsync(GetActivityLogsRequest request);

		Task<int> ClearAsync(ClearActivityLogsRequest request);
	}
}
