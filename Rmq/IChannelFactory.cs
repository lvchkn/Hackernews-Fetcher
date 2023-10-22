using RabbitMQ.Client;

namespace Hackernews_Fetcher.Rmq;

public interface IChannelFactory
{
    IModel? Create();
}