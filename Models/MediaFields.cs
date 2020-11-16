using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StellaTheStaffe.Models
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class MediaFields
    {
        [JsonConstructor]
        public MediaFields(DateTime? timestamp, string? mediaUrl, string? id, MediaType? mediaType, string? caption, string? thumbnailUrl, string? permalink, string? username)
        {
            Timestamp = timestamp?.ToUniversalTime();
            MediaUrl = mediaUrl;
            Id = id;
            MediaType = mediaType;
            Caption = caption;
            ThumbnailUrl = thumbnailUrl;
            Permalink = permalink;
            Username = username;
        }

        public DateTime? Timestamp { get; set; }
        public string? MediaUrl { get; set; }
        public string? Id { get; set; }
        public MediaType? MediaType { get; set; }
        public string? Caption { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Permalink { get; set; }
        public string? Username { get; set; }

    }
}
