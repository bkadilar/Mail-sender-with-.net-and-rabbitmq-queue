using DB.Interfaces;
using Mail.Models;
using Mail.Services;
using RabbitMQ.Client;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Mail.Services
{
    
    public class Sender : ISenderService
    {
        private readonly IMailInterface _mails;
        public Sender(IMailInterface mails)
        {
            _mails = mails;
        }
        public async Task<bool> SendEmailAsync(getMailRequestOnQueue mailRequest)
        {
            Entities.Mail thisMail = await _mails.GetById(mailRequest.Id);
            thisMail.modify_time = DateTime.Now;
            try
            {
                MailSettings _mailSettings = new MailSettings();
                _mailSettings.Mail = mailRequest.From.Email;
                _mailSettings.Password = mailRequest.From.Password;
                _mailSettings.Host = mailRequest.From.host;
                _mailSettings.Port = mailRequest.From.port;
                _mailSettings.EnableSsl = mailRequest.From.enableSsl;

                MailMessage message = new MailMessage();
                
                //System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                message.From = new MailAddress(_mailSettings.Mail,mailRequest.To.subject);
                message.To.Add(new MailAddress(mailRequest.Mail));
                message.Subject = mailRequest.To.subject;
                message.IsBodyHtml = true; //to make message body as html
                message.Body = mailRequest.To.message;
                message.ReplyToList.Add(new MailAddress(_mailSettings.Mail));
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Port = _mailSettings.Port;
                    smtp.Host = _mailSettings.Host; //for gmail host  
                    smtp.EnableSsl = _mailSettings.EnableSsl;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(_mailSettings.Mail, _mailSettings.Password);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Timeout = 5000;
                    smtp.Send(message);

                    thisMail.status = 1;
                     _mails.Update(thisMail);

                }

            }
            catch (System.Exception ex)
            {
                Console.Write(ex);
                thisMail.status = 2;
                thisMail.reason = JsonConvert.SerializeObject(ex);
                 _mails.Update(thisMail);
            }
            return true;
        }

    }
}
