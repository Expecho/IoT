using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MqttFunction;
using MQTTnet;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddTransient<MqttPublisher>();
        services.AddOptions<MqttOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(MqttOptions.ConnectionInfo).Bind(settings);
            });
        services.AddTransient<MqttFactory>();
    })
    .Build();

host.Run();