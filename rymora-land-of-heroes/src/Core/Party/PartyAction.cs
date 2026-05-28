using RymoraLandOfHeroes.Core.Common;

namespace RymoraLandOfHeroes.Core.Party;

public enum PartyActionType
{
    Travel,
    Mine,
    CutWood,
    TransferItem
}

public enum PartyActionEndType
{
    ByCount,
    ByItemQuantity,
    ByTime
}

public sealed record PartyActionRequest(
    PartyActionType ActionType,
    PartyActionEndType EndType,
    float TimeToExecute,
    int? LimitCount = null,
    float? EndTime = null,
    string? ItemName = null,
    int? ItemLevel = null,
    float? ItemWeight = null,
    int? Quantity = null,
    string? TargetPartyId = null,
    TilePosition? Destination = null,
    IReadOnlyList<TilePosition>? Path = null,
    string? AutomationActionId = null);

public sealed class PartyActionState
{
    public PartyActionState(PartyActionRequest request)
    {
        Request = request;
    }

    public static PartyActionState Restore(
        PartyActionRequest request,
        float currentTime,
        float passedTime,
        int executedCount,
        bool started)
    {
        if (!float.IsFinite(currentTime) || !float.IsFinite(passedTime) || currentTime < 0 || passedTime < 0 || executedCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentTime), "Action state values cannot be negative.");
        }

        return new PartyActionState(request)
        {
            CurrentTime = currentTime,
            PassedTime = passedTime,
            ExecutedCount = executedCount,
            Started = started
        };
    }

    public PartyActionRequest Request { get; }
    public float CurrentTime { get; private set; }
    public float PassedTime { get; private set; }
    public int ExecutedCount { get; private set; }
    public bool Started { get; private set; }
    public bool IsReadyToExecute => CurrentTime >= Request.TimeToExecute;

    public void MarkStarted()
    {
        Started = true;
    }

    public void AddProgress(float deltaTime, float performance = 1)
    {
        if (deltaTime < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time cannot be negative.");
        }

        if (performance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(performance), "Performance cannot be negative.");
        }

        CurrentTime += deltaTime * performance;
        PassedTime += deltaTime;
    }

    public void MarkExecuted()
    {
        ExecutedCount++;
        CurrentTime = 0;
    }

    public bool IsComplete(int currentItemQuantity = 0)
    {
        return Request.EndType switch
        {
            PartyActionEndType.ByCount => Request.LimitCount is not null && ExecutedCount >= Request.LimitCount.Value,
            PartyActionEndType.ByItemQuantity => Request.LimitCount is not null && currentItemQuantity >= Request.LimitCount.Value,
            PartyActionEndType.ByTime => Request.EndTime is not null && PassedTime >= Request.EndTime.Value,
            _ => false
        };
    }
}

public sealed class PartyActionQueue
{
    private readonly Queue<PartyActionRequest> _pending = new();

    public PartyActionState? Current { get; private set; }
    public IReadOnlyList<PartyActionRequest> PendingRequests => _pending.ToArray();
    public int PendingCount => _pending.Count;
    public bool IsIdle => Current is null && _pending.Count == 0;

    public void Enqueue(PartyActionRequest request)
    {
        _pending.Enqueue(request);
    }

    public PartyActionState? StartNextIfIdle()
    {
        if (Current is not null || _pending.Count == 0)
        {
            return Current;
        }

        Current = new PartyActionState(_pending.Dequeue());
        Current.MarkStarted();
        return Current;
    }

    public PartyActionState? CompleteCurrentIfFinished(int currentItemQuantity = 0)
    {
        if (Current?.IsComplete(currentItemQuantity) != true)
        {
            return null;
        }

        var completed = Current;
        Current = null;
        return completed;
    }

    public void Clear()
    {
        _pending.Clear();
        Current = null;
    }

    public void Restore(PartyActionState? current, IEnumerable<PartyActionRequest> pending)
    {
        _pending.Clear();
        Current = current;
        foreach (var request in pending)
        {
            _pending.Enqueue(request);
        }
    }
}
