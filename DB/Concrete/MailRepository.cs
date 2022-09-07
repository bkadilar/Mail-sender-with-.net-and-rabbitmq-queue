using Entities;
using DB.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace DB.Concrete
{
    public class MailRepository : IMailInterface
    {
        private DataContext context;
        public MailRepository(DataContext _context)
        {
            context = _context;
        }
        public Mail Create(Mail mail)
        {
            context.mails.Add(mail);
            context.SaveChanges();

            return mail;
        }
        public async Task<Mail> CreateAsync(Mail mail)
        {
            context.mails.Add(mail);
            await context.SaveChangesAsync();

            return mail;
        }
        public async Task<Mail> UpdateAsync(Mail mail)
        {
            context.mails.Update(mail);
            await context.SaveChangesAsync();

            return (mail);
        }
        public Mail Update(Mail mail)
        {
            context.mails.Update(mail);
             context.SaveChanges();

            return (mail);
        }
        public  Task<Mail> GetById(int id)
        {
            return Task.FromResult ( context.mails.FirstOrDefault(p => p.id == id));
        }
    }
}
