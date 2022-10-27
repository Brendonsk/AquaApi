using System.ComponentModel.DataAnnotations;

namespace MqttApiPg
{
    public class MqttServiceOptions
    {
        [Range(1, 65535, ErrorMessage = "Invalid port")]
        public int Port { get; set; } = 1883;

        [Range(1, 65535, ErrorMessage = "Invalid TLS port")]
        public int TlsPort { get; set; } = 8883;
    }
}
