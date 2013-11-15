using System.Collections.Generic;
using RabbitMQ.Client.Events;
using log4net;

namespace RabbitTracer.Console.App.Services.MessagePersistence
{
    public class Log4NetMessagePersistenceStrategy : IMessagePersistenceStrategy
    {
        private readonly ILog _log;
        private readonly PersistableMessageMapper _messageMapper;

        public Log4NetMessagePersistenceStrategy(ILog log, PersistableMessageMapper messageMapper)
        {
            _log = log;
            _messageMapper = messageMapper;
        }

        public void Save(BasicDeliverEventArgs message)
        {
            var persistableMessage = _messageMapper.Map(message);
            var routingKeysFormatted = string.Join(",", persistableMessage.RoutingKeys ?? new List<string>());

            var logMessage = string.Format("Exchange:{0}|Routing Key:{1}|Message Body:{2}",
                persistableMessage.ExchangeName,
                routingKeysFormatted,
                persistableMessage.MessageBody ?? "<null>");

            _log.Info(logMessage);

        }
    }
}