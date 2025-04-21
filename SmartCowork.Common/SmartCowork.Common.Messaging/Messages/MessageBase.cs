

namespace SmartCowork.Common.Messaging.Messages
{
    public abstract class MessageBase : IMessage
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}