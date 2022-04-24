using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using System.Text;
using System.Text.RegularExpressions;

namespace chat.Hubs
{
    public class ChatHub : Hub
    {
        Regex regexPattern = new Regex("^/stock=(.*)$");

        public async Task SendMessage(string user, string message)
        {
            if (IsMessageACommand(message))
                await SendMassageToStockWorker(message);
            else
                await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        private bool IsMessageACommand(string message)
        {
            return regexPattern.IsMatch(message);
        }

        private async Task SendMassageToStockWorker(string message)
        {
            var split = message.Split('=');
            var stockCode = split[1];

            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "stock_search", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(stockCode);

                channel.BasicPublish(exchange: "", routingKey: "stock_search", body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }
        }
    }
}
