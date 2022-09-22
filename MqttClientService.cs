using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using System.Threading;

namespace MqttApiPg
{
    public class MqttClientService: BackgroundService
    {
        public readonly ILogger logger;
        private readonly string serviceName;
        private static double BytesDivider => 1048576.0;
        public ManagedMqttClient mqttClient;

        public MqttServiceConfiguration MqttServiceConfiguration { get; set; }

        public MqttClientService(MqttServiceConfiguration mqttServiceConfiguration, string serviceName)
        {
            MqttServiceConfiguration = mqttServiceConfiguration;
            this.logger = Log.ForContext("Type", nameof(MqttClientService));
            this.serviceName = serviceName;
            this.mqttClient = StartMqttManagedClient();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.Information("Starting service");
            this.StartMqttManagedClient();
            this.logger.Information("Service started");
            await base.StartAsync(cancellationToken);
        }

        private ManagedMqttClient StartMqttManagedClient()
        {
            var optionsBuilder = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(
                    new MqttClientOptionsBuilder()
                        .WithTcpServer("broker.emqx.io")
                        .WithClientId("EMQX_" + Guid.NewGuid().ToString())
                        .WithCleanSession()
                        .Build()
                );

            var client = (ManagedMqttClient)new MqttFactory().CreateManagedMqttClient();

            client.ApplicationMessageReceivedAsync += args => {
                var payload = args.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
                this.logger.Information("Received: Topic={Topic}; Message={Message}",
                    args.ApplicationMessage?.Topic,
                    payload);
                return Task.CompletedTask;
            };

            client.StartAsync(optionsBuilder.Build());

            client.SubscribeAsync(new List<MqttTopicFilter>  { new MqttTopicFilterBuilder().WithTopic("brendon").Build() });
            return client;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                this.LogMemoryInformation();
                await Task.Delay(this.MqttServiceConfiguration.DelayInMilliSeconds, stoppingToken);
            }
            catch (Exception ex)
            {
                this.logger.Error($"An error ocurred: {ex}");
            }
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
    }
}
