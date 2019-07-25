#region using
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MpSoft.AspNetCore.AggregatedServices;
using System;
#endregion using

namespace Microsoft.Extensions.DependencyInjection
{
	public static class IServiceCollectionExtensions
	{
		public static AggregatedServicesBuilder AddAggregatedServices(this IServiceCollection services)
			=> AddAggregatedServices(services,null);

		public static AggregatedServicesBuilder AddAggregatedServices(this IServiceCollection services,Action<CreateOptions> createOptions)
		{
			if (createOptions!=null)
				services.Configure(createOptions);
			services.TryAddSingleton<IAggregatedServiceGenerator>(sp => new AggregatedServiceGenerator(sp.GetService<IOptions<CreateOptions>>()));
			return new AggregatedServicesBuilder(services);
		}
	}

	public class AggregatedServicesBuilder
	{
		IServiceCollection _services;
		internal AggregatedServicesBuilder(IServiceCollection services)
			=> _services=services;

		public IServiceCollection Services => _services;

		public AggregatedServicesBuilder AddAggregatedService<TService>(ServiceLifetime lifetime,GenerateOptions generateOptions)
			=> AddAggregatedService(typeof(TService),lifetime,generateOptions);

		readonly static Type[] _parmTypes = new[] { typeof(IServiceProvider) };
		public AggregatedServicesBuilder AddAggregatedService(Type serviceType,ServiceLifetime lifetime,GenerateOptions generateOptions)
		{
			_services.Add(new ServiceDescriptor(serviceType,sp => sp.GetRequiredService<IAggregatedServiceGenerator>().Generate(serviceType,generateOptions).GetConstructor(_parmTypes)?.Invoke(new object[] { sp }),lifetime));
			return this;
		}
	}
}
