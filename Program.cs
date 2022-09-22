namespace MqttApiPg;

public class Program
{
    private static IConfigurationRoot? config;

    public static string EnvironmentName => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

    public static MqttServiceConfiguration Configuration { get; set; } = new();

    public static AssemblyName ServiceName => Assembly.GetExecutingAssembly().GetName();

    public static async Task<int> Main(string[] args)
    {
        ReadConfiguration();
        SetupLogging();

        try
        {
            Log.Information("Starting {ServiceName}, Version {Version}...", ServiceName.Name, ServiceName.Version);
            var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            await CreateHostBuilder(args, currentLocation!).Build().RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly.");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }

        return 0;
    }

    private static IHostBuilder CreateHostBuilder(string[] args, string currentLocation) =>
        Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(
            webBuilder =>
            {
                webBuilder.UseContentRoot(currentLocation);
                webBuilder.UseStartup<Startup>();
            })
            .UseSerilog()
            .UseWindowsService()
            .UseSystemd();

    private static void ReadConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json", false, true);

        if (!string.IsNullOrWhiteSpace(EnvironmentName))
        {
            var appsettingsFileName = $"appsettings.{EnvironmentName}.json";

            if (File.Exists(appsettingsFileName))
            {
                configurationBuilder.AddJsonFile(appsettingsFileName, false, true);
            }
        }

        config = configurationBuilder.Build();
        config.Bind(ServiceName.Name, Configuration);
    }

    private static void SetupLogging()
    {
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .WriteTo.Console();

        if (EnvironmentName != "Development")
        {
            loggerConfiguration
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Orleans", LogEventLevel.Information)
                .MinimumLevel.Information();
        }

        Log.Logger = loggerConfiguration.CreateLogger();
    }
}