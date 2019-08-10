#region using
using System.Reflection;
#endregion using

namespace MpSoft.AspNetCore.AggregatedServices.Mvc
{
	class DefaultAggregatedServiceRegisterContext:IAggregatedServiceRegisterContext
	{
		public ParameterInfo ParameterInfo { get; internal set; }
	}
}
