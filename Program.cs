using MqttApiPg;
using MqttApiPg.Models;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MQTTnet.AspNetCore.Routing;
using System.Text.Json;
using MqttApiPg.MqttControllers;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithMachineName()
    .WriteTo.Console()

    .CreateLogger();

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(
    o =>
    {
        o.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("PORT")!)); // Default HTTP pipeline
        o.ListenAnyIP(1883, l => l.UseMqtt()); // Mqtt
    });

builder.Host
    .UseWindowsService()
    .UseSystemd()
    .UseSerilog();

builder.Services
    .Configure<AquaDatabaseSettings>(builder.Configuration.GetSection("AquaDatabase"))
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(x => x.EnableAnnotations());

builder.Services
    .AddSingleton<MongoDbContext>()
    .AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

//builder.Services.AddSingleton<MqttServer>();
builder.Services.AddSingleton<MqttService>();
builder.Services.AddMqttControllers();
builder.Services.AddMqttDefaultJsonOptions(new JsonSerializerOptions(JsonSerializerDefaults.Web));

builder.Services
    .AddHostedMqttServerWithServices(optionsBuilder =>
    {
        optionsBuilder.WithoutDefaultEndpoint();
        optionsBuilder.WithDefaultEndpointPort(1883);
        optionsBuilder.WithPersistentSessions();
    })
    .AddMqttConnectionHandler()
    .AddConnections();

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();

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
    server.WithAttributeRouting(app.Services, allowUnmatchedRoutes: true);
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
    //app.UseHsts();
    //app.UseHttpsRedirection();
}

app.Run();