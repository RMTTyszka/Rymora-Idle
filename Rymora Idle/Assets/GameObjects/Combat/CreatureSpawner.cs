using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    public CreatureType creatureType;
    public CreatureBody creatureBodyPrefab;
    public CreatureBody Creature { get; set; }

    public void InstantiateCreature(Creature creature)
    {
        Debug.Log($"Instantianting {creature.Name}");
        Creature = Instantiate(creatureBodyPrefab, transform.position, transform.rotation, transform);
        Creature.gameObject.layer = gameObject.layer;
        Creature.SetImage(creature.Sprite);
        Creature.Creature = creature;
    }

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