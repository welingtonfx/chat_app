using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace stock_worker
{
    public class StockWorker
    {
        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };

            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "stock_search", durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueDeclare(queue: "chat_hub", durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                Console.WriteLine(" [*] Waiting for messages.");
                consumer.Received += (sender, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);

                    try
                    {
                        var stockInfo = GetStockInformation(message);
                        var stockMessage = ComposeStockMessage(stockInfo);
                        var response = Encoding.UTF8.GetBytes(stockMessage);

                        channel.BasicPublish(exchange: "", routingKey: "chat_hub", body: response);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not retrieve information for {message}");
                        var errorResponse = Encoding.UTF8.GetBytes($"Could not retrieve information for {message}");
                        channel.BasicPublish(exchange: "", routingKey: "chat_hub", body: errorResponse);
                    }

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };

                channel.BasicConsume(queue: "stock_search", autoAck: false, consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private static string ComposeStockMessage(Stock stock)
        {
            return $"{stock.Symbol} quote is ${stock.Close} per share";
        }

        private static Stock GetStockInformation(string symbol)
        {
            try
            {
                var httpClient = new HttpClient();

                var requestUrl = $"https://stooq.com/q/l/?s={symbol}&f=sd2t2ohlcv&h&e=csv";
                var responseStream = httpClient.GetStreamAsync(requestUrl).Result;

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = args => args.Header.ToLower()
                };

                using (var reader = new StreamReader(responseStream))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<Stock>();
                    return records.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not retrieve stock information for {symbol}");
            }
        }
    }
}
