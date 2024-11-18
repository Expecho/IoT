using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace MqttFunction
{
    public record MqttMessage(string Topic, byte[] Payload);

    public class MqttPublisher(MqttFactory mqttFactory, IOptions<MqttOptions> mqttOptions)
    {
        public async Task PublishAsync(IEnumerable<MqttMessage> messages)
        {
            using var mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(mqttOptions.Value.Server, mqttOptions.Value.Port)
                .WithCredentials(mqttOptions.Value.Username, mqttOptions.Value.Password)
                .Build();

            try
            {
                await mqttClient.ConnectAsync(mqttClientOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to the MQTT broker: {ex.Message}");
            }

            foreach (var message in messages)
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(message.Topic)
                    .WithPayload(message.Payload)
                    .Build();
                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
            }

            await mqttClient.DisconnectAsync();
        }
    }
}