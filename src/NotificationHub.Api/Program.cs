using Microsoft.EntityFrameworkCore;
using NotificationHub.Api.Data;
using NotificationHub.Api.Interfaces;
using NotificationHub.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add OpenAPI/Swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Add DbContext with In-Memory database
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseInMemoryDatabase("NotificationHubDb"));

// Register repository
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

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
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
