﻿using MqttApiPg.Entities;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;

namespace MqttApiPg.Services
{
    public class MqttClientService : IMqttClientService
    {
        private readonly IMqttClient mqttClient;
        private readonly MqttClientOptions options;
        private readonly ILogger<MqttClientService> _logger;
        private readonly string _clientId = "Heroku mqtt client";
        private readonly DiariaService _diariaService;

        public MqttClientService(MqttClientOptions options, ILogger<MqttClientService> logger, DiariaService diariaService = null)
        {
            this.options = options;
            mqttClient = new MqttFactory().CreateMqttClient();
            _logger = logger;
            _diariaService = diariaService;
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
                if (!args.ClientId.Equals(this._clientId))
                {
                    var msg = args.ApplicationMessage;
                    if (msg?.Topic.Equals("pg") ?? false)
                    {
                        if (decimal.TryParse(Encoding.UTF8.GetString(msg!.Payload), out decimal medida))
                        {
                            await _diariaService.CreateAsync(new Diaria()
                            {
                                Valor = medida,
                                DiaHora = DateTime.Now,
                                Situacao = true
                            });
                            args.IsHandled = true;
                        }
                    }
                }

                if (!args.IsHandled)
                {
                    this._logger.LogInformation("a mensagem do cliente {client} foi interpretada com sucesso", args.ClientId);
                }
                else
                {
                    this._logger.LogError("ocorreu um erro ao interpretar a mensagem do cliente {client}", args.ClientId);
                }
                this.LogMessage(args);
            }
            catch (Exception ex)
            {
                this._logger.LogError("An error occurred: {exception}", ex);
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

            if (!args.ClientId.Equals(_clientId) && (args.ApplicationMessage?.Topic.Equals("valvula/abrir") ?? false))
            {
                mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithPayload("recebido chefe")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                    .WithTopic("valvulaLog")
                    .Build());
            }
        }

        private async Task HandleConnectedAsync(MqttClientConnectedEventArgs args)
        {
            this.LogMessage(args);
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("pg").Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("Rele").Build());
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

            #region Reconnect_Using_Timer:https://github.com/dotnet/MQTTnet/blob/master/Samples/Client/Client_Connection_Samples.cs
            /* 
             * This sample shows how to reconnect when the connection was dropped.
             * This approach uses a custom Task/Thread which will monitor the connection status.
            * This is the recommended way but requires more custom code!
           */
            _ = Task.Run(
               async () =>
               {
                   while (!cancellationToken.IsCancellationRequested)
                   {
                       try
                       {
                           // This code will also do the very first connect! So no call to _ConnectAsync_ is required in the first place.
                           if (!await mqttClient.TryPingAsync(cancellationToken))
                           {
                               await mqttClient.ConnectAsync(mqttClient.Options, cancellationToken);

                               // Subscribe to topics when session is clean etc.
                               
                               _logger.LogInformation("The MQTT client is connected.");
                           }
                       }
                       catch (Exception ex)
                       {
                           // Handle the exception properly (logging etc.).
                           _logger.LogError(ex, "The MQTT client  connection failed");
                       }
                       finally
                       {
                           // Check the connection state every 3 seconds and perform a reconnect if required.
                           await Task.Delay(TimeSpan.FromSeconds(1));
                       }
                   }
               }
            );
            #endregion
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

        public async Task PublishOnValvulaTopic(string payload)
        {
            //var result = mqttClient.PublishAsync(new MqttApplicationMessageBuilder().wi)
        }
    }
}
