using BPMMessaging;
using MongoDB.Driver;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
	var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
	return new MongoClient(settings);
});
builder.Services.AddSingleton(sp => // создается ли эта база данных автоматически?
	sp.GetRequiredService<IMongoClient>().GetDatabase("my_database"));

builder.Services.AddSingleton<IConnectionFactory, ConnectionFactory>(sp =>
	new ConnectionFactory { HostName = "localhost" });

builder.Services.AddSingleton<QueueConfigRepository>();
builder.Services.AddSingleton<RabbitMqListenerManager>();
builder.Services.AddSingleton<QueueMonitorService>();

var app = builder.Build();
app.Services.GetRequiredService<QueueMonitorService>();

app.Run();
