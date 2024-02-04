//using Cafeteria.Data;
//using Cafeteria.Modelos;
//using Cafeteria.Repositorio;
//using Microsoft.EntityFrameworkCore;
//using Serilog;
//using Serilog.Sinks.Grafana.Loki;
//using OpenTelemetry.Resources;
//using OpenTelemetry.Trace;
//using OpenTelemetry.Metrics;
//using Serilog.Sinks.OpenTelemetry;
//using Npgsql;
//using System.Diagnostics.Metrics;

//var builder = WebApplication.CreateBuilder(args);

//const string outputTemplate =
//    "[{Level:w}]: {Timestamp:dd-MM-yyyy:HH:mm:ss} {MachineName} {EnvironmentName} {SourceContext} {Message}{NewLine}{Exception}";


//builder.Services.AddScoped<IRepositorio<Cafe>, CafeRepositorio>();
//builder.Services.AddSingleton<Instrumentor>();

//var tracingOtlpEndpoint = new Uri("http://localhost:4317");
//var otel = builder.Services.AddOpenTelemetry();

//otel.ConfigureResource(resource => resource.AddService(Instrumentor.ServiceName));

//otel.WithTracing(metrics => metrics
//    .AddSource(Instrumentor.ServiceName)
//    .AddAspNetCoreInstrumentation(opt =>
//    {
//        opt.Filter = context =>
//        {
//            var ignore = new[] { "/swagger" };
//            return !ignore.Any(s => context.Request.Path.ToString().Contains(s));
//        };
//    })
//    .AddHttpClientInstrumentation(opts =>
//    {
//        opts.FilterHttpRequestMessage = req =>
//        {
//            var ignore = new[] { "/loki/api" };
//            return !ignore.Any(s => req.RequestUri!.ToString().Contains(s));
//        };
//    })
//    .AddOtlpExporter())
//    .WithMetrics(metrics => metrics
//    .AddMeter(Instrumentor.ServiceName)
//    .AddRuntimeInstrumentation()
//    .AddAspNetCoreInstrumentation()
//    .AddHttpClientInstrumentation()
//    .AddOtlpExporter());




////.AddMeter("Microsoft.AspNetCore.Hosting")
////    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
////    .AddPrometheusExporter());


//otel.WithTracing(tracing =>
//{
//    tracing.AddAspNetCoreInstrumentation();
//    tracing.AddHttpClientInstrumentation();
//    tracing.AddNpgsql();
//    if (tracingOtlpEndpoint != null)
//    {
//        tracing.AddOtlpExporter(otlpOptions =>
//        {
//            otlpOptions.Endpoint = tracingOtlpEndpoint;
//        });
//    }
//    else
//    {
//        tracing.AddConsoleExporter();
//    }
//});

//Log.Logger = new LoggerConfiguration()
//    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
//    .WriteTo.PostgreSQL(builder.Configuration.GetConnectionString("DefaultConnection"), "Logs", needAutoCreateTable: true)
//    .MinimumLevel.Error()
//    .MinimumLevel.Warning()
//    .MinimumLevel.Information()
//    .Enrich.FromLogContext()
//    .Enrich.WithThreadId()
//    .Enrich.WithEnvironmentName()
//    .Enrich.WithMachineName()
//    .WriteTo.Console()
// .WriteTo.OpenTelemetry(opts =>
// {
//     opts.IncludedData = IncludedData.SpecRequiredResourceAttributes;
//     opts.ResourceAttributes = new Dictionary<string, object>
//     {
//         ["app"] = "webapi",
//         ["runtime"] = "dotnet",
//         ["service.name"] = Instrumentor.ServiceName
//     };
// })
//    .CreateLogger();

//builder.Host.UseSerilog();

////Log.Logger = new LoggerConfiguration()
////    .WriteTo.GrafanaLoki("http://localhost:3100")
////    .MinimumLevel.Information()
////    .CreateLogger();

//builder.Services.AddLogging(loggingBuilder =>
//{
//    loggingBuilder.AddSerilog();
//});



////builder.WebHost.ConfigureKestrel(serverOptions =>
////{
////    serverOptions.ConfigureEndpointDefaults(listenOptions =>
////    {
////        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
////        listenOptions.UseConnectionLogging();
////        listenOptions.IPEndPoint.Port = 5243;
////    });
////});






//builder.Services.AddDbContext<ContextoBanco>(options =>
//{
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
//});

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();





//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();



////using (var scope = app.Services.CreateScope())
////{
////    var dbContext = scope.ServiceProvider.GetRequiredService<ContextoBanco>();
////    dbContext.Database.Migrate();
////}



////app.MapPrometheusScrapingEndpoint();
//app.MapControllers();

//app.Run();

//using BackendApiService;
using Cafeteria.Data;
using Cafeteria.Modelos;
using Cafeteria.Repositorio;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

const string outputTemplate =
    "[{Level:w}]: {Timestamp:dd-MM-yyyy:HH:mm:ss} {MachineName} {EnvironmentName} {SourceContext} {Message}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: outputTemplate)
    .WriteTo.OpenTelemetry(opts =>
    {
        opts.IncludedData = IncludedData.SpecRequiredResourceAttributes;
        opts.ResourceAttributes = new Dictionary<string, object>
        {
            ["app"] = "webapi",
            ["runtime"] = "dotnet",
            ["service.name"] = Instrumentor.ServiceName
        };
    })
    .CreateLogger();



builder.Services.AddDbContext<ContextoBanco>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});


builder.Services.AddScoped<IRepositorio<Cafe>, CafeRepositorio>();

builder.Host.UseSerilog();

builder.Services.AddSingleton<Instrumentor>();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(Instrumentor.ServiceName))
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource(Instrumentor.ServiceName)
            .AddAspNetCoreInstrumentation(opts =>
            {
                opts.Filter = context =>
                {
                    var ignore = new[] { "/swagger" };
                    return !ignore.Any(s => context.Request.Path.ToString().Contains(s));
                };
            })
            .AddHttpClientInstrumentation(opts =>
            {
                opts.FilterHttpRequestMessage = req =>
                {
                    var ignore = new[] { "/loki/api" };
                    return !ignore.Any(s => req.RequestUri!.ToString().Contains(s));
                };
            })
            .AddOtlpExporter())
    .WithMetrics(metricsProviderBuilder =>
        metricsProviderBuilder
            .AddMeter(Instrumentor.ServiceName)
            .AddRuntimeInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation().AddOtlpExporter());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.MapControllers();
app.Run();
