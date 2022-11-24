using MqttApiPg.Services;
using MqttApiPg.Settings;
using MQTTnet.Client;

namespace MqttApiPg.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMqttClientHostedService(this IServiceCollection services)
        {
            services.AddMqttClientServiceWithConfig(aspOptionBuilder =>
            {
                var clientSettinigs = AppSettingsProvider.ClientSettings;
                var brokerHostSettings = AppSettingsProvider.BrokerHostSettings;

                aspOptionBuilder
                    .WithCredentials(clientSettinigs.UserName, clientSettinigs.Password)
                    .WithClientId(clientSettinigs.Id)
                    .WithTcpServer(brokerHostSettings.Host, brokerHostSettings.Port);
            });
            return services;
        }

        private static IServiceCollection AddMqttClientServiceWithConfig(this IServiceCollection services, Action<MqttClientOptionsBuilder> configure)
        {
            services.AddSingleton<MqttClientOptions>(serviceProvider =>
            {
                var optionBuilder = new MqttClientOptionsBuilder();
                configure(optionBuilder);
                return optionBuilder.Build();
            });
            services.AddSingleton<MqttClientService>();
            services.AddSingleton<IHostedService>(serviceProvider =>
            {
                return serviceProvider.GetService<MqttClientService>()!;
            });
            services.AddSingleton<MqttClientServiceProvider>(serviceProvider =>
            {
                var mqttClientService = serviceProvider.GetService<MqttClientService>();
                var mqttClientServiceProvider = new MqttClientServiceProvider(mqttClientService!);
                return mqttClientServiceProvider;
            });
            return services;
        }
    }
}
