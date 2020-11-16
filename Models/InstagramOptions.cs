using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StellaTheStaffe.Models
{
    public class InstagramOptions
    {
        public InstagramOptions(string accessToken, string clientId, string clientSecret)
        {
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            ClientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
        }

        public string AccessToken { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }

        
    }
}
