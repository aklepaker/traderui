using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Diagnostics;
using traderui.Server.Hubs;
using traderui.Server.IBKR;

var builder = WebApplication.CreateBuilder(args);

var logConfiguration = builder.Configuration
    .SetBasePath(Environment.CurrentDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(logConfiguration);
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(logConfiguration)
    .CreateLogger();

var version = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName);
Log.Information($"{version.ProductName} v{version.ProductVersion}");

builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection(nameof(ServerOptions)));
builder.Services.AddMediatR(typeof(Program));
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IInteractiveBrokers, InteractiveBrokers>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();
// app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.UseCors(options =>
{
    options
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed(origin => true)
        .AllowCredentials()
        .Build();
});

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.MapHub<BrokerHub>("/hub/broker");
app.MapFallbackToFile("index.html");
app.Run();
