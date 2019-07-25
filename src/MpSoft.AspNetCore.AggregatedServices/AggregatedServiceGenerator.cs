#region using
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
#endregion using

namespace MpSoft.AspNetCore.AggregatedServices
{

	public class AggregatedServiceGenerator:IAggregatedServiceGenerator
	{
		readonly IAggregatedServiceGenerator _generator;

		public AggregatedServiceGenerator(IOptions<CreateOptions> createOptions)
			=> _generator=new JellequinAggregatedServiceGenerator(createOptions?.Value);

		public Task<Type> GenerateAsync(Type serviceType,GenerateOptions options)
			=> _generator.GenerateAsync(serviceType,options);

		public Type Generate(Type serviceType,GenerateOptions options)
			=> _generator.Generate(serviceType,options);
	}
}
