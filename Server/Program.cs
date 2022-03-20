using Microsoft.AspNetCore.Mvc;
using Serilog;
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

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<InteractiveBrokers>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddSignalR();

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
