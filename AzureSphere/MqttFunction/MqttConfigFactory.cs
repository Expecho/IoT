using System;
using System.Text;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MqttQualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel;

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
                    .WithWillDelayInterval(30)
                    .WithWillMessage(new MqttApplicationMessage
                    {
                        Topic = "azsphere/status",
                        Payload = Encoding.UTF8.GetBytes("disconnected"),
                        QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                        Retain = false
                    })
                    .Build())
                .Build();

            return new MqttConfig("CustomConnection", options);
        }
    }
}