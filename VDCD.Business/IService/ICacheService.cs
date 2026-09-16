using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDCD.Business.IService
{
	public interface ICacheService
	{
		Task<T> GetAsync<T>(string key);
		Task SetAsync<T>(string key, T value, int expireSeconds = 3600);
		Task RemoveAsync(string key);
	}
}
