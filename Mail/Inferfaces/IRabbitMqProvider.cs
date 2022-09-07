using System;
using RabbitMQ.Client;

namespace Mail.Inferfaces
{
   public  interface IRabbitMqProvider
    {
         IConnection ProvideConnection();
    }
}

