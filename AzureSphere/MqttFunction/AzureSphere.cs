using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace MqttFunction
{
    public class AzureSphere(ILogger<AzureSphere> log, MqttPublisher mqttPublisher)
    {
        [FunctionName("AzureSphere")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [DurableClient] IDurableEntityClient entityClient)
        {
            //if (req.QueryString.Value.Contains("delete", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    await entityClient.SignalEntityAsync<ICallerInfo>(new EntityId(nameof(CallerInfo), "sphere01"), e => e.Delete());
            //    return new AcceptedResult();
            //}

            //await entityClient.SignalEntityAsync<ICallerInfo>(new EntityId(nameof(CallerInfo), "sphere01"), e => e.UpdateLastMessageReceivedTimestampAsync(DateTime.Now));

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
