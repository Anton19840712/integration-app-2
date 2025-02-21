using BPMEngine.DB.Consts;
using BPMMessaging.mapping;
using BPMMessaging.models;
using BPMMessaging.parsing;
using BPMMessaging.publishing;
using BPMMessaging.repository;
using BPMSystem.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using RabbitMQ.Client;
using Serilog;

namespace BPMSystem;

public class Program
{
    
    public static void Main(string[] args)
    {
        Console.Title = "server bpm";
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((ctx, cfg) => cfg
                   .ReadFrom.Configuration(ctx.Configuration)
                   .WriteTo.Console());

		// Извлечение строк подключения
		DBConsts.DBConnections.ConnectionString_BPM = builder.Configuration.GetConnectionString("BPM_db");
        DBConsts.DBConnections.ConnectionString_BLL = builder.Configuration.GetConnectionString("BLL_db");

        // Добавление сервисов
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "BPM", Version = "V1" });
        });

        // Настройка DbContext для работы с PostgreSQL через Npgsql
        builder.Services.AddDbContextPool<BPMContext>(opt =>
        {
            if (!string.IsNullOrEmpty(DBConsts.DBConnections.ConnectionString_BPM))
            {
                opt.UseNpgsql(DBConsts.DBConnections.ConnectionString_BPM,
                    sqlopt =>
                    {
                        sqlopt.EnableRetryOnFailure();
                        sqlopt.CommandTimeout(60);
                    });
                opt.EnableSensitiveDataLogging();
                opt.EnableDetailedErrors();
            }
        });

		// Добавление остальных сервисов
		builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMqSettings"));
		builder.Services.AddSingleton<IMessagePublisher, MessagePublisher>();
        builder.Services.AddScoped<IJsonParsingService, JsonParsingService>();

        builder.Services.AddHostedService<OutboxIntegrationTrackingService>();
		builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
		builder.Services.AddSingleton<IMongoClient>(sp =>
		{
			var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
			return new MongoClient(settings.ConnectionString);
		});

		builder.Services.AddSingleton(typeof(IMongoRepository<>), typeof(MongoRepository<>));


		builder.Services.AddSingleton<IConnectionFactory, ConnectionFactory>(sp =>
			new ConnectionFactory
			{
				HostName = builder.Configuration["RabbitMqSettings:HostName"],
				Port = int.Parse(builder.Configuration["RabbitMqSettings:Port"] ?? "5672"),
				UserName = builder.Configuration["RabbitMqSettings:UserName"],
				Password = builder.Configuration["RabbitMqSettings:Password"]
			});

		builder.Services.AddAutoMapper(typeof(MappingProfile));

		builder.Services.AddSingleton<QueueConfigRepository>();
		builder.Services.AddSingleton<RabbitMqListenerManager>();
		builder.Services.AddSingleton<QueueMonitorService>();

		var app = builder.Build();

		// Логируем запуск приложения
		Log.Information("Приложение запущено");

		app.Services.GetRequiredService<QueueMonitorService>();



        // Настройка middleware для Swagger и других функций
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();  // Включение страницы ошибок в разработке
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseCors(x => x.SetIsOriginAllowed(origin => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        app.UseRouting();
        app.MapControllers(); // Привязка контроллеров

        // Запуск приложения


        app.Run();
    }
}
