using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace MqttFunction
{
    public class AzureSphere(ILogger<AzureSphere> log, MqttPublisher mqttPublisher)
    {
        [Function("AzureSphere")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [DurableClient] DurableTaskClient entityClient)
        {
            //if (req.QueryString!.Value!.Contains("delete", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    var entityId = new EntityInstanceId(nameof(CallerInfo), "sphere01");
            //    await entityClient.Entities.SignalEntityAsync(entityId, "Delete");
            //    return new AcceptedResult();
            //}

            var formDatas = await req.ReadFormAsync();
            if (formDatas == null)
            {
                log.LogWarning("Invalid request, no form data found.");
                return new BadRequestResult();
            }

            var messages = formDatas.Keys
                .Select(formData =>
                    new MqttMessage($"sensor/{formData.ToLowerInvariant()}", Encoding.UTF8.GetBytes(formDatas[formData]!)));

            messages = messages.Append(new MqttMessage("azsphere/status", Encoding.UTF8.GetBytes("connected")));

            await mqttPublisher.PublishAsync(messages);

            return new OkResult();
        }
    }
}
