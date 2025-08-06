using Content.Shared.Notifications;
using Robust.Shared.Network;

namespace Content.Client.Notifications;

public sealed class NotificationManager
{
    [Dependency] private readonly IClientNetManager _netManager = default!;

    private readonly List<SharedNotification> _notifications = [];
    private NotificationsWindow? _window;

    public NotificationsWindow Window
    {
        set
        {
            UnsubscribeWindow();
            _window = value;
            SubscribeWindow();
        }
    }

    public void Initialize()
    {
           _netManager.RegisterNetMessage<NotificationsUpdatedMsg>(ReceivedNotificationsUpdatedMsg);
    }

    private void ReceivedNotificationsUpdatedMsg(NotificationsUpdatedMsg message)
    {
        if (message.Clear)
            _notifications.Clear();

        if (message.Notifications is not { Count: > 1 })
            return;

        _notifications.AddRange(message.Notifications);
    }

    private void SubscribeWindow()
    {
        if (_window == null)
            return;

    }

    private void UnsubscribeWindow()
    {
        if (_window == null)
            return;

    }
}
