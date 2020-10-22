﻿using System;
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
        Task UpdateAsync(DateTime timestamp);
        Task CheckConnectedStateAsync();
        void Delete();
    }

    public class CallerInfo : ICallerInfo
    {
        public DateTime LastValueReceived { get; set; } = DateTime.MinValue;

        public bool CheckConnectedStateStarted { get; set; }

        public async Task UpdateAsync(DateTime timestamp)
        {
            LastValueReceived = timestamp;

            if (!CheckConnectedStateStarted)
            {
                await CheckConnectedStateAsync();
                CheckConnectedStateStarted = true;
            }
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        public async Task CheckConnectedStateAsync()
        {
            if ((DateTime.Now - LastValueReceived).TotalMinutes >= 2)
                await PublishDisconnectedMessageAsync();
            
            Entity.Current.SignalEntity<ICallerInfo>(Entity.Current.EntityId, DateTime.Now.AddMinutes(2), async e => await e.CheckConnectedStateAsync());
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