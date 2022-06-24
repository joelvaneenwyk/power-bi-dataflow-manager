using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.DataFlow.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;


[assembly: FunctionsStartup(typeof(Microsoft.DataFlow.EmergencyBrake.Startup))]
namespace Microsoft.DataFlow.EmergencyBrake
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);

            builder.Services.AddScoped<IAuthService,AuthService>();

            builder.Services.AddTransient<IDataFlowService, DataFlowService>();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EmergencyBrake", Version = "v1" });
            });
        }
    }
}
