using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GearlistFront.Model
{
    public class GearModel
    {
        [Required]
        public string Type { get; set; }
        [Required]
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string Year { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Notes { get; set; }
    }
}
