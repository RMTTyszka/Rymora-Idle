using Global;
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    public Creature Creature { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.33f);
    }
}
