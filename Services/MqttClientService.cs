using MongoDB.Driver;
using MqttApiPg.Entities;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;

namespace MqttApiPg.Services
{
    public class MqttClientService : IMqttClientService
    {
        private readonly MqttClientOptions options;
        private readonly ILogger<MqttClientService> _logger;
        private readonly string _clientId = "Heroku mqtt client";
        private readonly DiariaService _diariaService;
        private readonly MensalService _mensalService;
        private readonly RegistroService _registroService;

        public IMqttClient mqttClient { get; set; }

        public MqttClientService(MqttClientOptions options, ILogger<MqttClientService> logger, DiariaService diariaService, RegistroService registroService, MensalService mensalService)
        {
            this.options = options;
            _logger = logger;
            _diariaService = diariaService;
            _registroService = registroService;
            _mensalService = mensalService;

            mqttClient = new MqttFactory().CreateMqttClient();
            ConfigureMqttClient();
        }

        private void ConfigureMqttClient()
        {
            mqttClient.ConnectedAsync += HandleConnectedAsync;
            mqttClient.ApplicationMessageReceivedAsync += HandleApplicationMessageReceivedAsync;
        }

        private async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            try
            {
                this.LogMessage(args);
                if (!args.ClientId.Equals(this._clientId))
                {
                    var msg = args.ApplicationMessage;
                    switch (msg?.Topic)
                    {
                        case "pg":
                            if (decimal.TryParse(Encoding.UTF8.GetString(msg!.Payload), out decimal medida))
                            {
                                try
                                {
                                    var hoje = DateTime.Now;
                                    await _diariaService.CreateAsync(new Diaria()
                                    {
                                        Valor = medida,
                                        DiaHora = hoje
                                    });
                                    args.IsHandled = true;

                                    Mensal? ultimoMes = await _mensalService.GetPorMesEAno(hoje.Year, hoje.Month);

                                    decimal mediaConsumoPorDia = (ultimoMes?.ConsumoTotal) ?? 0 / DateTime.DaysInMonth(hoje.Year, hoje.Month);
                                    decimal mediaConsumoPorHora = mediaConsumoPorDia / 24;

                                    if (medida/1000 > mediaConsumoPorHora * (decimal)1.1)
                                    {
                                        try
                                        {
                                            await _registroService.CreateAsync(new Registro()
                                            {
                                                DataOcorrencia = hoje,
                                                Mensagem = "mockMsg",
                                                Decisao = false
                                            });
                                        }
                                        catch (Exception)
                                        {
                                            _logger.LogError("registro não pode ser inserido");
                                            throw;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    _logger.LogError("Erro ao inserir na tabela diária");
                                }
                            }
                            else
                            {
                                _logger.LogError("Formato da payload inválido");
                            }

                            break;

                        default:
                            _logger.LogError(
                                "Tópico inválido: {topico}",
                                msg?.Topic ?? "[nulo]");

                            break;
                    }
                }

                if (!args.IsHandled)
                {
                    this._logger.LogInformation("Mensagem do cliente interpretada com sucesso");
                }

                else
                {
                    this._logger.LogError("Ocorreu um erro ao interpretar a mensagem");
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError("Erro não identificado: {exception}", ex);
                throw;
            }
        }

        private void LogMessage(MqttApplicationMessageReceivedEventArgs args)
        {
            var payload = args.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

            this._logger.LogInformation(
                "Message: ClientId = {ClientId}, Topic = {Topic}, Payload = {Payload}, QoS = {QoS}, Retain-Flag = {RetainFlag}",
                args.ClientId,
                args.ApplicationMessage?.Topic,
                payload,
                args.ApplicationMessage?.QualityOfServiceLevel,
                args.ApplicationMessage?.Retain);
        }

        private async Task HandleConnectedAsync(MqttClientConnectedEventArgs args)
        {
            //this.LogMessage(args);
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("pg").WithExactlyOnceQoS().Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("Rele").WithExactlyOnceQoS().Build());
        }

        private void LogMessage(MqttClientConnectedEventArgs args)
        {
            var result = args.ConnectResult;
            this._logger.LogInformation(
                "Connected, code: {ResultCode}",
                result.ResultCode);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await mqttClient.ConnectAsync(options);

            _ = Task.Run(
               async () =>
               {
                   while (!cancellationToken.IsCancellationRequested)
                   {
                       try
                       {
                           if (!await mqttClient.TryPingAsync(cancellationToken))
                           {
                               var res = await mqttClient.ConnectAsync(mqttClient.Options, cancellationToken);
                               // Subscribe to topics when session is clean etc.
                               
                               //_logger.LogInformation("The MQTT client is connected.");
                           }
                       }
                       catch (Exception ex)
                       {
                           //_logger.LogError(ex, "The MQTT client  connection failed");
                       }
                       finally
                       {
                           await Task.Delay(TimeSpan.FromSeconds(3));
                       }
                   }
               }
            );
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                var disconnectOption = new MqttClientDisconnectOptions
                {
                    Reason = MqttClientDisconnectReason.NormalDisconnection,
                    ReasonString = "NormalDiconnection"
                };
                await mqttClient.DisconnectAsync(disconnectOption, cancellationToken);
            }
            await mqttClient.DisconnectAsync();
        }

        
    }
}
