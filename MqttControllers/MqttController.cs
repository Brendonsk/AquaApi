using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MQTTnet.AspNetCore.Routing;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Internal;

namespace MqttApiPg.MqttControllers
{
    [MqttController]
    [MqttRoute("[controller]")]
    public class MqttController : MqttBaseController
    {
        readonly ILogger<MqttController> _logger;
        public MqttController(ILogger<MqttController> logger)
        {
            _logger = logger;
        }

        [MqttRoute("valvula/abrir")]
        public Task AbreValvula()
        {
            //MqttContext.ApplicationMessage.Topic
            var payload = MqttContext.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(MqttContext.ApplicationMessage.Payload);

            this._logger.LogInformation(
                "Message: ClientId = {ClientId}, Topic = {Topic}, Payload = {Payload}, QoS = {QoS}, Retain-Flag = {RetainFlag}",
                MqttContext.ClientId,
                MqttContext.ApplicationMessage?.Topic,
                payload,
                MqttContext.ApplicationMessage?.QualityOfServiceLevel,
                MqttContext.ApplicationMessage?.Retain);
            //if (_mqttService.mqttServer is not null && _mqttService.mqttServer?.IsStarted == true)
            //{
            //    try
            //    {
            //        var resposta = _mqttService.mqttServer.InjectApplicationMessage(
            //        new InjectedMqttApplicationMessage(new MqttApplicationMessageBuilder()
            //            .WithTopic("valvulaLog")
            //            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
            //            .WithPayload($"{MqttContext.ClientId}: {MqttContext.ApplicationMessage}")
            //            .Build())
            //        );

            //        return Ok();
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("{ex}", ex.ToString());
            //        return Task.FromException(ex);
            //    }
            //}

            return Task.FromException(new Exception("Server not started"));
        }
    }
}
