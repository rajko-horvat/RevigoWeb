using Microsoft.AspNetCore.Rewrite;
using IRB.RevigoWeb;

internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddRazorPages();

		var app = builder.Build();

        var options = new RewriteOptions();
        options.Rules.Add(new RewriteAspxUrl());
        app.UseRewriter(options);

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error");
		}

		app.UseStatusCodePagesWithReExecute("/ErrorCode", "?StatusCode={0}");

		app.UseStaticFiles();
		//app.UseCookiePolicy();
		app.UseRouting();
		//app.UseAuthorization();
		app.MapRazorPages();

		// intialize allication global state
		Global.ApplicationStart(app.Configuration);

		app.Run();

		Global.ApplicationEnd();
	}
}