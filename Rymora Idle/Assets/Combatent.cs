using Global;
using UnityEngine;

public class Combatent : MonoBehaviour
{
    public Creature Creature { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        Creature = GetComponent<Creature>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
