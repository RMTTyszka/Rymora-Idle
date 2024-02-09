using UnityEngine;

public class CreatureBody : MonoBehaviour
{
    public Creature Creature { get; set; }
    public SpriteRenderer SpriteRenderer { get; set; }
    // Start is called before the first frame update
    void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetImage(Sprite sprite)
    {
        if (sprite is not null)
        {
            SpriteRenderer.sprite = sprite;
        }
    }
}
