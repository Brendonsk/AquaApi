using System.ComponentModel.DataAnnotations;

namespace MqttApiPg
{
    public class MqttServiceOptions
    {
        [Range(1, 65535, ErrorMessage = "Invalid port")]
        public int Port { get; set; } = 1883;

        [MinLength(1, ErrorMessage = "Invalid users")]
        public List<User> Users { get; set; } = new();

        [Range(1, int.MaxValue, ErrorMessage = "The heartbeat delay is invalid")]
        public int DelayInMilliSeconds { get; set; } = 30000;

        [Range(1, 65535, ErrorMessage = "Invalid TLS port")]
        public int TlsPort { get; set; } = 8883;
    }
}
