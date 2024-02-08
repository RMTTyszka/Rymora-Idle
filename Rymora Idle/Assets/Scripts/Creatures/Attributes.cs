public class Attributes
{
    public AttributeInstance Strength { get; set; } = new(Attribute.Strength.ToString());
        public AttributeInstance Agility { get; set; } = new(Attribute.Agility.ToString());
    public AttributeInstance Vitality { get; set; } = new(Attribute.Vitality.ToString());
        public AttributeInstance Wisdom { get; set; } = new(Attribute.Wisdom.ToString());
    public AttributeInstance Intuition { get; set; } = new(Attribute.Intuition.ToString());
        public AttributeInstance Charisma { get; set; } = new(Attribute.Charisma.ToString());
}