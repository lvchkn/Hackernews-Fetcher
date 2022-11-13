using System.Text;
using System.Text.Json;
using Models;
using RabbitMQ.Client;

namespace RabbitConnections.Publisher;

public class Publisher
{
    private readonly IModel? _channel;

    public Publisher(IChannelFactory channelFactory)
    {
        _channel = channelFactory.Create();
    }

    public void Publish<TMessage>(string exchangeName, TMessage message) where TMessage : class, IMessage
    {
        _channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);
        var queueName = _channel.QueueDeclare("stories", exclusive: false).QueueName;

        const string routingKey = "feed.story";

        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var serializedMessageBody = JsonSerializer.Serialize(message, jsonSerializerOptions);
        var messageBody = Encoding.UTF8.GetBytes(serializedMessageBody);

        _channel.QueueBind(queueName, exchangeName, routingKey);

        _channel.BasicPublish(exchangeName, routingKey, body: messageBody);
    }
}