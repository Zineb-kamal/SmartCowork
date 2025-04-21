
namespace SmartCowork.Common.Messaging.Messages
{
    public interface IMessage
    {
        Guid Id { get; }
        DateTime Timestamp { get; }
    }
}
