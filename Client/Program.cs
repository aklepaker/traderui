using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using traderui.Client;
using traderui.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});

builder.Services.AddHttpClient<IBrokerService, BrokerService>(client => { client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress); });
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAntDesign();

await builder.Build().RunAsync();
