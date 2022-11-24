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
            throw new NotImplementedException();
        }
    }
}
