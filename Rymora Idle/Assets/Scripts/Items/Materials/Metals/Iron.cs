using UnityEditor.PackageManager;

namespace Items.Metals
{
    public class Iron : Metal
    {
        public Iron()
        {
            Level = 1;
            Name = nameof(Iron);
            Weight = 3;
        }
    }
}