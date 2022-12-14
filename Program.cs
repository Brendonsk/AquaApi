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
using MqttApiPg.Services;
using Quartz;

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

    .AddSingleton<DiariaService>()
    .AddSingleton<MensalService>()
    .AddSingleton<RegistroService>()
    .AddSingleton<ValvulaService>()

    .AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

//builder.Services.AddQuartz(config =>
//{
//    config.UseMicrosoftDependencyInjectionJobFactory();
//    var jobKey = new JobKey("MensalJob");
//    config.AddJob<MensalJob>(opts => opts.WithIdentity(jobKey));

//    config.AddTrigger(opts => opts
//        .ForJob(jobKey)
//        .WithIdentity("MensalJob-trigger")
//        .WithCronSchedule("0 0/2 * * * ?")
//        .StartNow());
//});
//builder.Services.AddTransient<MensalJob>();

builder.Services.AddMqttClientHostedService();

builder.Services
    .AddConnections();

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("./swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.Run();