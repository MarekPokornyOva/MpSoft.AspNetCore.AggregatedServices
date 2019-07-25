#region using
using Microsoft.Extensions.DependencyInjection;
using MpSoft.AspNetCore.AggregatedServices;
using System;
#endregion using

namespace SimpleSample
{
	class Program
	{
		static void Main(string[] args)
		{
			IServiceCollection services = new ServiceCollection();
			Configure(services);
			IServiceProvider sp = services.BuildServiceProvider();
			IMyAggregatedService inst = sp.GetService<IMyAggregatedService>();

			Console.WriteLine("SimpleSample - demonstrates aggregated services generation.");
			Console.WriteLine("===========");
			Console.WriteLine("Service #1: "+(inst.Service1?.DoSomething()??"Unavailable. Please uncomment source code line 26."));
			Console.WriteLine("Service #2: "+(inst.Service2?.DoSomethingElse()??"Unavailable. Please uncomment source code line 27."));
		}

		static void Configure(IServiceCollection services)
		{
			services.AddSingleton<IMyService1,MyService1>();
			//services.AddTransient<IMyService2,MyService2>();
			services.AddAggregatedServices()
				.AddAggregatedService<IMyAggregatedService>(ServiceLifetime.Transient,new GenerateOptions { ResolveMode=ResolveMode.OnDemandCached,ServicesRequired=false });
		}
	}

	public interface IMyService1
	{
		string DoSomething();
	}

	public interface IMyService2
	{
		string DoSomethingElse();
	}

	public interface IMyAggregatedService
	{
		IMyService1 Service1 { get; }
		IMyService2 Service2 { get; }
	}

	class MyService1:IMyService1
	{
		public string DoSomething()
			=> "Service is up & running.";
	}

	class MyService2:IMyService2
	{
		public string DoSomethingElse()
			=> "Service is up & running.";
	}
}
