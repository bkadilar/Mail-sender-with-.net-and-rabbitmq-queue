using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Mail.Models
{
    public class MailRequest
    {
        public To To { get; set; }
        public From From { get; set; }
        public string[] Mails { get; set; }
    }

    public class From
    {
        public string? Email { get; set; }
        public string Password { get; set; }
        public string host { get; set; }
        public int port { get; set; }
        public bool enableSsl { get; set; }

    }
    public class To
    {
        public string subject { get; set; }
        public string message { get; set; }
        public List<IFormFile> attachments { get; set; }

    }
 

    public class getMailRequestOnQueue
    {
        public To To { get; set; }
        public From From { get; set; }
        public string Mail { get; set; }
        public int Id { get; set; } = 0;
        public string DealerMail { get; set; }
        public string TransactionId { get; set; }
        public string Guid { get; set; }
        public int UserId { get; set; }

    }
}
