using Abbanks.TodoApp.Application.Extensions;
using Abbanks.TodoApp.Application.Mappings;
using Abbanks.TodoApp.Application.Services;
using Abbanks.TodoApp.Application.Services.Implementations;
using Abbanks.TodoApp.Application.Settings;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Abbanks.TodoApp.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<ITodoService, TodoService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddAutoMapper(typeof(MappingProfile));

            services.AddApplicationValidation();
            services.AddFluentValidationAutoValidation();

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            return services;
        }
    }
}
