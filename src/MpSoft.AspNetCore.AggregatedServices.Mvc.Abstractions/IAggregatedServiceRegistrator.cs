#region using
using System;
using System.Reflection;
#endregion using

namespace MpSoft.AspNetCore.AggregatedServices.Mvc
{
	public interface IAggregatedServiceRegistrator
	{
		void Register(Type serviceType,IAggregatedServiceRegisterContext context);
	}

	public interface IAggregatedServiceRegisterContext
	{
		ParameterInfo ParameterInfo { get; }
	}
}
