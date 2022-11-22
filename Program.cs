using MqttApiPg;
using MqttApiPg.Models;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using System.Text.Json;
using MqttApiPg.Settings;
using System.Configuration;
using MqttApiPg.Extensions;
using Serilog;
using Serilog.Exceptions;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithMachineName()
    .WriteTo.Console()

    .CreateLogger();

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
var builder = WebApplication.CreateBuilder(args);

BrokerHostSettings brokerHostSettings = new BrokerHostSettings();
builder.Configuration.GetSection(nameof(BrokerHostSettings)).Bind(brokerHostSettings);
AppSettingsProvider.BrokerHostSettings = brokerHostSettings;

ClientSettings clientSettings = new ClientSettings();
builder.Configuration.GetSection(nameof(ClientSettings)).Bind(clientSettings);
AppSettingsProvider.ClientSettings = clientSettings;

builder.Services
    .Configure<AquaDatabaseSettings>(builder.Configuration.GetSection("AquaDatabase"))
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(x => x.EnableAnnotations());

builder.Services
    .AddSingleton<MongoDbContext>()
    .AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddMqttClientHostedService();

builder.Services
    .AddConnections();

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    //endpoints.MapConnectionHandler<MqttConnectionHandler>(
    //    "/mqtt",
    //    httpConnectionDispatcherOptions => httpConnectionDispatcherOptions.WebSockets.SubProtocolSelector =
    //        protocolList => protocolList.FirstOrDefault() ?? string.Empty);
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