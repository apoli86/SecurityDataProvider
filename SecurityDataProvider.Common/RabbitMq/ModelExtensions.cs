using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecurityDataProvider.Common.RabbitMq
{
    public static class ModelExtensions
    {
        public static void CreateQueueIfNotExists(this IModel channel, string queueName)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );
        }

        public static IBindConsumerFluent BindConsumer(this IModel channel, Action<IModel, BasicDeliverEventArgs> eventHandler)
        {
            return new BindConsumerFluent(channel, eventHandler);
        }

        public static void PublishMessageOnQueue<T>(this IModel channel, T message, string queue) where T : new()
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }

            var body = JsonConvert.SerializeObject(message);

            channel.PublishMessageOnQueue(body, queue);
        }

        public static void PublishMessageOnQueue(this IModel channel, string message, string queue)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }

            var body = Encoding.UTF8.GetBytes(message);

            channel.PublishMessageOnQueue(body, queue);
        }

        public static void PublishMessageOnQueue(this IModel channel, byte[] message, string queue)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: "",
                routingKey: queue,
                properties,
                message
            );
        }

        public static void SendAck(this IModel channel, ulong deliveryTag)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            channel.BasicAck(deliveryTag, multiple: false);
        }

        public static void SendNack(this IModel channel, ulong deliveryTag)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            channel.BasicNack(deliveryTag, multiple: false, requeue: true);
        }
    }

    public class BindConsumerFluent : IBindConsumerFluent, IBindConsumerOnQueueFluent, IBindConsumerAckFluent
    {
        private readonly IModel channel;
        private readonly Action<IModel, BasicDeliverEventArgs> eventHandler;
        private string queue;
        private CancellationTokenSource cancellationTokenSource;

        public BindConsumerFluent(IModel channel, Action<IModel, BasicDeliverEventArgs> eventHandler)
        {
            this.channel = channel;
            this.eventHandler = eventHandler;
        }

        public IBindConsumerOnQueueFluent WithCancellationToken(CancellationTokenSource cancellationTokenSource)
        {
            this.cancellationTokenSource = cancellationTokenSource;

            return this;
        }

        public IBindConsumerAckFluent OnQueue(string queue)
        {
            this.queue = queue;

            return this;
        }

        public async Task WithAutomaticAck()
        {
            await BindConsumer(withAck: true);
        }



        public async Task WithManualAck()
        {
            await BindConsumer(withAck: false);
        }

        private async Task BindConsumer(bool withAck)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, msg) =>
            {
                try
                {
                    eventHandler(channel, msg);
                }
                catch(RabbitMQClientException)
                {
                    cancellationTokenSource.Cancel();
                }
            };

            cancellationTokenSource.Token.Register(() =>
            {
                channel.BasicCancel(consumer.ConsumerTag);
                channel.BasicRecover(true);
            });

            await Task.Run(() =>
            {
                channel.BasicConsume(queue, withAck, consumer);
                cancellationTokenSource.Token.WaitHandle.WaitOne();
            });
        }
    }

    public interface IBindConsumerFluent
    {
        IBindConsumerOnQueueFluent WithCancellationToken(CancellationTokenSource cancellationTokenSource);
    }

    public interface IBindConsumerOnQueueFluent
    {
        IBindConsumerAckFluent OnQueue(string queue);
    }

    public interface IBindConsumerAckFluent
    {
        Task WithAutomaticAck();
        Task WithManualAck();
    }
}
