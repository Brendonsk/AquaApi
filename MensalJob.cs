using MqttApiPg.Services;
using Quartz;
using System.Net.Mime;

namespace MqttApiPg
{
    public class MensalJob : IJob
    {
        private MensalService _mensalService;
        private ILogger<MensalJob> _logger;
        public MensalJob(MensalService mensalService, ILogger<MensalJob> logger)
        {
            _mensalService = mensalService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Iniciando job para registro de consumo mensal...");
            await _mensalService.SalvaConsumoMesAnterior();
        }
    }
}
