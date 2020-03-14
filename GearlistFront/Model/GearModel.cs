using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearlistFront.Model
{
    public class GearModel
    {
        public string Type { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string Year { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Notes { get; set; }
    }
}
