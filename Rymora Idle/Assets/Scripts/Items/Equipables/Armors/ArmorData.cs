using System.Collections.Generic;

namespace Items.Equipables.Armors;

public static class ArmorData
{
    public static ArmorStatus ArmorStatus(this ArmorInstance armor)
    {
        return DataByCategory[armor.Category];
    }

    public static Dictionary<ArmorCategory, ArmorStatus> DataByCategory = new Dictionary<ArmorCategory, ArmorStatus>
    {
        {
            ArmorCategory.Light, new ArmorStatus
            {
                Protection = 3,
                Evasion = 10
            }
        },    
        {
            ArmorCategory.Medium, new ArmorStatus
            {
                Protection = 5,
                Evasion = 5
            }
        },    
        {
            ArmorCategory.Heavy, new ArmorStatus
            {
                Protection = 14,
                Evasion = 0
            }
        },
    };
}