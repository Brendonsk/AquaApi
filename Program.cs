using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MqttApiPg;
using MqttApiPg.Controlllers;
using MQTTnet.AspNetCore;
using MQTTnet.AspNetCore.Routing;
using MQTTnet.Server;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithMachineName()
    .WriteTo.Console()

    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Orleans", LogEventLevel.Information)
    .MinimumLevel.Information()

    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(
    o =>
    {
        o.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("PORT")!)); // Default HTTP pipeline
    });

builder.Host
    .UseSerilog()
    .UseWindowsService()
    .UseSystemd();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

builder.Services
    .AddSingleton<MqttService>()
    .AddHostedMqttServerWithServices(optionsBuilder =>
    {
        optionsBuilder.WithDefaultEndpoint();
    })
    .AddMqttConnectionHandler()
    .AddConnections()

    .AddSingleton<MqttController>()
    .AddControllers();

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapConnectionHandler<MqttConnectionHandler>(
        "/mqtt",
        httpConnectionDispatcherOptions => httpConnectionDispatcherOptions.WebSockets.SubProtocolSelector =
            protocolList => protocolList.FirstOrDefault() ?? string.Empty);
});

app.UseMqttServer(server => 
{
    app.Services
        .GetRequiredService<MqttService>()
            .ConfigureServer(server);    
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("./swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
    app.UseHttpsRedirection();
}

app.Run();