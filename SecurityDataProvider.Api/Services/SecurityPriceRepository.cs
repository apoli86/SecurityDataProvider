using SecurityDataProvider.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecurityDataProvider.Entities.Configuration;

namespace SecurityDataProvider.Api.Services
{
    public class SecurityPriceRepository
    {
        private readonly IDictionary<string, Entities.Requests.SecurityPrice> securityPriceDictionary;
        private readonly ConnectionFactory factory;
        private readonly NLog.ILogger logger;

        public SecurityPriceRepository(IOptions<RabbitMQCredential> config, NLog.ILogger logger)
        {
            this.securityPriceDictionary = new ConcurrentDictionary<string, Entities.Requests.SecurityPrice>();

            this.factory = new ConnectionFactory() { HostName = config.Value.Host, UserName = config.Value.Username, Password = config.Value.Password };
            this.logger = logger;
        }

        public Entities.Requests.SecurityPrice GetSecurityPrice(string symbol)
        {
            logger.Log(LogLevel.Info, $"Processing SecurityPrice request for Symbol [{symbol}] ");
            string sanitizedSymbol = symbol?.Trim().ToUpper() ?? string.Empty;

            try
            {
                

                using (IConnection connection = factory.CreateConnection())
                using (IModel channel = connection.CreateModel())
                {
                    ReadSecurityPrice(channel);

                    Entities.Requests.SecurityPrice securityPrice;
                    if (securityPriceDictionary.TryGetValue(sanitizedSymbol, out securityPrice))
                    {
                        return securityPrice;
                    }

                    SendSecurityPriceRequest(channel, sanitizedSymbol);

                    return new Entities.Requests.SecurityPrice() { Symbol = sanitizedSymbol };
                }

            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, e);

                return new Entities.Requests.SecurityPrice() { Symbol = sanitizedSymbol };
            }
        }

        private void SendSecurityPriceRequest(IModel channel, string symbol)
        {
            channel.QueueDeclare(queue: "SecurityPriceRequest",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var request = new Entities.Requests.SecurityPriceRequest() { Symbol = symbol };
            string message = JsonConvert.SerializeObject(request);
            var body = Encoding.UTF8.GetBytes(message);

            logger.Log(LogLevel.Info, $"Publishing SecurityPriceRequest to SecurityPriceRequest queue");

            channel.BasicPublish(exchange: "",
                                 routingKey: "SecurityPriceRequest",
                                 basicProperties: null,
                                 body: body);
        }


        private void ReadSecurityPrice(IModel channel)
        {
            channel.QueueDeclare(queue: "SecurityPriceResponse",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            logger.Log(LogLevel.Info, $"Reading SecurityPrice from SecurityPriceResponse");

            var message = channel.BasicGet(queue: "SecurityPriceResponse",
                             autoAck: true);

            if (message == null)
                return;

            var body = Encoding.UTF8.GetString(message.Body);

            Entities.Requests.SecurityPrice securityPrice = JsonConvert.DeserializeObject<Entities.Requests.SecurityPrice>(body);

            if (!this.securityPriceDictionary.ContainsKey(securityPrice.Symbol))
            {
                this.securityPriceDictionary.Add(securityPrice.Symbol, securityPrice);
            }

        }
    }
}
