using System;
using System.Text;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace MqttFunction
{
    public static class AzureSphere
    {
        [FunctionName("AzureSphere")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            [Mqtt(typeof(MqttConfigFactory))] ICollector<IMqttMessage> outMessages)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var formDatas = await req.ReadFormAsync();

            outMessages.Add(
                    new MqttMessage("azsphere/status",
                        Encoding.UTF8.GetBytes("connected"),
                        MqttQualityOfServiceLevel.AtLeastOnce,
                        false));

            using (var deviceClient = DeviceClient.CreateFromConnectionString(Environment.GetEnvironmentVariable("IoTCentral_CS"), TransportType.Mqtt))
            {
                foreach (var formData in formDatas.Keys)
                {
                    outMessages.Add(
                        new MqttMessage($"sensor/{formData.ToLowerInvariant()}",
                            Encoding.UTF8.GetBytes($"{formDatas[formData]}"),
                            MqttQualityOfServiceLevel.AtLeastOnce,
                            true));

                    var messageString = "{ \"" + formData.ToLowerInvariant() + "\": " + formDatas[formData] + " }";
                    var message = new Message(Encoding.ASCII.GetBytes(messageString));
                    
                    await deviceClient.SendEventAsync(message);
                }
            }

            return new OkResult();
        }
    }
}
