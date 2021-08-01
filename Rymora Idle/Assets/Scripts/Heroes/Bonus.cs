namespace Heroes
{
    public class Bonus
    {
        public int Value { get; set; }
        public BonusType Type { get; set; }
        public float StartedAt { get; set; }
        public float ExpiresAt { get; set; }
    }
}