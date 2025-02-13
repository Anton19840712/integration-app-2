using BPMEngine.DB.Consts;
using BPMIntegration.Models;
using BPMIntegration.Publishing;
using BPMIntegration.Services.Background;
using BPMIntegration.Services.Parsing;
using BPMIntegration.Services.Save;
using BPMSystem.DB;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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

		// ���������� ����� �����������
		DBConsts.DBConnections.ConnectionString_BPM = builder.Configuration.GetConnectionString("BPM_db");
        DBConsts.DBConnections.ConnectionString_BLL = builder.Configuration.GetConnectionString("BLL_db");

        // ���������� ��������
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "BPM", Version = "V1" });
        });

        // ��������� DbContext ��� ������ � PostgreSQL ����� Npgsql
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

        // ����������� Marten
        builder.Services.AddScoped<IDocumentStore>(provider =>
        {
            return DocumentStore.For(options =>
            {
                options.Advanced.HiloSequenceDefaults.SequenceName = "Id";
				// options.DatabaseSchemaName = "public";
				// MartenSchemaConfiguration.Configure(options);// ��� ��������� ������ �� ��������
				options.Connection(DBConsts.DBConnections.ConnectionString_BPM!);

                options.Schema.For<IntegrationEntity>()
                        //.UseIdentityKey()
                        .DocumentAlias("integration");

				options.Schema.For<OutboxMessage>()
						//.UseIdentityKey()
						.DocumentAlias("outbox");
			});
        });

        // ���������� ��������� ��������
        builder.Services.AddScoped<ISaveService, SaveService>();
        builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();
        builder.Services.AddScoped<IJsonParsingService, JsonParsingService>();

        builder.Services.AddHostedService<OutboxProcessorService>();
        

        // �������� ����������
        var app = builder.Build();

        // ��������� middleware ��� Swagger � ������ �������
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();  // ��������� �������� ������ � ����������
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseCors(x => x.SetIsOriginAllowed(origin => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        app.UseRouting();
        app.MapControllers(); // �������� ������������

        // ������ ����������
        app.Run();
    }
}
