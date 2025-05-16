using FluentValidation;
using CommonGateLib.Validation;
using CommonGateLib.Models.Common;

namespace middleware
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
