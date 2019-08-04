using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace SecurityDataProvider.Common.RabbitMq
{
    public static class BasicDeliverEventArgsExtensions
    {
        public static T GetMessage<T>(this BasicDeliverEventArgs args) where T : new()
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            string body = Encoding.UTF8.GetString(args.Body);

            T response = JsonConvert.DeserializeObject<T>(body);

            return response;
        }
    }
}
