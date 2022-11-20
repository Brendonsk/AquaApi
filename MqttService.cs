using Microsoft.Extensions.Options;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Server;
using MQTTnet.Exceptions;
using System.Xml.Schema;
using MQTTnet.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting.Server;

namespace MqttApiPg
{
    public class MqttService
    {
        public readonly ILogger<MqttService> _logger;
        public static double BytesDivider => 1048576.0;

        public MqttServer mqttServer;

        private readonly string ClientId = "Heroku App";

        public MqttService(ILogger<MqttService> logger)
        {
            _logger = logger;
        }

        public Task StartedAsync(EventArgs args)
        {
            if (this.mqttServer!.IsStarted)
            {
                this._logger.LogInformation("Service started");
                LogMemoryInformation();

                //this.mqttServer.SubscribeAsync(this.ClientId, "valvula/abrir");
                //this.mqttServer.SubscribeAsync(this.ClientId, "valvulaLog");

                return Task.CompletedTask;
            }

            this._logger.LogError("Service failed to start");
            return Task.FromException(
                new MqttConfigurationException(
                    new Exception("Server could not be started")
                )
            );
            
        }

        //public void ConfigureServerOptions(AspNetMqttServerOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder
        //        .WithoutDefaultEndpoint()
        //        .WithDefaultEndpointPort(1883);
        //}

        public void ConfigureServer(MqttServer server)
        {
            this.mqttServer = server;

            server.InterceptingSubscriptionAsync += InterceptSubscriptionAsync;
            server.InterceptingPublishAsync += InterceptApplicationMessagePublishAsync;
            server.StartedAsync += StartedAsync;
        }

        //public Task ValidateConnectionAsync(ValidatingConnectionEventArgs args)
        //{
        //    try
        //    {
        //        var currentUser = this._config.Value.Users.FirstOrDefault(u => u.UserName == args.UserName);

        //        if (
        //            currentUser is null |
        //            args.UserName == currentUser?.UserName |
        //            args.Password != currentUser?.Password
        //            )
        //        {
        //            args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
        //            this.LogMessage(args, true);
        //            return Task.CompletedTask;
        //        }

        //        args.ReasonCode = MqttConnectReasonCode.Success;
        //        this.LogMessage(args, true);
        //        return Task.CompletedTask;
        //    }
        //    catch (Exception ex)
        //    {
        //        this._logger.LogError("An error ocurred: {exception}", ex);
        //        return Task.FromException(ex);
        //    }
        //}

        public Task InterceptSubscriptionAsync(InterceptingSubscriptionEventArgs args)
        {
            try
            {
                args.ProcessSubscription = true;
                this.LogMessage(args, true);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                this._logger.LogError("An error occurred: {exception}", ex);
                return Task.FromException(ex);
            }
        }

        public async Task InterceptApplicationMessagePublishAsync(InterceptingPublishEventArgs args)
        {
            if (args.ClientId.Equals(this.ClientId))
            {
                return;
            }

            try
            {
                args.ProcessPublish = true;
                this.LogMessage(args);

                var resposta =
                    new InjectedMqttApplicationMessage(new MqttApplicationMessageBuilder()
                        .WithTopic("valvulaLog")
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                        .WithPayload($"{args.ClientId}: {Encoding.UTF8.GetString(args.ApplicationMessage.Payload)}")
                        .Build());

                resposta.SenderClientId= this.ClientId;
                await this.mqttServer.InjectApplicationMessage(resposta);
            }
            catch (Exception ex)
            {
                this._logger.LogError("An error occurred: {Exception}.", ex);
            }
        }

        public void LogMemoryInformation()
        {
            var totalMemory = GC.GetTotalMemory(false);
            var memoryInfo = GC.GetGCMemoryInfo();
            var divider = BytesDivider;

            this._logger.LogInformation(
                "Heartbeat for service {ServiceName}: Total {Total}, heap size: {HeapSize}, memory load: {MemoryLoad}.",
                "MqttApiPg",
                $"{(totalMemory / divider):N3}",
                $"{(memoryInfo.HeapSizeBytes / divider):N3}",
                $"{(memoryInfo.MemoryLoadBytes / divider):N3}");
        }

        public void LogMessage(InterceptingSubscriptionEventArgs args, bool successfull)
        {
            this._logger.LogInformation(
                successfull
                    ? "New subscription: ClientId = {ClientId}, TopicFilter = {TopicFilter}"
                    : "Subscription failed for clientId = {ClientId}, TopicFilter = {TopicFilter}",
                args.ClientId,
                args.TopicFilter);
        }     

        public void LogMessage(InterceptingPublishEventArgs args)
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

        //public void LogMessage(ValidatingConnectionEventArgs args, bool showPassword)
        //{
        //    if (showPassword)
        //    {
        //        this._logger.LogInformation(
        //            "New connection: ClientId = {ClientId}, Endpoint = {Endpoint}, Username = {UserName}, Password = {Password}, CleanSession = {CleanSession}",
        //            args.ClientId,
        //            args.Endpoint,
        //            args.UserName,
        //            args.Password,
        //            args.CleanSession);
        //    }
        //    else
        //    {
        //        this._logger.LogInformation(
        //            "New connection: ClientId = {ClientId}, Endpoint = {Endpoint}, Username = {UserName}, CleanSession = {CleanSession}",
        //            args.ClientId,
        //            args.Endpoint,
        //            args.UserName,
        //            args.CleanSession);
        //    }
        //}
    }
}
