using System.Collections.Generic;
using Global;

namespace DefaultNamespace
{
    public class Party
    {
        public Creature Hero { get; set; }
        
        public List<Creature> Members { get; set; }

        public Party()
        {
            Hero = new Creature();
            Members = new List<Creature>();
            Members.Add(Hero);
        }
    }
}