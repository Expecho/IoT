//using Microsoft.ApplicationInsights;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.DurableTask.Entities;
//using Newtonsoft.Json;

//namespace MqttFunction
//{
//    public static class CallerInfoContainer
//    {
//        [Function(nameof(CallerInfo))]
//        public static Task DispatchAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
//        {
//            return dispatcher.DispatchAsync(operation =>
//            {
//                if (operation.State.GetState(typeof(DateTime)) is null)
//                {
//                    operation.State.SetState(DateTime.MinValue);
//                }

//                switch (operation.Name.ToLowerInvariant())
//                {
//                    case "add":
//                        int state = operation.State.GetState<int>();
//                        state += operation.GetInput<int>();
//                        operation.State.SetState(state);
//                        return new(state);
//                    case "get":
//                        return new(operation.State.GetState<DateTime>());
//                    case "delete":
//                        operation.State.SetState(null);
//                        break;
//                }

//                return default;
//            });
//        }
//    }

//    public class CallerInfo : ITaskEntity
//    {
//        [JsonIgnore]
//        private readonly TelemetryClient telemetryClient;

//        public CallerInfo(TelemetryClient telemetryClient)
//        {
//            this.telemetryClient = telemetryClient;
//        }

//        public DateTime LastValueReceived { get; set; } = DateTime.MinValue;

//        public bool CheckConnectedStateStarted { get; set; }

//        public async Task UpdateLastMessageReceivedTimestampAsync(DateTime timestamp)
//        {
//            LastValueReceived = timestamp;

//            if (!CheckConnectedStateStarted)
//            {
//                await SendDisconnectedMqttMessageWhenThresholdReachedOrScheduleCheck();
//                CheckConnectedStateStarted = true;
//            }

//            if ((DateTime.Now - LastValueReceived).TotalMinutes >= 15)
//            {
//                CheckConnectedStateStarted = false;
//            }
//        }

//        public void Delete()
//        {
//            Entity.Current.DeleteState();
//            telemetryClient.TrackEvent("StateDeleted");
//        }

//        public async Task SendDisconnectedMqttMessageWhenThresholdReachedOrScheduleCheck()
//        {
//            if ((DateTime.Now - LastValueReceived).TotalMinutes >= 2)
//            {
//                telemetryClient.TrackEvent("TimeoutDetected");

//                try
//                {
//                    await PublishDisconnectedMessageAsync();
//                }
//                catch (Exception ex)
//                {
//                    telemetryClient.TrackException(ex);
//                }
//            }

//            Entity.Current.SignalEntity<ICallerInfo>(Entity.Current.EntityId, DateTime.Now.AddMinutes(2), async e => await e.SendDisconnectedMqttMessageWhenThresholdReachedOrScheduleCheck());
//            telemetryClient.TrackEvent("TimeoutDetectionScheduled");
//        }

//        [Function(nameof(CallerInfo))]
//        public static Task Run([EntityTrigger] TaskEntityDispatcher dispatcher)
//            => dispatcher.DispatchAsync(s => s.c<CallerInfo>();

//        private async Task PublishDisconnectedMessageAsync()
//        {
//            //var connectionString = new MqttConnectionString(Environment.GetEnvironmentVariable("MqttConnection"), "CustomConfiguration");

//            //var factory = new MqttFactory();
//            //var mqttClient = factory.CreateMqttClient();
//            //var options = new MqttClientOptionsBuilder()
//            //                    .WithClientId(connectionString.ClientId)
//            //                    .WithTcpServer(connectionString.Server)
//            //                    .WithCredentials(connectionString.Username, connectionString.Password)
//            //                    .Build();
//            //await mqttClient.ConnectAsync(options);

//            //var message = new MqttApplicationMessageBuilder()
//            //                .WithTopic("azsphere/status")
//            //                .WithPayload("disconnected")
//            //                .Build();

//            //await mqttClient.PublishAsync(message, CancellationToken.None);
//            telemetryClient.TrackEvent("DisconnectedMessagePublished");
//        }

//        public ValueTask<object?> RunAsync(TaskEntityOperation operation)
//        {
//            operation.Context.SignalEntity(Entity.Current.EntityId, "UpdateLastMessageReceivedTimestampAsync", DateTime.Now);
//        }
//    }
//}