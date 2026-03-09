namespace NotificationHub.Worker.Sms;

/// <summary>
/// Background service that consumes SMS notification messages from RabbitMQ.
/// Implements up to 3 retry attempts; failed messages are persisted for later querying.
/// </summary>
public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SMS Worker started");

        // TODO: Register MassTransit consumer for SMS queue
        // MassTransit will manage the consumer lifecycle; this method can be left as-is
        // or used for additional startup logic.

        await Task.CompletedTask;
    }
}
