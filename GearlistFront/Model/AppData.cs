using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearlistFront.Model
{
    public class AppData
    {
        public GearModel Model { get; set; }

        public IEnumerable<StolenItemFound> ListOfStolenItemFound { get; set; }
        public IEnumerable<GearModel> ListOfItems { get; set; }

    }
}
