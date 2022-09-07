using System;
using Entities;
namespace DB.Interfaces
{
    public interface IMailInterface
    {
        Task<Mail> CreateAsync(Mail mail);
        Mail Create(Mail mail);
        Task<Mail> UpdateAsync(Mail mail);
        Task<Mail> GetById(int id);
        Mail Update(Mail mail);
    }
}

