namespace MqttApiPg
{
    public class Startup
    {
        private readonly AssemblyName serviceName = Assembly.GetExecutingAssembly().GetName();
        private readonly MqttServiceOptions mqttServiceConfiguration = new();
        private readonly string[] summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
        {
            public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        }

        public Startup(IConfiguration configuration)
        {
            configuration.GetSection(this.serviceName.Name).Bind(this.mqttServiceConfiguration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSingleton(this.mqttServiceConfiguration);
            services.AddSingleton(_ => new MqttService(this.mqttServiceConfiguration, this.serviceName.Name ?? "MqttService"));
            services.AddSingleton(_ => new MqttClientService(this.mqttServiceConfiguration, this.serviceName.Name ?? "MqttClientService"));
            services.AddSingleton<IHostedService>(p => p.GetRequiredService<MqttService>());
            services.AddSingleton<IHostedService>(p => p.GetRequiredService<MqttClientService>());

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("./swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = string.Empty;
                });
            }
            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging();
            app.UseRouting();

            

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapGet("/weatherforecast", () =>
                {
                    var forecast = Enumerable.Range(1, 5).Select(index =>
                        new WeatherForecast
                        (
                            DateTime.Now.AddDays(index),
                            Random.Shared.Next(-20, 55),
                            summaries[Random.Shared.Next(summaries.Length)]
                        ))
                        .ToArray();
                    return forecast;
                }).WithName("GetWeatherForecast");
            });

            _ = app.ApplicationServices.GetService<MqttService>();
            _ = app.ApplicationServices.GetService<MqttClientService>();
        }
    }
}
