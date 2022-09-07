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
    public class UserRepository : IUserInterface
    {
        private DataContext context;
        public UserRepository(DataContext _context)
        {
            context = _context;
        }
        public User Create(User user)
        {
            context.users.Add(user);
            context.SaveChanges();

            return user;
        }
        public async Task<User> CreateAsync(User user)
        {
            context.users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }
        public async Task<User> UpdateAsync(User user)
        {
            context.users.Update(user);
            await context.SaveChangesAsync();

            return (user);
        }
        public User Update(User user)
        {
            context.users.Update(user);
            context.SaveChanges();

            return (user);
        }
        public Task<User> GetById(int id)
        {
            return Task.FromResult(context.users.FirstOrDefault(p => p.id == id));
        }
        public Task<User> GetByGuid(string guid)
        {
            return Task.FromResult(context.users.FirstOrDefault(p => p.guid == guid));
        }
        public User GetByDealerMail(string dealerMail)
        {
            return context.users.FirstOrDefault(p => p.dealer_mail ==dealerMail);
        }
    }
}
