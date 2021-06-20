using System.Collections.Generic;
using Areas.Realms.Cities;
using Areas.Realms.Places;

namespace Areas.Realms
{
    public class Realm
    {
        public string Name { get; set; }
        public List<Mine> Mines { get; set; }
        public List<City> Cities { get; set; }
    }
}