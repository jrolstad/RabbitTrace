using System.Configuration;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
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
                    var messageEncoding = Encoding.UTF8;


                    var log = LogManager.GetLogger(typeof (Program));

                    foreach (BasicDeliverEventArgs message in subscription)
                    {
                        // Process each message
                        var body = messageEncoding.GetString(message.Body);
                        var exchange = message.Exchange;
                        var routingKey = message.RoutingKey;

                        log.InfoFormat("{0}|{1}|{2}",exchange,routingKey,body);

                        subscription.Ack(message);
                    }
                }
            }
        }
    }
}
