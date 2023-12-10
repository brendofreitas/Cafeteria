using Cafeteria.Data;
using Cafeteria.Modelos;
using Cafeteria.Repositorio;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);


//builder.Services.AddOpenTelemetryTracing(b =>
//{
//    b.SetResourceBuilder(
//        ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName))
//     .AddAspNetCoreInstrumentation()
//     .AddHttpClientInstrumentation()
//     .AddOtlpExporter(opts => { opts.Endpoint = new Uri("http://localhost:4317"); });
//});


var tracingOtlpEndpoint = new Uri("http://localhost:4317");
var otel = builder.Services.AddOpenTelemetry();

otel.ConfigureResource(resource => resource.AddService(serviceName: builder.Environment.ApplicationName));

otel.WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation()
.AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    .AddPrometheusExporter());


otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddNpgsql();
    if (tracingOtlpEndpoint != null)
    {
        tracing.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint =tracingOtlpEndpoint;
        });
    }
    else
    {
        tracing.AddConsoleExporter();
    }
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.PostgreSQL(builder.Configuration.GetConnectionString("DefaultConnection"), "Logs", needAutoCreateTable: true)
    .MinimumLevel.Error()
    .MinimumLevel.Warning()
    .CreateLogger();

//Log.Logger = new LoggerConfiguration()
//    .WriteTo.GrafanaLoki("http://localhost:3100")
//    .MinimumLevel.Information()
//    .CreateLogger();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSerilog();
});


builder.Services.AddScoped<IRepositorio<Cafe>, CafeRepositorio>();

builder.Services.AddDbContext<ContextoBanco>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();



using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ContextoBanco>();
    dbContext.Database.Migrate();
}


app.MapPrometheusScrapingEndpoint();
app.MapControllers();

app.Run();
