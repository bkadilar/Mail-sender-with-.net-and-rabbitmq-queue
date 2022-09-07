using System;
using RabbitMQ.Client;
using System.Text;
using Mail.Inferfaces;

namespace Mail.Services
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    { 

        public bool Publish(IModel channel,string message ,string queeName)
        {
           
            var body = Encoding.UTF8.GetBytes(message);
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: "",
                                         routingKey: queeName,
                                         basicProperties: properties,
                                         body: body);
                    Console.WriteLine(" [x] Sent {0}", message);
                
            
            return true;
        }
    }
}

