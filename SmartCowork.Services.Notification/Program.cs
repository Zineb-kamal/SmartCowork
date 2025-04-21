using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SmartCowork.Services.Notification.Infrastructure.HealthChecks;
using SmartCowork.Services.Notification.Infrastructure.MongoDB;
using SmartCowork.Services.Notification.Repository;


var builder = WebApplication.CreateBuilder(args);

// Détection de l'environnement
var env = builder.Environment;

// Configuration avec l'environnement
builder.Configuration
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Ajout de MongoDB avec la configuration appropriée
builder.Services.AddMongoDb(builder.Configuration, env);
// Add repositories
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Add controllers
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddMongoDbHealthCheck(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            Status = report.Status.ToString(),
            Checks = report.Entries.Select(e => new
            {
                Component = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description
            }),
            Duration = report.TotalDuration
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
