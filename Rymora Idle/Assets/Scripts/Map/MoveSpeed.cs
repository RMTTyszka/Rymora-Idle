using System;

namespace Map
{
    [Serializable]
    public enum MoveSpeed
    {
        Forest = 70,
        Plain = 100,
        Mountain = 30,
        Desert = 50,
        Jungle = 60,
        Road = 110,
        PavedRoad = 120,
        Swamp = 40,
        Snow = 50,
        Water = 20,
        Ice = 60
    }
}