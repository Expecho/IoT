namespace MqttFunction
{
    public class MqttOptions
    {
        public const string ConnectionInfo = "MqttConnection";

        public string Server { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
    }
}
