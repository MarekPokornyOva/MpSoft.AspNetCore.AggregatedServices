#region using
using System;
using System.Threading.Tasks;
#endregion using

namespace MpSoft.AspNetCore.AggregatedServices
{
	public interface IAggregatedServiceGenerator
	{
		Task<Type> GenerateAsync(Type serviceType,GenerateOptions options);
		Type Generate(Type serviceType,GenerateOptions options);
	}

	public static class IAggregatedServiceGeneratorExtensions
	{
		public static Task<Type> GenerateAsync<TService>(this IAggregatedServiceGenerator service,GenerateOptions options)
			=> service.GenerateAsync(typeof(TService),options);

		public static Type Generate<TService>(this IAggregatedServiceGenerator service,GenerateOptions options)
			=> service.Generate(typeof(TService),options);
	}
}
