using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MQTTnet.Extensions.ManagedClient;

namespace MqttApiPg
{
    [Route("api/[controller]")]
    [ApiController]
    public class MqttController : ControllerBase
    {
        MqttClientService mqttClientService;
        public MqttController(MqttClientService mqttClientService)
        {
            this.mqttClientService = mqttClientService;
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PublishMessage(string msg)
        {
            try
            {
                var result = await mqttClientService.mqttClient.InternalClient.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithTopic("brendon")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .WithPayload(msg)
                    .Build()
                );

                if (result.ReasonCode.Equals(MqttClientPublishReasonCode.Success))
                {
                    return new OkResult();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, result.ReasonCode.ToString());
            }
            catch (Exception ex)
            {
                mqttClientService.logger.Error(ex.ToString());
                throw;
            }
            
        }
    }
}
