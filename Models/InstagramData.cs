using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StellaTheStaffe.Models
{
    public class InstagramData
    {
        [BsonConstructor]
        public InstagramData(string id, DateTime timestamp, OembedData oembedData)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Timestamp = timestamp.ToUniversalTime();
            OembedData = oembedData ?? throw new ArgumentNullException(nameof(oembedData));
        }

        [BsonId, BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string Id { get; }

        public DateTime Timestamp { get; set; }

        public OembedData OembedData { get; }
    }
}
