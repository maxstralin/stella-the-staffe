using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StellaTheStaffe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StellaTheStaffe.Services
{
    public class InstagramFetchService : IHostedService
    {
        private readonly PostsContext context;
        private readonly TimeSpan RefreshRate = TimeSpan.FromMinutes(30);
        private readonly HttpClient HttpClient = new HttpClient();
        private readonly InstagramOptions options;
        private Timer? Timer = null;
        private string? AppToken;

        public static event EventHandler<IEnumerable<InstagramData>>? NewData;


        public InstagramFetchService(PostsContext context, IOptions<InstagramOptions> options)
        {
            this.context = context;
            this.options = options.Value;
        }

        private string GetMediaUri(int limit, string fields, string? after) => $"https://graph.instagram.com/me/media?limit={limit}&fields={fields}&access_token={options.AccessToken}&after={after}";
        private async Task<string> GetOembedUriAsync(string permalink)
        {
            AppToken ??= await GetAppAccessTokenAsync();
            return $"https://graph.facebook.com/v9.0/instagram_oembed?url={permalink}&access_token={AppToken}";
        }

        private async Task<string> GetAppAccessTokenAsync()
        {
            var data = await HttpClient.GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={options.ClientId}&client_secret={options.ClientSecret}&grant_type=client_credentials");
            var res = JsonConvert.DeserializeObject<AppTokenResponse>(data);
            return res.AccessToken;
        }

        public async Task FetchAsync()
        {
            var lastEntry = await context.Data.AsQueryable().OrderByDescending(a => a.Timestamp).FirstOrDefaultAsync();
            if (lastEntry?.Timestamp != null && lastEntry.Timestamp.Add(RefreshRate) > DateTime.UtcNow) return;

            bool traverse = true;
            var foundEntries = new Stack<InstagramData>();
            while (traverse)
            {
                var data = await HttpClient.GetStringAsync(GetMediaUri(100, "id,timestamp,permalink", null));
                var response = JsonConvert.DeserializeObject<InstagramResponse>(data);
                if (response.Paging.Next == null) traverse = false; //If there's no next, no need to fetch more
                foreach (var item in response.Data)
                {
                    if (item.Timestamp <= lastEntry?.Timestamp)
                    {
                        traverse = false;
                        break;
                    }

                   var uri = await GetOembedUriAsync(item.Permalink!);
                    var embedData = await HttpClient.GetStringAsync(uri);
                    var embedResponse = JsonConvert.DeserializeObject<OembedData>(embedData, new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new SnakeCaseNamingStrategy()
                        }
                    }) ?? throw new NullReferenceException("Couldn't deserialise Oembed data");

                    var entry = new InstagramData(item.Id!, item.Timestamp!.Value, embedResponse);
                    await context.Data.InsertOneAsync(entry);
                    foundEntries.Push(entry);
                }
            }
            if (foundEntries.Count > 0) NewData?.Invoke(this, foundEntries);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Timer = new Timer(async (o) => await FetchAsync(), null, TimeSpan.Zero, RefreshRate);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Timer?.Change(Timeout.Infinite, 0);
            Timer = null;
            return Task.CompletedTask;
        }
    }
}
