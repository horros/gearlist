using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AzFunctions.Model
{
    public class GearModel
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }
        [JsonPropertyName("GearId")]
        public Guid GearId { get; set; }
        [JsonPropertyName("Type")]
        public string Type { get; set; }
        [JsonPropertyName("Manufacturer")]
        public string Manufacturer { get; set; }
        [JsonPropertyName("Model")]
        public string Model { get; set; }
        [JsonPropertyName("Serial")]
        public string Serial { get; set; }
        [JsonPropertyName("Year")]
        public string Year { get; set; }
        [JsonPropertyName("PurchaseDate")]
        public DateTime? PurchaseDate { get; set; }
        [JsonPropertyName("Notes")]
        public string Notes { get; set; }
        [JsonPropertyName("Owner")]
        public string Owner { get; set; }
        [JsonPropertyName("Images")]
        public List<string> Images { get; set; }
        public bool Stolen { get; set; }
    }
}
