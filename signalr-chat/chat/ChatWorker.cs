using chat.Hubs;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace chat
{
    public class ChatWorker : BackgroundService
    {
        private readonly IHubContext<ChatHub> _chatHub;

        public ChatWorker(IHubContext<ChatHub> chatHub)
        {
            _chatHub = chatHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //RabbitMQ
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "chat_hub", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _chatHub.Clients.All.SendAsync("ReceiveMessage", "Robot", message);
                };
                channel.BasicConsume(queue: "chat_hub", autoAck: true, consumer: consumer);

                while (!stoppingToken.IsCancellationRequested)
                {
                    Console.WriteLine("[ChatWorker] Listening...");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}
