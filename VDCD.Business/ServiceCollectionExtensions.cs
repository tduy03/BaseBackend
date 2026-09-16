using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace VDCD.Business
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddBusinessServices(this IServiceCollection services)
		{
			var assembly = Assembly.Load("VDCD.Business");

			var serviceTypes = assembly
				.GetTypes()
				.Where(t =>
					t.IsClass &&
					!t.IsAbstract &&
					t.IsPublic &&
					!t.IsNested &&
					t.Namespace == "VDCD.Business.Service" &&
					(t.Name.EndsWith("Service", StringComparison.Ordinal) ||
					 t.Name.EndsWith("Sevice", StringComparison.Ordinal) ||
					 t.Name.EndsWith("Bll", StringComparison.Ordinal)));

			foreach (var impl in serviceTypes)
			{
				var interfaceType = impl.GetInterfaces()
					.FirstOrDefault(i => i.Namespace == "VDCD.Business.IService");

				if (interfaceType != null)
				{
					services.AddScoped(interfaceType, impl);
				}
				else
				{
					services.AddScoped(impl);
				}
			}

			return services;
		}
	}
}
