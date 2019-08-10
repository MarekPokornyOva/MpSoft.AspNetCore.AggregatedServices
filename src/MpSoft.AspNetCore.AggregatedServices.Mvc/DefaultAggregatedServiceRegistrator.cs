#region using
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
#endregion using

namespace MpSoft.AspNetCore.AggregatedServices.Mvc
{
	public class DefaultAggregatedServiceRegistrator:IAggregatedServiceRegistrator
	{
		readonly AggregatedServicesBuilder _builder;
		readonly GenerateOptions _generateOptions;

		public DefaultAggregatedServiceRegistrator(AggregatedServicesBuilder builder):this(builder,new GenerateOptions { ResolveMode=ResolveMode.OnDemandCached,ServicesRequired=false })
		{ }

		public DefaultAggregatedServiceRegistrator(AggregatedServicesBuilder builder,GenerateOptions generateOptions)
		{
			_builder=builder;
			_generateOptions=generateOptions;
		}

		static Type _attrType=typeof(AggregatedServiceAttribute);
		public void Register(Type serviceType,IAggregatedServiceRegisterContext context)
		{
			if (context.ParameterInfo.CustomAttributes.Any(x => _attrType.IsAssignableFrom(x.AttributeType)))
				if (!_builder.Services.Any(x=>x.ServiceType==serviceType))
					_builder.AddAggregatedService(serviceType,ServiceLifetime.Transient,_generateOptions);
				//warn when serviceType is already registered with different CreateOptions and/or GenerateOptions?
		}
	}
}
