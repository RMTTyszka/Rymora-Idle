namespace RymoraLandOfHeroes.Core.Content;

public interface IWeaponCatalog
{
    WeaponTemplate GetWeapon(string name);
    IReadOnlyList<WeaponTemplate> GetAllWeapons();
    WeaponTemplate GetRandomWeapon();
}

public interface IArmorCatalog
{
    ArmorTemplate GetArmor(string name);
    IReadOnlyList<ArmorTemplate> GetAllArmors();
    ArmorTemplate GetRandomArmor();
}

public interface IMonsterCatalog
{
    CreatureTemplate GetMonster(string name);
    IReadOnlyList<CreatureTemplate> GetMonstersByEncounter(string encounterId);
}

public interface IEncounterCatalog
{
    EncounterTemplate GetEncounter(string id);
    IReadOnlyList<EncounterTemplate> GetEncountersByRegion(string regionName);
}

public interface IMaterialCatalog
{
    MaterialItem GetMaterial(string name);
    IReadOnlyList<MaterialItem> GetAllMaterials();
}
