using Microsoft.EntityFrameworkCore;
using NotificationHub.Application.Interfaces;
using NotificationHub.Api.Data;
using NotificationHub.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add OpenAPI/Swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Add DbContext with PostgreSQL or In-Memory database
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationLogRepository, NotificationLogRepository>();

// Add CORS for local development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
await dbContext.Database.MigrateAsync();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
