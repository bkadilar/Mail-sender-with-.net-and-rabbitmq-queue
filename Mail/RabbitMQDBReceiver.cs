using DB.Interfaces;
using Entities;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mail
{
    public class RabbitMQDBReceiver : BackgroundService
    {

        private IConnection _connection;
        private IModel _channel;
        private ISenderService _senderService;
        private IMailInterface _mails;
        public RabbitMQDBReceiver(IMailInterface mails,ISenderService senderService, ILoggerFactory loggerFactory)
        {
            InitRabbitMQ();
            _senderService = senderService;
            _mails = mails;
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory { HostName = "93.113.61.221", UserName = "myuser", Password = "123QWEasd**" };
            Global.DBQueueName = "mail_create";
            // create connection  
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
            //   _channel.QueueDeclare(Global.QueeName, false, false, false, null); //demo.queee.log
            _channel.QueueBind(Global.DBQueueName, "demo.exchange", "demo.queue.*", null);
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

            _channel.BasicConsume(Global.DBQueueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task<Boolean> HandleMessage(string mailRequest)
        {
            // we just print this message


            try
            {

                getMailRequestOnQueue getMailRequestOnQueue = JsonConvert.DeserializeObject<getMailRequestOnQueue>(mailRequest);
                Entities.Mail thisMail = new Entities.Mail();
                thisMail.status = 0;
                thisMail.subject = getMailRequestOnQueue.To.subject;
                thisMail.message = getMailRequestOnQueue.To.message;
                thisMail.to_email = getMailRequestOnQueue.Mail;
                thisMail.from_email = getMailRequestOnQueue.From.Email;
                thisMail.from_password = getMailRequestOnQueue.From.Password;
                thisMail.from_host = getMailRequestOnQueue.From.host;
                thisMail.from_port = getMailRequestOnQueue.From.port;
                thisMail.from_enable_ssl = getMailRequestOnQueue.From.enableSsl;
                thisMail.dealer_mail = getMailRequestOnQueue.DealerMail;
                thisMail.guid = getMailRequestOnQueue.Guid;
                thisMail.user_id = getMailRequestOnQueue.UserId;
                thisMail.transaction_id = getMailRequestOnQueue.TransactionId;
                thisMail.create_time = DateTime.Now;
                thisMail.modify_time = DateTime.Now;
                Entities.Mail createdMail =  _mails.Create(thisMail);

                getMailRequestOnQueue.Id = createdMail.id;
                var newMailRequest = JsonConvert.SerializeObject(getMailRequestOnQueue);
                this.sendToQueue(newMailRequest);
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.ToString());
                return false;
            }

        }
        private void sendToQueue(string msg)
        {
            var factory = new ConnectionFactory { HostName = "93.113.61.221", UserName = "myuser", Password = "123QWEasd**" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "mail_server",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);


                var body = Encoding.UTF8.GetBytes(msg);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                     routingKey: "mail_server",
                                     basicProperties: properties,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", msg);
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

