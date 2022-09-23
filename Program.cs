var builder = WebApplication.CreateBuilder(args);
builder.Host
    .UseSerilog()
    .UseWindowsService()
    .UseSystemd();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddControllers();

var app = builder.Build();

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

//Program: https://github.com/Brendonsk/DotnetHerokuDeployTest/blob/mqtt_integration/Startup.cs
//Startup: https://github.com/Brendonsk/DotnetHerokuDeployTest/blob/mqtt_integration/Startup.cs
//Modelo a seguir: https://github.com/Brendonsk/DotnetHerokuDeployTest/commit/7767685dc243b793ef6aac1273164dafffdd5ef5#diff-0b69b473fe937040615d69f606751f61ddbc2e3a1849360ff2456c22afe88c0b