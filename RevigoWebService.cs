#if WINDOWS_SERVICE
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Rewrite;
using System.ServiceProcess;

namespace IRB.RevigoWeb
{
	public class RevigoWebService : ServiceBase
	{
		private WebApplicationBuilder oBuilder;
		private WebApplication oWebApplication;

		public RevigoWebService()
		{
			if (OperatingSystem.IsWindows())
			{
				this.ServiceName = "Revigo Web Service";
				this.CanHandlePowerEvent = false;
				this.CanShutdown = false;
				this.CanStop = true;
				this.CanPauseAndContinue = false;
			}

			// not really needed, but for nullable sake
			oBuilder = WebApplication.CreateBuilder();
			oWebApplication = WebApplication.Create();
		}

		protected override void OnStart(string[] args)
		{
			oBuilder = WebApplication.CreateBuilder(args);

			oBuilder.Services.Configure<FormOptions>(options =>
			{
				options.KeyLengthLimit = 8192;
				options.ValueCountLimit = 4096;
				options.ValueLengthLimit = 8388608;
			});

			// Add services to the container.
			oBuilder.Services.AddRazorPages();

			oWebApplication = oBuilder.Build();

			var options = new RewriteOptions();
			options.Rules.Add(new RewriteAspxUrl());
			oWebApplication.UseRewriter(options);

			// Configure the HTTP request pipeline.
			if (!oWebApplication.Environment.IsDevelopment())
			{
				oWebApplication.UseExceptionHandler("/Error");
			}
			else
			{
				//app.UseExceptionHandler("/Error");
				oWebApplication.UseDeveloperExceptionPage();
			}

			oWebApplication.UseStatusCodePagesWithReExecute("/ErrorCode", "?StatusCode={0}");

			oWebApplication.UseStaticFiles();
			//app.UseCookiePolicy();
			oWebApplication.UseRouting();
			//app.UseAuthorization();
			oWebApplication.MapRazorPages();

			oWebApplication.Lifetime.ApplicationStarted.Register(OnStarted);
			oWebApplication.Lifetime.ApplicationStopped.Register(OnStopped);

			oWebApplication.RunAsync();
		}

		private void OnStarted()
		{
			Global.StartApplication(oWebApplication.Configuration);
		}

		private void OnStopped()
		{
			Global.StopApplication();
		}

		protected override void OnStop()
		{
			Task oTask = oWebApplication.StopAsync(TimeSpan.FromMinutes(1.0));
			oTask.Wait();
		}
	}
}
#endif