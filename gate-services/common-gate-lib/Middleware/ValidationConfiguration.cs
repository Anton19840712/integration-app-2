
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using CommonGateLib.Models;
using CommonGateLib.Validation;

namespace CommonGateLib.Middleware
{
    public static class ValidationConfiguration
    {
        public static IServiceCollection AddValidationServices(this IServiceCollection services)
        {
            services.AddScoped<IServerInstanceFluentValidator, ServerInstanceFluentValidator>();
            services.AddScoped<IValidator<ServerInstanceModel>, ServerInstanceModelValidator>();
            return services;
        }
    }
}
