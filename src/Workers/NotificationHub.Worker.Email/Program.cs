using MassTransit;
using NotificationHub.Worker.Email.Consumers;
using NotificationHub.Worker.Email.Services;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

// Maileroo configuration
builder.Services.Configure<MailerooOptions>(
    builder.Configuration.GetSection("Maileroo"));

// Maileroo typed HttpClient (base address + API key header)
builder.Services.AddHttpClient<IMailerooEmailSender, MailerooEmailSender>(client =>
{
    client.BaseAddress = new Uri("https://smtp.maileroo.com/api/v2/");
    client.DefaultRequestHeaders.Add("X-API-Key",
        builder.Configuration["Maileroo:ApiKey"] ?? string.Empty);
});

// Named HttpClient for downloading attachment URLs
builder.Services.AddHttpClient("AttachmentDownloader");

// MassTransit / RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EmailNotificationConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbit = builder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rabbit["Host"], rabbit["VirtualHost"], h =>
        {
            h.Username(rabbit["Username"]!);
            h.Password(rabbit["Password"]!);
        });

        cfg.ReceiveEndpoint("email-notifications", e =>
        {
            e.ConfigureConsumer<EmailNotificationConsumer>(context);
            e.UseMessageRetry(r =>
                r.Exponential(3, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(5)));
        });
    });
});

var host = builder.Build();
await host.RunAsync();
