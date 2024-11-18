using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Diagnostics;

[assembly: FunctionsStartup(typeof(MqttFunction.Startup))]

namespace MqttFunction;

public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {

    }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();
        builder.Services.AddTransient<MqttPublisher>();
        builder.Services.AddTransient<IMqttNetLogger, MqttNetNullLogger>();
        builder.Services.AddTransient<MqttFactory>();
        builder.Services.AddTransient<MqttPublisher>();
        builder.Services.AddOptions<MqttOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(MqttOptions.ConnectionInfo).Bind(settings);
            });
    }
}
