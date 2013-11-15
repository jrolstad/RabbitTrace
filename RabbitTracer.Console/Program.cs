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
            var url = ConfigurationManager.AppSettings.Get("RabbitMqUrl");
            
            var connectionFactory = new ConnectionFactory {Uri = url};

            using ( var connection = connectionFactory.CreateConnection())
            using ( var channel = connection.CreateModel())
            {
                const string tracingExchangeName = "amq.rabbitmq.trace";
                const string tracingQueueName = "some_tracing_queue";

                // Create the queue for subscribing to
                channel.QueueDeclare(queue: tracingQueueName, durable: false, exclusive: false, autoDelete: true, arguments: null);
                channel.QueueBind(queue: tracingQueueName,exchange:tracingExchangeName,routingKey: null);

                // Subscribe to messages
                using (var subscription = new Subscription(model: channel, queueName: tracingQueueName))
                {
                    var messageEncoding = Encoding.UTF8;

                    XmlConfigurator.Configure();
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
