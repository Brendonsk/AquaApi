using MqttApiPg;
using MqttApiPg.Controlllers;
using MqttApiPg.Models;
using MQTTnet.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithMachineName()
    .WriteTo.Console()

    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Orleans", LogEventLevel.Information)
    .MinimumLevel.Information()

    .CreateBootstrapLogger();

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.ConfigureKestrel(
//    o =>
//    {
//        o.ListenAnyIP(1883, l => l.UseMqtt());
//        o.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("PORT")!)); // Default HTTP pipeline
//    });

builder.Host
    .UseSerilog()
    .UseWindowsService()
    .UseSystemd();

builder.Services
    .Configure<AquaDatabaseSettings>(builder.Configuration.GetSection("AquaDatabase"))
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(x => x.EnableAnnotations());

builder.Services
    .AddSingleton<MongoDbContext>()
    .AddSingleton<MqttService>()
    .AddHostedMqttServerWithServices(optionsBuilder =>
    {
        optionsBuilder.WithoutDefaultEndpoint();
    })
    .AddMqttConnectionHandler()
    .AddConnections()

    .AddSingleton<MqttController>()
    .AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null); ;

builder.Services.Configure<AquaDatabaseSettings>(
    builder.Configuration.GetSection("AquaDatabase")    
);

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapConnectionHandler<MqttConnectionHandler>(
        "/mqtt",
        httpConnectionDispatcherOptions => httpConnectionDispatcherOptions.WebSockets.SubProtocolSelector =
            protocolList => protocolList.FirstOrDefault() ?? string.Empty);
    endpoints.MapControllers();
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
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Run();