#region using
//using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
//using Microsoft.AspNetCore.Mvc.Infrastructure;
//using Microsoft.Extensions.DependencyInjection.Extensions;
using MpSoft.AspNetCore.AggregatedServices;
using MpSoft.AspNetCore.AggregatedServices.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion using

namespace Microsoft.Extensions.DependencyInjection
{
	public static class IServiceCollectionExtensions
	{
		public static AggregatedServicesBuilder AddMvc(this AggregatedServicesBuilder builder)
			=> AddMvc(builder,options=> { });

		public static AggregatedServicesBuilder AddMvc(this AggregatedServicesBuilder builder,Action<AggMvcOptions> options)
		{
			AggMvcOptions opts = new AggMvcOptions();
			options(opts);
			IServiceCollection services = builder.Services;
			//services.AddSingleton<IAggregatedServiceRegistrator>(sp=>new DefaultAggregatedServiceRegistrator(builder));
			//services.TryAddEnumerable(ServiceDescriptor.Singleton<IActionDescriptorProvider>((Func<IServiceProvider,AggActionDescriptorProvider>)(sp => new AggActionDescriptorProvider(1000,sp.GetRequiredService<IAggregatedServiceRegistrator>()))));

			//I have to force IActionDescriptorCollectionProvider to enumerate IActionDescriptorProvider s and register aggregated services. Hopefully, that won't cause an issue.
			//ActionDescriptorCollection forget=services.BuildServiceProvider().GetRequiredService<IActionDescriptorCollectionProvider>().ActionDescriptors;
			//Maybe it would be enough to call AggActionDescriptorProvider.FindAndRegister directly. How to let customize IAggregatedServiceRegistrator?
			//AggActionDescriptorProvider.FindAndRegister(forget.Items,new DefaultAggregatedServiceRegistrator(builder));

			ApplicationPartManager appPartManager = GetServiceFromCollection<ApplicationPartManager>(services);
			ControllerFeature controllerFeature = new ControllerFeature();
			appPartManager.PopulateFeature(controllerFeature);
			AggActionDescriptorProvider.FindAndRegister(controllerFeature.Controllers,opts.Registrator??new DefaultAggregatedServiceRegistrator(builder));

			return builder;
		}

		static T GetServiceFromCollection<T>(IServiceCollection services)
			=> (T)((IEnumerable<ServiceDescriptor>)services).LastOrDefault((ServiceDescriptor d) => d.ServiceType==typeof(T))?.ImplementationInstance;
	}
}
