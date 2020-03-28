using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GearlistFront.Model
{
    public class SASToken
    {
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
