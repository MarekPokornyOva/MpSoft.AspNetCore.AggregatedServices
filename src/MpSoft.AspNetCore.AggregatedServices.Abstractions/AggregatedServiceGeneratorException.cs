#region using
using System;
#endregion using

namespace MpSoft.AspNetCore.AggregatedServices
{
	public abstract class AggregatedServiceGeneratorException:Exception
	{
		protected AggregatedServiceGeneratorException():base()
		{ }

		protected AggregatedServiceGeneratorException(string message) : base(message)
		{ }

		protected AggregatedServiceGeneratorException(string message,Exception innerException) : base(message,innerException)
		{ }
	}
}
