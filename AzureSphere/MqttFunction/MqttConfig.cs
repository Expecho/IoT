using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using MQTTnet.Extensions.ManagedClient;

namespace MqttFunction
{
    public class MqttConfig : CustomMqttConfig
    {
        public override IManagedMqttClientOptions Options { get; }

        public override string Name { get; }

        public MqttConfig(string name, IManagedMqttClientOptions options)
        {
            Options = options;
            Name = name;
        }
    }
}