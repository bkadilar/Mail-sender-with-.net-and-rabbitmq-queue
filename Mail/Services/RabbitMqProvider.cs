using System;
using Mail.Inferfaces;

namespace Mail.Services
{
    public class RabbitMqProvider : IRabbitMqProvider
    {
        public RabbitMQ.Client.IConnection ProvideConnection()
        {
            var factory = new RabbitMQ.Client.ConnectionFactory { HostName = "93.113.61.221", UserName = "myuser", Password = "123QWEasd**" };

            return factory.CreateConnection();
        }
    }
}

