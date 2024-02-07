namespace Heroes
{
    public class Skills
    {
        public Skill Mine { get; set; }
        public Skill Lumberjack { get; set; }

        public Skills()
        {
            Mine = new Skill();
            Lumberjack = new Skill();
        }




        public static decimal MineTime = 3;
        public static decimal CutWoodTime = 2;
    }
}