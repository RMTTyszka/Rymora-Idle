namespace RymoraLandOfHeroes.Core.Application;

public sealed class UIState
{
    public Screen CurrentScreen { get; internal set; } = Screen.Map;
    public string? SelectedPartyId { get; internal set; }
}
