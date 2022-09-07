using System;
using RabbitMQ.Client;

namespace Mail.Inferfaces
{
    public interface IRabbitMqPublisher
    {
        bool Publish(IModel? channel,string message, string queeName);
    }
}

