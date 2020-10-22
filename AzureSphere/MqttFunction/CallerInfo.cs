using System;
using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace MqttFunction
{
    public interface ICallerInfo
    {
        DateTime LastValueReceived { get; set; }

        void Update(DateTime timestamp);
    }

    public class CallerInfo : ICallerInfo
    {
        public DateTime LastValueReceived { get; set; } = DateTime.MinValue;

        public void Update(DateTime timestamp)
        {
            LastValueReceived = timestamp;
        }

        public async Task CheckConnectedStateAsync()
        {
            if ((DateTime.Now - LastValueReceived).TotalMinutes >= 5)
                await PublishDisconnectedMessageAsync();

            Entity.Current.SignalEntity<ICallerInfo>(Entity.Current.EntityId, DateTime.Now.AddMinutes(5), async e => await CheckConnectedStateAsync());
        }

        [FunctionName(nameof(CallerInfo))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<CallerInfo>();

        private async Task PublishDisconnectedMessageAsync()
        {
            var connectionString = new MqttConnectionString(Environment.GetEnvironmentVariable("MqttConnection"), "CustomConfiguration");

            var factory = new MqttFactory();
            using var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                                .WithClientId(connectionString.ClientId)
                                .WithTcpServer(connectionString.Server)
                                .WithCredentials(connectionString.Username, connectionString.Password)
                                .Build();
            await mqttClient.ConnectAsync(options);

            var message = new MqttApplicationMessageBuilder()
                            .WithTopic("azsphere/status")
                            .WithPayload("disconnected")
                            .Build();

            await mqttClient.PublishAsync(message, CancellationToken.None);
        }
    }
}
