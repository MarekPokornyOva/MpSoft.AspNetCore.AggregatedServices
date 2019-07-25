﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MpSoft.AspNetCore.AggregatedServices;

namespace Web
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration=configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();

			services.AddSingleton<IMyService1,MyService1>();
			//services.AddTransient<IMyService2,MyService2>();
			services.AddAggregatedServices()
				.AddAggregatedService<IMyAggregatedService>(ServiceLifetime.Transient,new GenerateOptions { ResolveMode=ResolveMode.OnDemandCached,ServicesRequired=false });
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app,IHostingEnvironment env)
		{
			app.UseMvc(routes =>
			{
				routes.MapRoute(
						 name: "default",
						 template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
