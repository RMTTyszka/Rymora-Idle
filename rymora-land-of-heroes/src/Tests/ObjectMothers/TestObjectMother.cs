using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;
using RymoraLandOfHeroes.Core.Content;
using RymoraLandOfHeroes.Core.Hero;
using RymoraLandOfHeroes.Core.World;

namespace RymoraLandOfHeroes.Core.Tests.ObjectMothers;

internal static class TestObjectMother
{
    public static Creature CreateCreature(string name, LifeConfig? lifeConfig = null)
    {
        var progression = new ProgressionConfig(5, 5, 5, 5);
        var attributes = StatBlock<AttributeType>.Create(progression.InitialAttributePoints, progression.AttributeValueDivisor);
        var skills = StatBlock<SkillType>.Create(progression.InitialSkillPoints, progression.SkillValueDivisor);
        var properties = StatBlock<PropertyType>.Create(0, 1);
        var equipment = new Equipment
        {
            MainHand = CreateWeapon("Training Sword", attackSpeed: 1)
        };

        return new Creature(name, attributes, skills, properties, equipment, lifeConfig ?? new LifeConfig(100, 10));
    }

    public static WeaponTemplate CreateWeapon(string name, float attackSpeed, float baseDamage = 1)
    {
        return new WeaponTemplate(
            name,
            Level: 1,
            Size: WeaponSize.Light,
            DamageCategory: WeaponDamageCategory.Cutting,
            AttackSpeed: attackSpeed,
            BaseDamage: baseDamage,
            SizeMultiplier: 1,
            HitModifier: 0,
            Penetration: 0,
            CounterPotential: 0);
    }

    public static Creature CreateMonster(CreatureTemplate template)
    {
        return CreateCreature(template.CreatureName, new LifeConfig(1, 0));
    }

    public static GameConfig CreateGameConfig(float encounterProbability = 0)
    {
        return new GameConfig(
            EncounterProbability: encounterProbability,
            EncounterInterval: 1,
            EncounterPolicy: new EncounterPolicy(EncounterModifierMode.None),
            CorpseDecayTime: 60,
            Progression: new ProgressionConfig(5, 5, 5, 5),
            Life: new LifeConfig(100, 10),
            Collection: new CollectionConfig(
                DifficultyBase: 0,
                DifficultyPerMaterialLevel: 0,
                MiningActionTime: 1,
                WoodcuttingActionTime: 1),
            Travel: new TravelConfig(ActionTime: 1),
            Combat: new CombatConfig(
                HitRollRange: new RollRange(1, 1),
                EvadeRollRange: new RollRange(1, 1),
                BaseCriticalMultiplier: 1.5f,
                Targeting: new TargetingConfig(LowLifeWeight: 100, ThreatWeight: 1)),
            Save: new SaveConfig(AutoSaveIntervalSeconds: 30));
    }

    public static WorldState CreateWorld(
        IRandomSource random,
        IReadOnlyCollection<TilePosition>? wallPositions = null,
        IReadOnlyCollection<TilePosition>? minePositions = null,
        IReadOnlyCollection<TilePosition>? forestPositions = null,
        float regionEncounterModifier = 0,
        float zoneEncounterModifier = 0)
    {
        var wallSet = wallPositions?.ToHashSet() ?? new HashSet<TilePosition>();
        var mineSet = minePositions?.ToHashSet() ?? new HashSet<TilePosition>();
        var forestSet = forestPositions?.ToHashSet() ?? new HashSet<TilePosition>();
        var encounters = new[]
        {
            new EncounterTemplate(
                "plain-encounter",
                "Plain Encounter",
                Level: 1,
                new[] { new CreatureTemplate("Goblin", MonsterClass.Combatant, new SpriteReference("goblin")) })
        };
        var encountersByTerrain = new Dictionary<TerrainType, IReadOnlyList<EncounterTemplate>>
        {
            [TerrainType.Plain] = encounters,
            [TerrainType.Mine] = encounters,
            [TerrainType.Forest] = encounters
        };
        var wild = new RegionData(
            "wild",
            "Wild",
            IsSafeSpot: false,
            EncounterProbabilityModifier: regionEncounterModifier,
            EncountersByTerrain: encountersByTerrain);
        var safe = new RegionData(
            "safe",
            "Safe",
            IsSafeSpot: true,
            EncounterProbabilityModifier: 0,
            EncountersByTerrain: new Dictionary<TerrainType, IReadOnlyList<EncounterTemplate>>());
        var zone = new ZoneData("edge", "Edge", Level: 1, EncounterProbabilityModifier: zoneEncounterModifier);
        var tiles = new List<WorldTile>();

        for (var x = 0; x <= 3; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                var position = new TilePosition(x, y);
                var isWall = wallSet.Contains(position);
                var isMine = mineSet.Contains(position);
                var isForest = forestSet.Contains(position);
                var isSafe = position == new TilePosition(3, 0);
                var terrain = isWall
                    ? CreateTerrain(TerrainType.Wall, isWalkable: false, encounterModifier: 0, isCity: false)
                    : CreateTerrain(
                        isSafe ? TerrainType.City : isMine ? TerrainType.Mine : isForest ? TerrainType.Forest : TerrainType.Plain,
                        isWalkable: true,
                        encounterModifier: 15,
                        isCity: isSafe,
                        allowsMining: isMine,
                        allowsWoodcutting: isForest);

                tiles.Add(new WorldTile(position, terrain, isSafe ? safe : wild, zone));
            }
        }

        return new WorldState(tiles, random);
    }

    private static TerrainData CreateTerrain(
        TerrainType type,
        bool isWalkable,
        int encounterModifier,
        bool isCity,
        bool allowsMining = false,
        bool allowsWoodcutting = false)
    {
        return new TerrainData(
            type,
            isWalkable,
            MoveSpeed: 100,
            Quality: 1,
            EncounterRateModifier: encounterModifier,
            AllowsMining: allowsMining,
            AllowsWoodcutting: allowsWoodcutting,
            IsCity: isCity,
            IsPlace: isCity);
    }
}
