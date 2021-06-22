using System;
using System.Collections.Generic;
using Areas.Realms.Cities;
using Areas.Realms.Places;
using UnityEngine.Serialization;

namespace Areas.Realms
{
    [Serializable]
    public class Realm : Visitable
    {
        public List<Mine> Mines { get; set; }
        public List<City> Cities { get; set; }
    }
}