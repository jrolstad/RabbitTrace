using RabbitMQ.Client.Events;

namespace RabbitTracer.Console.App.Services.MessagePersistence
{
    public interface IMessagePersistenceStrategy
    {
        void Save(BasicDeliverEventArgs message);
    }
}