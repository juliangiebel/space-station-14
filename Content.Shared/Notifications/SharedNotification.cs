using Robust.Shared.Serialization;

namespace Content.Shared.Notifications;

[Serializable]
[NetSerializable]
public sealed class SharedNotification
{
    public Guid Id { get; }
    public string Title { get; }
    public string Message { get; }
    public string ActionLabel { get; }
    public string ActionCommand { get; }

    public SharedNotification(Guid id, string title, string message, string actionLabel, string actionCommand)
    {
        Id = id;
        Title = title;
        Message = message;
        ActionLabel = actionLabel;
        ActionCommand = actionCommand;
    }
}
