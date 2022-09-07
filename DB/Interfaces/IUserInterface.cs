using System;
using Entities;

namespace DB.Interfaces
{
    public interface IUserInterface
    {
        Task<User> CreateAsync(User user);
        User Create(User user);
        Task<User> UpdateAsync(User user);
        Task<User> GetById(int id);
        Task<User> GetByGuid(string guid);
        User Update(User user);
        User GetByDealerMail(string dealerMail);
    }
}

