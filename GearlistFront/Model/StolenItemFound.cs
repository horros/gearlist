using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GearlistFront.Model
{
    public class StolenItemFound
    {
        public string id { get; set; }
        [JsonPropertyName("Owner")]
        public string Owner { get; set; }
        [JsonPropertyName("Serial")]
        public string Serial { get; set; }
        [JsonPropertyName("Message")]
        public string Message { get; set; }
    }
}
