using UnityEngine;

namespace Heroes
{
    public class Hero : MonoBehaviour
    {
        public new string Name { get; set; }
        public int Level{ get; set; }
    
        public Inventory Inventory { get; set; }

        public Hero()
        {
            Inventory = new Inventory();
        }
    }
}
