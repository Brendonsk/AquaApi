namespace MqttApiPg
{
    public class MqttServiceConfiguration
    {
        public int Port { get; set; } = 1883;
        public List<User> Users { get; set; } = new();
        public int DelayInMilliSeconds { get; set; } = 30000;
        public int TlsPort { get; set; } = 8883;

        public bool isValid()
        {
            if (this.Port is <= 0 or > 65535)
            {
                throw new Exception("Invalid port");
            }

            if (!this.Users.Any())
            {
                throw new Exception("Invalid users");
            }

            if (this.DelayInMilliSeconds <= 0)
            {
                throw new Exception("The heartbeat delay is invalid");
            }

            if (this.TlsPort is <= 0 or > 65535)
            {
                throw new Exception("Invalid TLS port");
            }

            return true;
        }
    }
}
