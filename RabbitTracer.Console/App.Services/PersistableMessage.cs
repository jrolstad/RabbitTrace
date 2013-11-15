using System.Collections.Generic;

namespace RabbitTracer.Console.App.Services
{
    public class PersistableMessage<T>
    {
        public string ExchangeName { get; set; }

        public List<string> RoutingKeys { get; set; }

        public T MessageBody { get; set; }
    }
}