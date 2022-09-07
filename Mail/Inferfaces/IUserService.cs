using System;
using Entities;

namespace Mail.Inferfaces
{
    public interface IUserService
    {
        User Create(User user);
        User Update(User user);
        User GetByDealerMail(string dealerMail);
        string NewTransaction();
        string NewSecretKey(int h);
        string GenSHA512(string password);
    }
}

