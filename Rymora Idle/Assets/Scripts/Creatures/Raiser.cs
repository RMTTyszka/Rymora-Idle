using UnityEngine;

public abstract class Raiser : Roller
{
    protected Raiser(string name, int points, int modifier, float difficult) : base(name, points, modifier)
    {
        Difficult = difficult;
    }

    public float Difficult { get; set; }
    public int Count { get; set; } = 0;
    
    public  int Roll(int lvl) {
        //Debug.Log("Raiser roll called");
        RollForRaise(lvl);
        return base.Roll();

    }

    public int GetMod (int lvl)
    {
        //Debug.Log("caguei");
        RollForRaise(lvl);
        return GetValue();


    }
    private void RollForRaise(int lvl) {
        if ( lvl >= Points/5) {
            float chance = 1000f/((Points/5f)*Difficult);
            float roll = Random.Range(0f, 10001f);
            //Debug.Log(roll + " " + chance);
            if (roll <= chance) {
                //value++;
               // owner. UpdateStats();
                Debug.Log(Name + " aumentou, agora é " + Points);
                //chance = (1000f/((value/5f)*difficult))/10000f*100;
                //Debug.Log(chance);
            }
        }
    }
}