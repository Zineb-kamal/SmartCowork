
namespace SmartCowork.Common.Messaging.RabbitMQ
{
    public interface IRabbitMQProducer : IDisposable
    {
        void PublishMessage<T>(string exchangeName, string routingKey, T message);
    }
}