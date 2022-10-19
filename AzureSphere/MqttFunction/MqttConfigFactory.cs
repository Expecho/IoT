using System;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

namespace MqttFunction
{
    public class MqttConfigFactory : ICreateMqttConfig
    {
        public CustomMqttConfig Create(INameResolver nameResolver, ILogger logger)
        {
            var connectionString = new MqttConnectionString(nameResolver.Resolve("MqttConnection"), "CustomConfiguration");

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(connectionString.ClientId)
                    .WithTcpServer(connectionString.Server, connectionString.Port)
                    .WithCredentials(connectionString.Username, connectionString.Password)
                    .Build())
                .Build();

            return new MqttConfig("CustomConnection", options);
        }
    }
}