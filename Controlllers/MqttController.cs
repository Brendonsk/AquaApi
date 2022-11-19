using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MQTTnet.AspNetCore.Routing;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Internal;

namespace MqttApiPg.Controlllers
{
    [MqttController]
    [MqttRoute("[controller]")]
    public class MqttController : MqttBaseController
    {
        readonly MqttService _mqttService;
        readonly ILogger<MqttController> _logger;
        public MqttController(MqttService mqttService, ILogger<MqttController> logger)
        {
            this._mqttService = mqttService;
            _logger = logger;
        }

        [HttpPost]
        [MqttRoute("valvula/abrir")]
        public Task AbreValvula()
        {
            //MqttContext.ApplicationMessage.Topic
            if (_mqttService.mqttServer is not null && _mqttService.mqttServer?.IsStarted == true)
            {
                try
                {
                    var resposta = _mqttService.mqttServer.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(new MqttApplicationMessageBuilder()
                        .WithTopic("valvulaLog")
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                        .WithPayload($"{MqttContext.ClientId}: {MqttContext.ApplicationMessage}")
                        .Build())
                    );

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
