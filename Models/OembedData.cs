using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StellaTheStaffe.Models
{
    public class OembedData
    {
        [JsonConstructor, BsonConstructor]
        public OembedData(string authorName, string html, string providerName, string providerUrl, string thumbnailHeight, string thumbnailUrl, string thumbnailWidth, string type, string version, int width)
        {
            AuthorName = authorName;
            Html = html;
            ProviderName = providerName;
            ProviderUrl = providerUrl;
            ThumbnailHeight = thumbnailHeight;
            ThumbnailUrl = thumbnailUrl;
            ThumbnailWidth = thumbnailWidth;
            Type = type;
            Version = version;
            Width = width;
        }

        public string AuthorName { get; }
        public string Html { get; }
        public string ProviderName { get; }
        public string ProviderUrl { get; }
        public string ThumbnailHeight { get; }
        public string ThumbnailUrl { get; }
        public string ThumbnailWidth { get; }
        public string Type { get; }
        public string Version { get; }
        public int Width { get; }
    }
}
