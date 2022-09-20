namespace MqttApiPg
{
    public class MqttService : BackgroundService
    {
        private readonly ILogger logger;
        private readonly string serviceName;
        private static double BytesDivider => 1048576.0;
        public MqttServiceConfiguration MqttServiceConfiguration { get; set; }

        public MqttService(string serviceName, MqttServiceConfiguration mqttServiceConfiguration)
        {
            MqttServiceConfiguration = mqttServiceConfiguration;
            this.logger = Log.ForContext("Type", nameof(MqttService));
            this.serviceName = serviceName;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!this.MqttServiceConfiguration.isValid())
            {
                throw new Exception("Invalid configuration");
            }

            this.logger.Information("Starting service");
            this.StartMqttServer();
            this.logger.Information("Service started");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.LogMemoryInformation();
                await Task.Delay(this.MqttServiceConfiguration.DelayInMilliSeconds, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.Error($"An error ocurred: {ex}");
            }
        }

        private Task ValidateConnectionAsync(ValidatingConnectionEventArgs args)
        {
            try
            {
                var currentUser = this.MqttServiceConfiguration.Users.FirstOrDefault(u => u.UserName == args.UserName);

                if (
                    currentUser is null |
                    args.UserName == currentUser?.UserName |
                    args.Password != currentUser?.Password
                    )
                {
                    args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    this.LogMessage(args, true);
                    return Task.CompletedTask;
                }

                args.ReasonCode = MqttConnectReasonCode.Success;
                this.LogMessage(args, true);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                this.logger.Error($"An error ocurred: {ex}");
                return Task.FromException(ex);
            }
        }

        private Task InterceptSubscriptionAsync(InterceptingSubscriptionEventArgs args)
        {
            try
            {
                args.ProcessSubscription = true;
                this.LogMessage(args, true);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                this.logger.Error($"An error occurred: {ex}");
                return Task.FromException(ex);
            }
        }

        private Task InterceptApplicationMessagePublishAsync(InterceptingPublishEventArgs args)
        {
            try
            {
                args.ProcessPublish = true;
                this.LogMessage(args);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occurred: {Exception}.", ex);
                return Task.FromException(ex);
            }
        }

        private void StartMqttServer()
        {
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(this.MqttServiceConfiguration.Port)
                .WithEncryptedEndpointPort(this.MqttServiceConfiguration.TlsPort);

            var mqttServer = new MqttFactory().CreateMqttServer(optionsBuilder.Build());
            mqttServer.ValidatingConnectionAsync += this.ValidateConnectionAsync;
            mqttServer.InterceptingSubscriptionAsync += this.InterceptSubscriptionAsync;
            mqttServer.InterceptingPublishAsync += this.InterceptApplicationMessagePublishAsync;
            mqttServer.StartAsync();
        }

        private void LogMemoryInformation()
        {
            var totalMemory = GC.GetTotalMemory(false);
            var memoryInfo = GC.GetGCMemoryInfo();
            var divider = BytesDivider;

            Log.Information(
                "Heartbeat for service {ServiceName}: Total {Total}, heap size: {HeapSize}, memory load: {MemoryLoad}.",
                this.serviceName,
                $"{(totalMemory / divider):N3}",
                $"{(memoryInfo.HeapSizeBytes / divider):N3}",
                $"{(memoryInfo.MemoryLoadBytes / divider):N3}");
        }

        private void LogMessage(InterceptingSubscriptionEventArgs args, bool successfull)
        {
            this.logger.Information(
                successfull
                    ? "New subscription: ClientId = {ClientId}, TopicFilter = {TopicFilter}"
                    : "Subscription failed for clientId = {ClientId}, TopicFilter = {TopicFilter}",
                args.ClientId,
                args.TopicFilter);
        }     

        private void LogMessage(InterceptingPublishEventArgs args)
        {
            var payload = args.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

            this.logger.Information(
                "Message: CLientId = {ClientId}, Topic = {Topic}, Payload = {Payload}, QoS = {QoS}, Retain-Flag = {RetainFlag}",
                args.ClientId,
                args.ApplicationMessage?.Topic,
                payload,
                args.ApplicationMessage?.QualityOfServiceLevel,
                args.ApplicationMessage?.Retain);
        }

        private void LogMessage(ValidatingConnectionEventArgs args, bool showPassword)
        {
            if (showPassword)
            {
                this.logger.Information(
                    "New connection: ClientId = {ClientId}, Endpoint = {Endpoint}, Username = {UserName}, Password = {Password}, CleanSession = {CleanSession}",
                    args.ClientId,
                    args.Endpoint,
                    args.UserName,
                    args.Password,
                    args.CleanSession);
            }
            else
            {
                this.logger.Information(
                    "New connection: ClientId = {ClientId}, Endpoint = {Endpoint}, Username = {UserName}, CleanSession = {CleanSession}",
                    args.ClientId,
                    args.Endpoint,
                    args.UserName,
                    args.CleanSession);
            }
        }
    }
}
