using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDCD.Entities.DTO.Request;
using VDCD.Entities.DTO.Response;
using VDCD.Entities.Entities;

namespace VDCD.Business.IService
{
	public interface IUserService
	{
		Task<UserRespone?> GetAsync(int id);
		Task<PagedResult<UserRespone>> GetAllAsync(UserFilterRequest? filter = null);
        Task<UserRespone> CreateAsync(UserCreateRequest request);
		Task<UserRespone> UpdateAsync(int id, UserUpdateRequest request);
		Task DeleteAsync(int id);
		Task<UserRespone> GetCurrentUser();
		Task<bool> HasPermisionsAdmin();
	}
}
