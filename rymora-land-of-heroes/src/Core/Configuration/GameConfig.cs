namespace RymoraLandOfHeroes.Core.Configuration;

public sealed record GameConfig(
    float EncounterProbability,
    float EncounterInterval,
    EncounterPolicy EncounterPolicy,
    float CorpseDecayTime,
    ProgressionConfig Progression,
    LifeConfig Life,
    CollectionConfig Collection,
    TravelConfig Travel,
    CombatConfig Combat,
    SaveConfig Save);

public sealed record ProgressionConfig(
    float InitialAttributePoints,
    float InitialSkillPoints,
    float AttributeValueDivisor,
    float SkillValueDivisor);

public sealed record LifeConfig(float BaseLife, float VitalityLifeMultiplier);

public enum EncounterModifierMode
{
    None,
    AddToProbability,
    SubtractFromProbability
}

public sealed record EncounterPolicy(EncounterModifierMode TerrainModifierMode);

public sealed record CollectionConfig(
    float DifficultyBase,
    float DifficultyPerMaterialLevel,
    float MiningActionTime,
    float WoodcuttingActionTime);

public sealed record TravelConfig(float ActionTime);

public sealed record SaveConfig(float AutoSaveIntervalSeconds);

public sealed record CombatConfig(
    RollRange HitRollRange,
    RollRange EvadeRollRange,
    float BaseCriticalMultiplier,
    TargetingConfig Targeting);

public sealed record RollRange(int Min, int Max)
{
    public void Validate()
    {
        if (Min > Max)
        {
            throw new ArgumentOutOfRangeException(nameof(Min), "Roll range min cannot be greater than max.");
        }
    }
}

public sealed record TargetingConfig(float LowLifeWeight, float ThreatWeight);
