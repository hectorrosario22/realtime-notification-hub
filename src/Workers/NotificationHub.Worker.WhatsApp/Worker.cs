namespace NotificationHub.Worker.WhatsApp;

/// <summary>
/// Background service that consumes WhatsApp notification messages from RabbitMQ.
/// Implements up to 3 retry attempts; failed messages are persisted for later querying.
/// </summary>
public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("WhatsApp Worker started");

        // TODO: Register MassTransit consumer for WhatsApp queue
        // MassTransit will manage the consumer lifecycle; this method can be left as-is
        // or used for additional startup logic.

        await Task.CompletedTask;
    }
}
