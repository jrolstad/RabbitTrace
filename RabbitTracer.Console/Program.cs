using System.Configuration;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using RabbitTracer.Console.App.Services;
using RabbitTracer.Console.App.Services.MessagePersistence;
using log4net;
using log4net.Config;

namespace RabbitTracer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var url = ConfigurationManager.AppSettings.Get("RabbitMqUrl");
            var tracingExchangeName = ConfigurationManager.AppSettings.Get("TracingExchangeName");
            var tracingQueueName = ConfigurationManager.AppSettings.Get("TracingQueuename");

            var connectionFactory = new ConnectionFactory {Uri = url};

            using ( var connection = connectionFactory.CreateConnection())
            using ( var channel = connection.CreateModel())
            {
                // Create the queue for subscribing to
                channel.QueueDeclare(queue: tracingQueueName, durable: false, exclusive: false, autoDelete: true, arguments: null);
                channel.QueueBind(queue: tracingQueueName,exchange:tracingExchangeName,routingKey: null);

                // Subscribe to messages
                using (var subscription = new Subscription(model: channel, queueName: tracingQueueName))
                {
                    var messagePersistor = new Log4NetMessagePersistenceStrategy(
                        log:LogManager.GetLogger(typeof (Program)), 
                        messageMapper: new PersistableMessageMapper());

                    foreach (BasicDeliverEventArgs message in subscription)
                    {
                        messagePersistor.Save(message);

                        subscription.Ack(message);
                    }
                }
            }
        }
    }
}
