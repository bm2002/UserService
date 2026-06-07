using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UserService.AdminUi;
using UserService.AdminUi.Services;

namespace UserService.AdminUi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            string apiBaseUrl = builder.Configuration["ApiBaseUrl"]
                ?? throw new InvalidOperationException("ApiBaseUrl не настроен в wwwroot/appsettings.json");

            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
            builder.Services.AddScoped<IUserApiService, UserApiService>();

            await builder.Build().RunAsync();
        }
    }
}
