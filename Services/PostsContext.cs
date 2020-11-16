using MongoDB.Driver;
using StellaTheStaffe.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StellaTheStaffe.Services
{
    public class PostsContext
    {
        public readonly IMongoCollection<InstagramData> Data;
        public PostsContext(IMongoClient client)
        {
            Data = client.GetDatabase("instagram").GetCollection<InstagramData>("persistedPosts");
        }
    }
}
