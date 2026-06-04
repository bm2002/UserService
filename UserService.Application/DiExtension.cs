using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Services;

namespace UserService.Application
{
    public static class DiExtension
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // registration servces Application layer
            services.AddScoped<IUserService, UserAppService>();
            return services;
        }
    }
}
