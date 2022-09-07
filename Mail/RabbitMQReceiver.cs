using Mail.Models;
using Mail.Services;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace Mail
{
    public class RabbitMQReceiver : BackgroundService
    {

        private IConnection _connection;
        private IModel _channel;
        private ISenderService _senderService;
        public RabbitMQReceiver(ISenderService senderService,ILoggerFactory loggerFactory)
        {
            InitRabbitMQ();
            _senderService = senderService;
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory { HostName = "93.113.61.221", UserName = "myuser", Password = "123QWEasd**" };
            Global.QueueName = "mail_server";
            // create connection  
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
            //   _channel.QueueDeclare(Global.QueeName, false, false, false, null); //demo.queee.log
            _channel.QueueBind(Global.QueueName, "demo.exchange", "demo.queue.*", null);
            _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message  
                var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

                // handle the received message  
                HandleMessage(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(Global.QueueName, false, consumer);
            return Task.CompletedTask;
        }

        private  Task<Boolean> HandleMessage(string mailRequest)
        {
            // we just print this message
            
    
            try
            {
                 _senderService.SendEmailAsync(JsonConvert.DeserializeObject<getMailRequestOnQueue>(mailRequest));
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.ToString());
                return Task.FromResult(false);
            }

        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}

