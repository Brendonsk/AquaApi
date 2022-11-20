using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MQTTnet.AspNetCore.Routing;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Internal;

namespace MqttApiPg.MqttControllers
{
    [MqttController]
    [MqttRoute]
    public class MqttController : MqttBaseController
    {
        readonly ILogger<MqttController> _logger;
        private MqttService _mqttService;
        public MqttController(ILogger<MqttController> logger, MqttService mqttService)
        {
            _logger = logger;
            _mqttService = mqttService;
        }

        [MqttRoute("valvula/abrir")]
        public Task AbreValvula()
        {
            if (_mqttService.mqttServer is not null && _mqttService.mqttServer?.IsStarted == true)
            {
                try
                {
                    var resposta =
                        new InjectedMqttApplicationMessage(new MqttApplicationMessageBuilder()
                            .WithTopic("valvulaLog")
                            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                            .WithPayload($"{MqttContext.ClientId}: {Encoding.UTF8.GetString(MqttContext.ApplicationMessage.Payload)}")
                            .Build()
                        );

                    resposta.SenderClientId = _mqttService.ClientId;
                    _mqttService.mqttServer.InjectApplicationMessage(resposta);

                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError("{ex}", ex.ToString());
                    return Task.FromException(ex);
                }
            }

            return Task.FromException(new Exception("Server not started"));
        }
    }
}
