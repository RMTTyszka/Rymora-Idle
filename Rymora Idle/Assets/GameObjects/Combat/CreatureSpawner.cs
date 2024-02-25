using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    public CreatureType creatureType;
    public CreatureBody creatureBodyPrefab;
    public CharCanvas combatantUIElementsPrefab;
    public CreatureBody CreatureBody { get; set; }

    public void InstantiateCreature(Creature creature)
    {
        Debug.Log($"Instantianting {creature.Name}");
        CreatureBody = Instantiate(creatureBodyPrefab, transform.position, transform.rotation, transform);
        CreatureBody.gameObject.layer = gameObject.layer;
        CreatureBody.SetImage(creature.Sprite);
        CreatureBody.Creature = creature;
        CreatureBody.CombatantUIElements =
            Instantiate(combatantUIElementsPrefab, transform.position, transform.rotation, transform);
        CreatureBody.CombatantUIElements.SetCreature(CreatureBody.Creature);
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

    public void Clear()
    {
        Destroy(CreatureBody.gameObject);
    }
}