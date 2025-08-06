using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Notifications;

public sealed class NotificationsUpdatedMsg : NetMessage
{
    public override MsgGroups MsgGroup { get; } = MsgGroups.Command;

    public bool Clear { get; set; }
    public List<SharedNotification>? Notifications;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Clear = buffer.ReadBoolean();
        buffer.ReadPadBits();

        var count = buffer.ReadVariableInt32();
        Notifications = new List<SharedNotification>(count);

        for (var i = 0; i < count; i++)
        {
            Notifications.Add(new SharedNotification(
                buffer.ReadGuid(),
                buffer.ReadString(),
                buffer.ReadString(),
                buffer.ReadString(),
                buffer.ReadString()
                ));
        }

    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Clear);
        buffer.WritePadBits();

        if (Notifications == null)
            return;

        buffer.WriteVariableInt32(Notifications.Count);
        foreach (var notification in Notifications)
        {
            buffer.Write(notification.Id);
            buffer.Write(notification.Title);
            buffer.Write(notification.Message);
            buffer.Write(notification.ActionLabel);
            buffer.Write(notification.ActionCommand);
        }
    }
}
