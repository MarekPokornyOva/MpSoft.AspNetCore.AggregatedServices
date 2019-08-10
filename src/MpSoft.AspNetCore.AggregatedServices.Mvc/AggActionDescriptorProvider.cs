#region using
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endregion using

namespace MpSoft.AspNetCore.AggregatedServices.Mvc
{
	class AggActionDescriptorProvider:IActionDescriptorProvider
	{
		readonly IAggregatedServiceRegistrator _serviceRegistrator;

		internal AggActionDescriptorProvider(int order,IAggregatedServiceRegistrator serviceRegistrator)
		{
			Order=order;
			_serviceRegistrator=serviceRegistrator;
		}

		public int Order { get; }

		public void OnProvidersExecuted(ActionDescriptorProviderContext context)
		{ }

		public void OnProvidersExecuting(ActionDescriptorProviderContext context)
			=> FindAndRegister(context.Results,_serviceRegistrator);

		internal static void FindAndRegister(IEnumerable<ActionDescriptor> actionDescriptors,IAggregatedServiceRegistrator serviceRegistrator)
			=> FindAndRegister(actionDescriptors.OfType<ControllerActionDescriptor>().Select(x => x.ControllerTypeInfo).Distinct(),serviceRegistrator);

		internal static void FindAndRegister(IEnumerable<TypeInfo> controllerTypes,IAggregatedServiceRegistrator serviceRegistrator)
		{
			IEnumerable<(Type ServiceType, IEnumerable<ParameterInfo> Parameters)> parameterTypes =
				controllerTypes.SelectMany(controllerType => controllerType.GetConstructors().Where(x => (!x.IsStatic)&&x.IsPublic).SelectMany(x => x.GetParameters()))
				.GroupBy(x => x.ParameterType).Select(x => (x.Key, (IEnumerable<ParameterInfo>)x));
			IEnumerable<(Type ServiceType, IEnumerable<ParameterInfo> Parameters)> toRegister = parameterTypes/*.Where(x => JellequinAggregatedServiceGenerator.ValidateServiceType(x.ServiceType))*/;

			foreach ((Type serviceType, IEnumerable<ParameterInfo> parameters) in toRegister)
				foreach (ParameterInfo parameter in parameters)
					serviceRegistrator.Register(serviceType,new DefaultAggregatedServiceRegisterContext { ParameterInfo=parameter });
		}
	}
}
