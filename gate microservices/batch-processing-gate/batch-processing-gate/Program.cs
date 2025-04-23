using Microsoft.Extensions.Configuration;
using sftp_dynamic_gate_app;
using sftp_dynamic_gate_app.middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var configuration = builder.Configuration;

var services = builder.Services;


services.AddRabbitMqServices(configuration);

var app = builder.Build();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();





