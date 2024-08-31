using RabbitMQ.Client;
using System.Text;

namespace UserService.Services
{
    public interface IRabbitMQSenderService
    {
        bool SendMessage(string message);
    }

    public class RabbitMQSenderService : IRabbitMQSenderService
    {
        public const string ExchangeName = "UserExchange";
        public const string QueueName = "UserQueue";
        public const string RoutingKey = "UserKey";
        public const string HostName = "rabbitmq";

        public bool SendMessage(string message)
        {
            try
            {
                var factory = new ConnectionFactory { HostName = HostName };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
                        channel.QueueDeclare(QueueName, false, false, false);
                        channel.QueueBind(QueueName, ExchangeName, RoutingKey);

                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(ExchangeName, RoutingKey, null, body);

                        channel.Close();
                        connection.Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {

            }

            return false;
        }
    }
}
