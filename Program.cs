using Microsoft.AspNetCore.Rewrite;
using IRB.RevigoWeb;
using System.Reflection;
#if WINDOWS_SERVICE
using System.ServiceProcess;
#endif

internal class Program
{
#if WORKER_SERVICE
	// To do: Implement Worker Service
#elif WINDOWS_SERVICE

	private static void Main()
	{
		// ensure that CWD is the assembly path to be able to access contents and configuration files
		Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

		ServiceBase[] ServicesToRun;
		ServicesToRun = new ServiceBase[]
		{
			new RevigoWebService()
		};
		ServiceBase.Run(ServicesToRun);
	}
#else

	private static WebApplication oWebApplication;

	private static void Main(string[] args)
	{
		// ensure that CWD is the assembly path to be able to access contents and configuration files
		Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddRazorPages();

		oWebApplication = builder.Build();

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

		oWebApplication.Run();
	}

	private static void OnStarted()
	{
		Global.StartApplication(oWebApplication.Configuration);
	}

	private static void OnStopped()
	{
		Global.StopApplication();
	}
#endif
}