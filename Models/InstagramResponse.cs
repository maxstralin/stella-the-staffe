using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StellaTheStaffe.Models
{
    public class InstagramResponse
    {
        [JsonConstructor]
        public InstagramResponse(IEnumerable<MediaFields> data, Paging paging)
        {
            Data = data;
            Paging = paging;
        }

        public IEnumerable<MediaFields> Data { get; set; }
        public Paging Paging { get; set; }
    }
}
