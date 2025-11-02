using RabbitMQ.Client;

namespace AMESA_be.Queue.rabbitmq.Infra;

public interface IRabbitConnectionBuilder
{
    IConnection? CreateConnection();
}
