using Mediator;
using NotificationHub.Application.Common.Mappings;
using NotificationHub.Application.Common.Models;
using NotificationHub.Application.Interfaces;
using NotificationHub.Application.Notifications.Queries;

namespace NotificationHub.Application.Notifications.Handlers;

public sealed class GetAllNotificationsQueryHandler(INotificationRepository notificationRepository)
    : IRequestHandler<GetAllNotificationsQuery, IReadOnlyCollection<NotificationResult>>
{
    public async ValueTask<IReadOnlyCollection<NotificationResult>> Handle(
        GetAllNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        var notifications = await notificationRepository.GetAllAsync(cancellationToken);
        return notifications.Select(notification => notification.ToResult()).ToArray();
    }
}
