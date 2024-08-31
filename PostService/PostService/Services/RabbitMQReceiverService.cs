using Microsoft.EntityFrameworkCore;
using PostService.Data;
using PostService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PostService.Services
{
    public class RabbitMQReceiverService : BackgroundService
    {
        private readonly ILogger<RabbitMQReceiverService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IModel _channel;

        public const string ExchangeName = "UserExchange";
        public const string QueueName = "UserQueue";
        public const string RoutingKey = "UserKey";
        public const string HostName = "rabbitmq";

        public RabbitMQReceiverService(ILogger<RabbitMQReceiverService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider; // Inject IServiceProvider

            var factory = new ConnectionFactory { HostName = HostName };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
            _channel.QueueDeclare(QueueName, false, false, false);
            _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
            _channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (sender, args) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<PostServiceContext>();

                    var body = args.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var data = JsonSerializer.Deserialize<RabbitMQResponse>(message, options);
                    _logger.LogInformation($"Received message: {message}");

                    if (data != null && data.Type == "UserUpdate")
                    {
                        var posts = await context.Post.Where(x => x.AuthorId == data.UserId).ToListAsync();
                        if (posts.Any())
                        {
                            foreach (var post in posts)
                            {
                                post.AuthorName = data.UserName;
                            }
                            await context.SaveChangesAsync();
                            _logger.LogInformation("Database updated successfully.");
                        }
                        else
                        {
                            _logger.LogWarning("No posts found for the specified UserId.");
                        }
                    }

                    _channel.BasicAck(args.DeliveryTag, false);
                }
            };

            _channel.BasicConsume(QueueName, false, consumer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQReceiverService is starting.");

            stoppingToken.Register(() => _logger.LogInformation("RabbitMQReceiverService is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("RabbitMQReceiverService is running.");

                await Task.Delay(1000, stoppingToken); // Delay to prevent a tight loop
            }

            _logger.LogInformation("RabbitMQReceiverService has stopped.");
        }


        public override void Dispose()
        {
            _channel?.Dispose();
            base.Dispose();
        }
    }
}
