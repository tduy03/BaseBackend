using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VDCD.Business.IService;

namespace VDCD.Business.Service
{
	public class CacheService : ICacheService
	{
		private readonly IDistributedCache _cache;

		public CacheService(IDistributedCache cache)
		{
			_cache = cache;
		}

		public async Task<T> GetAsync<T>(string key)
		{
			var data = await _cache.GetStringAsync(key);

			if (data == null)
				return default;

			return JsonSerializer.Deserialize<T>(data);
		}

		public async Task SetAsync<T>(string key, T value, int expireSeconds = 3600)
		{
			var options = new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expireSeconds)
			};

			var json = JsonSerializer.Serialize(value);

			await _cache.SetStringAsync(key, json, options);
		}

		public async Task RemoveAsync(string key)
		{
			await _cache.RemoveAsync(key);
		}
	}
}
