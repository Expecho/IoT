using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Diagnostics;
using MqttFunction;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddTransient<IMqttNetLogger, MqttNetNullLogger>();
builder.Services.AddTransient<MqttFactory>();
builder.Services.AddTransient<MqttPublisher>();
builder.Services.AddOptions<MqttOptions>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        configuration.GetSection(MqttOptions.ConnectionInfo).Bind(settings);
    });

builder.Build().Run();
