using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StellaTheStaffe.Models
{
    public class MongoDbOptions
    {
        public MongoDbOptions(string user, string password)
        {
            User = user;
            Password = password;
        }

        public string User { get; }
        public string Password { get; }
    }
}
