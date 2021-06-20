using System.Collections.Generic;
using Heroes;

namespace Global
{
    public static class HeroesManager
    {
        public static List<Hero> Heroes { get; set; }
        public static void InstantiateHeroes()
        {
            var heroes = new List<Hero>();
            heroes.Add(new Hero
            {
                Name = "Benbel",
                Level = 1
            });     
            heroes.Add(new Hero
            {
                Name = "Marroghar",
                Level = 1
            });      
            heroes.Add(new Hero
            {
                Name = "Niera",
                Level = 1
            });
            Heroes = heroes;
        }
    }
}