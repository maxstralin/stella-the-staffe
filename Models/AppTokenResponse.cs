using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StellaTheStaffe.Models
{
    public class AppTokenResponse
    {
        [JsonConstructor]
        public AppTokenResponse(string accessToken, string tokenType)
        {
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            TokenType = tokenType ?? throw new ArgumentNullException(nameof(tokenType));
        }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get;  }
        [JsonProperty(PropertyName = "token_type")] 
        public string TokenType { get; }
    }
}
