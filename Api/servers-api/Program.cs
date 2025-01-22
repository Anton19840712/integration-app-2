using RabbitMQ.Client;
using Serilog;
using servers_api.endpoints;
using servers_api.Handlers;
using servers_api.Patterns;
using servers_api.Services.Brokers;
using servers_api.Services.Connectors;
using servers_api.Services.InternalSystems;
using servers_api.Services.Parsers;
using tcp_client;

Console.Title = "client api";

var builder = WebApplication.CreateBuilder(args);

// Чтение порта для запуска client
string port = builder.Configuration["Port"]
			   ?? args.FirstOrDefault(arg => arg.StartsWith("--port="))?.Split('=')[1];

if (string.IsNullOrEmpty(port))
{
	port = "5001"; // Порт по умолчанию
}

// Настройка адреса запуска
string url = $"http://localhost:{port}";
builder.WebHost.UseUrls(url);

// Логирование адреса запуска
Log.Information("Приложение client запускается на {Url}", url);

builder.Host.UseSerilog((ctx, cfg) => cfg
                   .ReadFrom.Configuration(ctx.Configuration)
                   .WriteTo.Console()
                   .WriteTo.Seq("http://localhost:5341")
               );

// Common infrastructure:
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors();

builder.Services.AddHostedService<ResponseListenerService>();

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
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.MapCommonApiEndpoints();
app.MapTcpApiEndpoints();

app.Run();
