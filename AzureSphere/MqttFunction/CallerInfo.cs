using System;
using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;

namespace MqttFunction
{
    public interface ICallerInfo
    {
        Task UpdateLastMessageReceivedTimestampAsync(DateTime timestamp);
        Task SendDisconnectedMqttMessageWhenThresholdReachedOrScheduleCheck();
        void Delete();
    }

    public class CallerInfo : ICallerInfo
    {
        [JsonIgnore]
        private readonly TelemetryClient telemetryClient;

        public CallerInfo(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        public DateTime LastValueReceived { get; set; } = DateTime.MinValue;

        public bool CheckConnectedStateStarted { get; set; }

        public async Task UpdateLastMessageReceivedTimestampAsync(DateTime timestamp)
        {
            LastValueReceived = timestamp;

            if (!CheckConnectedStateStarted)
            {
                await SendDisconnectedMqttMessageWhenThresholdReachedOrScheduleCheck();
                CheckConnectedStateStarted = true;
            }

            if ((DateTime.Now - LastValueReceived).TotalMinutes >= 15)
                CheckConnectedStateStarted = false;
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
            telemetryClient.TrackEvent("StateDeleted");
        }

        public async Task SendDisconnectedMqttMessageWhenThresholdReachedOrScheduleCheck()
        {
            if ((DateTime.Now - LastValueReceived).TotalMinutes >= 2)
            {
                telemetryClient.TrackEvent("TimeoutDetected");

                try
                {
                    await PublishDisconnectedMessageAsync();
                }
                catch (Exception ex)
                {
                    telemetryClient.TrackException(ex);
                }
            }

            Entity.Current.SignalEntity<ICallerInfo>(Entity.Current.EntityId, DateTime.Now.AddMinutes(2), async e => await e.SendDisconnectedMqttMessageWhenThresholdReachedOrScheduleCheck());
            telemetryClient.TrackEvent("TimeoutDetectionScheduled");
        }

        [FunctionName(nameof(CallerInfo))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<CallerInfo>();

        private async Task PublishDisconnectedMessageAsync()
        {
            var connectionString = new MqttConnectionString(Environment.GetEnvironmentVariable("MqttConnection"), "CustomConfiguration");

            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
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
            telemetryClient.TrackEvent("DisconnectedMessagePublished");
        }
    }
}