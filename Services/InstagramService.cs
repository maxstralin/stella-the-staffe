using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StellaTheStaffe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StellaTheStaffe.Services
{
    public class InstagramService
    {
        private readonly HttpClient HttpClient = new HttpClient();
        private readonly string Uri = "https://graph.instagram.com/{0}?limit=100&fields={1}&access_token={2}&after={3}";
        private readonly InstagramOptions options;
        private SortedDictionary<string, Stack<MediaFields>> CachedEntries { get; } = new SortedDictionary<string, Stack<MediaFields>>();
        private DateTime? LastCachedItem { get; set; }
        private TimeSpan RefreshRate = TimeSpan.FromMinutes(30);

        public InstagramService(IOptions<InstagramOptions> options)
        {
            this.options = options.Value;
        }

        public async Task TraverseAndCacheAsync()
        {
            bool traverse = true;
            var lastTimestamp = LastCachedItem.HasValue ? new DateTime(LastCachedItem.Value.Ticks) : (DateTime?)null;
            InstagramResponse? response = null;

            void ProcessItem(MediaFields item)
            {
                if (LastCachedItem == null || LastCachedItem.Value < item.Timestamp)
                {
                    LastCachedItem = item.Timestamp;
                }

                if (lastTimestamp.HasValue && item.Timestamp <= lastTimestamp.Value)
                {
                    traverse = false;
                    return;
                }

                var date = item.Timestamp!.Value.ToString("yyyy-MM-dd");

                if (!CachedEntries.ContainsKey(date))
                {
                    CachedEntries[date] = new Stack<MediaFields>();
                }
                CachedEntries[date].Push(item);
            }

            var fields = "timestamp,media_url,id,media_type,thumbnail_url";

            while (traverse)
            {
                response = await GetPostsAsync($"{fields},caption", response?.Paging?.Next);
                if (response.Paging.Next == null || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() == "development") traverse = false; //If there's no next or if we're in dev, no need to fetch more
                foreach (var item in response.Data)
                {
                    if (item.MediaType == MediaType.CAROUSEL_ALBUM)
                    {
                        var albumContents = await GetAlbumContentsAsync(item.Id!, fields);
                        foreach (var albumItem in albumContents.Data)
                        {
                            albumItem.Caption = item.Caption;
                            ProcessItem(albumItem);
                        }
                    }
                    else
                    {
                        ProcessItem(item);
                    }
                }
            }
        }

        private async Task<InstagramResponse> GetAlbumContentsAsync(string albumId, string fields)
        {
            var url = string.Format(Uri, $"{albumId}/children", fields, options.AccessToken, string.Empty);
            var unparsedResponse = await HttpClient.GetStringAsync(url);
            if (unparsedResponse is null) throw new NullReferenceException("Instagram result is null");
            return JsonConvert.DeserializeObject<InstagramResponse>(unparsedResponse, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            }) ?? throw new NullReferenceException("Couldn't deserialize response to an InstagramResponse");
        }

        private async Task<InstagramResponse> GetPostsAsync(string fields, string? next)
        {
            var url = string.Format(Uri, "me/media", fields, options.AccessToken, next);
            var unparsedResponse = await HttpClient.GetStringAsync(url);
            if (unparsedResponse is null) throw new NullReferenceException("Instagram result is null");
            return JsonConvert.DeserializeObject<InstagramResponse>(unparsedResponse, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            }) ?? throw new NullReferenceException("Couldn't deserialize response to an InstagramResponse");
        }

        public async Task<KeyValuePair<string, IEnumerable<MediaFields>>?> GetItemsByDateStringAsync(string date)
        {
            if (date is null)
            {
                throw new ArgumentNullException(nameof(date));
            }

            await TraverseAndCacheAsync();
            if (CachedEntries.TryGetValue(date, out var items))
            {
                return new KeyValuePair<string, IEnumerable<MediaFields>>(date, items);
            }
            return null;
        }

        public IEnumerable<string> GetFoundDates() => CachedEntries.Keys;

        public async Task<KeyValuePair<string, IEnumerable<MediaFields>>> GetLatestItemsAsync()
        {
            await TraverseAndCacheAsync();
            var entry = CachedEntries.Last();
            return new KeyValuePair<string, IEnumerable<MediaFields>>(entry.Key, entry.Value);
        }

    }
}
