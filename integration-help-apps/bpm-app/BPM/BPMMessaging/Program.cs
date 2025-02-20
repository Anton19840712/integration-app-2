using BPMMessaging;
using MongoDB.Driver;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ����������� Serilog �� ������������
Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.CreateLogger();

// ���������� Serilog � ����������
builder.Host.UseSerilog();

builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
	var settings = MongoClientSettings.FromConnectionString(
		builder.Configuration["MongoDbSettings:ConnectionString"]);
	return new MongoClient(settings);
});
builder.Services.AddSingleton(sp =>
	sp.GetRequiredService<IMongoClient>().GetDatabase(
		builder.Configuration["MongoDbSettings:DatabaseName"]));

builder.Services.AddSingleton<IConnectionFactory, ConnectionFactory>(sp =>
	new ConnectionFactory
	{
		HostName = builder.Configuration["RabbitMqSettings:HostName"],
		Port = int.Parse(builder.Configuration["RabbitMqSettings:Port"] ?? "5672"),
		UserName = builder.Configuration["RabbitMqSettings:UserName"],
		Password = builder.Configuration["RabbitMqSettings:Password"]
	});

builder.Services.AddSingleton<QueueConfigRepository>();
builder.Services.AddSingleton<RabbitMqListenerManager>();
builder.Services.AddSingleton<QueueMonitorService>();

var app = builder.Build();

// �������� ������ ����������
Log.Information("���������� ��������");

app.Services.GetRequiredService<QueueMonitorService>();

app.Run();
