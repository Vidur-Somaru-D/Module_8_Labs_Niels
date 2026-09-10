using Derivco.PlayerApi.Options;
using Derivco.PlayerApi.Repositories;
using Derivco.PlayerApi.Services;
// CHANGE: Serilog namespaces are required for host integration and newline-delimited JSON console output.
using Serilog;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

// CHANGE: Replace the default logging provider with Serilog, read levels from configuration,
// and write structured JSON exclusively to stdout so container platforms can collect the logs.
builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPlayerRepository, SqlPlayerRepository>();
builder.Services.AddScoped<IPlayerService, PlayerService>();

// CHANGE: Bind Database__ConnectionString through ASP.NET Core configuration and fail at startup
// with a clear message when the required environment variable has not been supplied.
builder.Services.AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ConnectionString),
        "Database__ConnectionString must be set before the application starts.")
    .ValidateOnStart();

var app = builder.Build();

// CHANGE: Emit one structured completion event for every HTTP request handled by the pipeline.
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();