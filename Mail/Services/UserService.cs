using System;
using System.Security.Cryptography;
using System.Text;
using DB.Interfaces;
using DB.Migrations;
using Entities;
using Mail.Inferfaces;
using Microsoft.EntityFrameworkCore;

namespace Mail.Services
{
    public class UserService : IUserService
    {
      
        private readonly IUserInterface _users;
        public UserService(IUserInterface users)
        {
            _users = users;
        }

        public User Create(User user)
        {
            return _users.Create(user);
        }
        public User Update(User user)
        {
            return _users.Update(user);
        }
        public User GetByDealerMail(string dealerMail)
        {
            return _users.GetByDealerMail(dealerMail);
        }
        public string NewTransaction()
        {
            return randomGenerator(15);
        }
        public string NewSecretKey(int keyLength)
        {
            RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            byte[] randomBytes = new byte[keyLength];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
        private string randomGenerator(int h)
        {
            Random random = new Random();
            char[] randomText = new char[16];
            for (int i = 0; i < h; i++)
            {
                randomText[i] = (char)(random.Next(0, 2) == 0
                    ? random.Next(48, 58) //0-9
                    : random.Next(0, 2) == 0
                        ? random.Next(65, 91) //A-Z
                        : random.Next(97, 123)); //a-z
            }
            return new string(randomText);
        }
        public string GenSHA512(string s)
        {
            string r = "";

            try
            {
                byte[] d = Encoding.UTF8.GetBytes(s);

                using (SHA512 a = new SHA512Managed())
                {
                    byte[] h = a.ComputeHash(d);
                    r = BitConverter.ToString(h).Replace("-", "");
                }

            }
            catch
            {

            }

            return r;
        }
    }
}

