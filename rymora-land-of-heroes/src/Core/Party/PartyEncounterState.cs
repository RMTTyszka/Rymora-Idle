namespace RymoraLandOfHeroes.Core.Party;

public sealed class PartyEncounterState
{
    public float TravelSecondsSinceReset { get; private set; }

    public void AddTravelTime(float deltaTime)
    {
        if (!float.IsFinite(deltaTime) || deltaTime < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "Travel time must be finite and non-negative.");
        }

        TravelSecondsSinceReset += deltaTime;
    }

    public void Reset()
    {
        TravelSecondsSinceReset = 0;
    }

    public void Restore(float travelSecondsSinceReset)
    {
        if (!float.IsFinite(travelSecondsSinceReset) || travelSecondsSinceReset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(travelSecondsSinceReset), "Encounter travel time must be finite and non-negative.");
        }

        TravelSecondsSinceReset = travelSecondsSinceReset;
    }
}
