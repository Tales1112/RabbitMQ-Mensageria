using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ_Mensageria.Config;
using RabbitMQ_Mensageria.Models;
using RabbitMQ_Mensageria.Services;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ_Mensageria.Consumer
{
    public class ProcessMessageConsumer : BackgroundService
    {
        private readonly RabbitMqConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;

        public ProcessMessageConsumer(IOptions<RabbitMqConfiguration> option, IServiceProvider serviceProvider)
        {
            _configuration = option.Value;
            _serviceProvider = serviceProvider;
            var factory = new ConnectionFactory
            {
                HostName = _configuration.Host
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(
                queue: _configuration.Queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments:null);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, eventArgs) =>
             {
                 var contentArray = eventArgs.Body.ToArray();
                 var contentString = Encoding.UTF8.GetString(contentArray);
                 var message = JsonSerializer.Deserialize<SendMessageModel>(contentString);

                 NotifyUser(message);

                 _channel.BasicAck(eventArgs.DeliveryTag, false);
             };
            _channel.BasicConsume(_configuration.Queue, false, consumer);
            return Task.CompletedTask;
        }
        public void NotifyUser(SendMessageModel message)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                notificationService.NotifyUser(message.FromUser, message.ToUser, message.Content);
            }
        }
    }
}
