using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client.Events;

namespace RabbitTracer.Console.App.Services
{
    public class PersistableMessageMapper
    {
        private readonly Encoding _messageEncoding = Encoding.UTF8;

        public virtual PersistableMessage<string> Map(BasicDeliverEventArgs message)
        {
            var messageBody = DecodeValue(message.Body);

            var exchangeHeaderValue = GetMessageHeader<byte[]>(message, "exchange_name");
            var exchange = DecodeValue(exchangeHeaderValue);

            var routingKeysHeaderValue = GetMessageHeader<ArrayList>(message, "routing_keys");
            var routingKeys = routingKeysHeaderValue
                .Cast<byte[]>()
                .Select(DecodeValue)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();

            return new PersistableMessage<string>
            {
                ExchangeName = exchange,
                MessageBody = messageBody,
                RoutingKeys = routingKeys
            };
        }

        private static T GetMessageHeader<T>(BasicDeliverEventArgs message, string headerName)
        {
            try
            {
                return (T)message.BasicProperties.Headers[headerName];
            }
            catch (KeyNotFoundException exception)
            {
                const string exceptionMessageFormat = "Unable to find Header '{0}'.  Available values are: {1}";
                var exceptionMessage = string.Format(exceptionMessageFormat, headerName, string.Join(",", message.BasicProperties.Headers.Keys));

                throw new KeyNotFoundException(exceptionMessage, exception);
            }
        }

        private string DecodeValue(byte[] message)
        {
            return message == null ?
                null :
                _messageEncoding.GetString(message);
        }
    }
}