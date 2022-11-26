using MQTTnet;
using System.Text;

namespace MqttApiPg.Services
{
    public class ValvulaService
    {
        private readonly IMqttClientService mqttClientService;

        public ValvulaService(MqttClientServiceProvider provider)
        {
            this.mqttClientService = provider.MqttClientService;
        }

        public async Task FechaValvula()
        {
            await this.mqttClientService.mqttClient.PublishAsync(
                new MqttApplicationMessageBuilder()
                    .WithTopic("Rele")
                    .WithPayload(Encoding.UTF8.GetBytes("4"))
                    .Build()
            );
        }
        public async Task AbreValvula()
        {
            var res = await this.mqttClientService.mqttClient.PublishAsync(
                new MqttApplicationMessageBuilder()
                    .WithTopic("Rele")
                    .WithPayload(Encoding.UTF8.GetBytes("1"))
                    .Build()
            );
        }
    }
}
