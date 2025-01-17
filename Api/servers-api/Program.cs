using RabbitMQ.Client;
using Serilog;
using servers_api.background;
using servers_api.endpoints;
using servers_api.Handlers;
using servers_api.http_client_factory;
using servers_api.Patterns;
using servers_api.ping;
using servers_api.Services.Brokers;
using servers_api.Services.Connectors;
using servers_api.Services.InternalSystems;
using servers_api.Services.Parsers;
using servers_api.start;

Console.Title = "client api";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
                   .ReadFrom.Configuration(ctx.Configuration)
                   .WriteTo.Console()
                   .WriteTo.Seq("http://localhost:5341")
               );

// Common infrastructure:
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors();

//ping server with client:
builder.Services.AddTransient<ITcpPingClientService, TcpPingClientService>();

//
builder.Services.AddSingleton<IHttpClientFactoryService, HttpClientFactoryService>();

builder.Services.AddTransient<ITCPServerRunner, TCPServerRunner>();


builder.Services.AddTransient<IJsonParsingService, JsonParsingService>();
builder.Services.AddTransient<IRabbitMqQueueManager, RabbitMqQueueManager>();
builder.Services.AddTransient<ITeachService, TeachService>();

builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddTransient<IUploadHandler, UploadHandler>();
builder.Services.AddScoped<ISenderService, SenderService>();

builder.Services.AddSingleton<IConnectionFactory>(provider =>
{
    return new ConnectionFactory
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "guest",
        Password = "guest"
    };
});

builder.Services.AddSingleton<IRabbitMqQueueListener, RabbitMqQueueListener>();
//builder.Services.AddHostedService<TcpPingBackgroundService>();

var app = builder.Build();

var urls = builder.WebHost.GetSetting("urls");
Log.Information($"Server is running on: {urls}");

app.UseSerilogRequestLogging();

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.MapApiEndpoints();

app.Run();
