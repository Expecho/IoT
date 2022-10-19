using System;
using System.Text;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace MqttFunction
{
    public static class AzureSphere
    {
        [FunctionName("AzureSphere")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            [DurableClient] IDurableEntityClient entityClient,
            [Mqtt(typeof(MqttConfigFactory))] ICollector<IMqttMessage> outMessages)
        {
            if (req.QueryString.Value.Contains("delete", StringComparison.InvariantCultureIgnoreCase))
            {
                await entityClient.SignalEntityAsync<ICallerInfo>(new EntityId(nameof(CallerInfo), "sphere01"), e => e.Delete());
                return new AcceptedResult();
            }

            var formDatas = await req.ReadFormAsync();
            if(formDatas == null)
            {
                log.LogWarning("Invalid request, no form data found.");
                return new BadRequestResult();
            }

            outMessages.Add(
                    new MqttMessage("azsphere/status",
                        Encoding.UTF8.GetBytes("connected"),
                        MqttQualityOfServiceLevel.AtLeastOnce,
                        false));

            await entityClient.SignalEntityAsync<ICallerInfo>(new EntityId(nameof(CallerInfo), "sphere01"), e => e.UpdateLastMessageReceivedTimestampAsync(DateTime.Now));
            
            foreach (var formData in formDatas.Keys)
            {
                outMessages.Add(
                    new MqttMessage($"sensor/{formData.ToLowerInvariant()}",
                        Encoding.UTF8.GetBytes($"{formDatas[formData]}"),
                        MqttQualityOfServiceLevel.AtLeastOnce,
                        true));
            }

            return new OkResult();
        }
    }
}
