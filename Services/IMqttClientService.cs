using MQTTnet.Client;

namespace MqttApiPg.Services
{
    public interface IMqttClientService: IHostedService
    {
        public IMqttClient mqttClient { get; set; }
    }
}
