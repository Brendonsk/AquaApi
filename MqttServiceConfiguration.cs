using System.ComponentModel.DataAnnotations;

namespace MqttApiPg
{
    public class MqttServiceConfiguration
    {
        public string serviceName { get; set; } = "MqttClientService";

        [Range(1, 65535)]
        public int Port { get; set; } = 1883;

        [MinLength(1)]
        public List<User> Users { get; set; } = new();

        [Range(1, int.MaxValue)]
        public int DelayInMilliSeconds { get; set; } = 30000;

        [Range(1, 65535)]
        public int TlsPort { get; set; } = 8883;
    }
}
