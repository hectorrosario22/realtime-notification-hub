using MassTransit;
using NotificationHub.Application.Interfaces;

namespace NotificationHub.Infrastructure.Messaging;

public sealed class MassTransitMessagePublisher(IPublishEndpoint publishEndpoint) : IMessagePublisher
{
    public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
        => publishEndpoint.Publish(message, ct);
}
