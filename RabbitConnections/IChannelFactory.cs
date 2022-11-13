using RabbitMQ.Client;

namespace RabbitConnections;

public interface IChannelFactory
{
    IModel? Create();
}