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




        public static int MineTime = 3;
        public static int CutWoodTime = 2;
    }
}