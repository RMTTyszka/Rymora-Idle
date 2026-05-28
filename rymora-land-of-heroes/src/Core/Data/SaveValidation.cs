using RymoraLandOfHeroes.Core.Application;

namespace RymoraLandOfHeroes.Core.Data;

public static class SaveValidation
{
    public static void Validate(SaveData save)
    {
        if (save.SaveVersion != SaveData.CurrentVersion)
        {
            throw new InvalidOperationException($"Unsupported save version: {save.SaveVersion}.");
        }

        if (!Enum.TryParse<Screen>(save.CurrentScreen, ignoreCase: false, out _))
        {
            throw new InvalidOperationException($"CurrentScreen is invalid in save: {save.CurrentScreen}.");
        }

        var activeCombatPartyIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var combat in save.ActiveCombats)
        {
            if (!activeCombatPartyIds.Add(combat.PartyId))
            {
                throw new InvalidOperationException($"Duplicate active combat for party: {combat.PartyId}.");
            }
        }

        var partyIds = new HashSet<string>(StringComparer.Ordinal);
        var partiesById = new Dictionary<string, PartySaveData>(StringComparer.Ordinal);
        foreach (var party in save.Parties)
        {
            if (!partyIds.Add(party.PartyId))
            {
                throw new InvalidOperationException($"Duplicate PartyId in save: {party.PartyId}.");
            }

            partiesById.Add(party.PartyId, party);

            ValidateQueue(party.PartyId, party.ActionQueue);
            ValidateAutomation(party.PartyId, party.Automation);
            ValidateInventory(party.PartyId, party.InventoryItems);
            if (party.IsInCombat && !activeCombatPartyIds.Contains(party.PartyId))
            {
                throw new InvalidOperationException($"Party {party.PartyId} is marked in combat but missing active combat.");
            }
        }

        if (save.SelectedPartyId is not null && !partyIds.Contains(save.SelectedPartyId))
        {
            throw new InvalidOperationException($"SelectedPartyId not found in save: {save.SelectedPartyId}.");
        }

        foreach (var combat in save.ActiveCombats)
        {
            if (!partiesById.TryGetValue(combat.PartyId, out var party))
            {
                throw new InvalidOperationException($"ActiveCombat.PartyId not found in save: {combat.PartyId}.");
            }

            if (!party.IsInCombat)
            {
                throw new InvalidOperationException($"Active combat party is not marked in combat: {combat.PartyId}.");
            }
        }
    }

    private static void ValidateQueue(string partyId, ActionQueueSaveData queue)
    {
        if (queue.Current is not null)
        {
            ValidateState(partyId, queue.Current);
        }

        foreach (var pending in queue.Pending)
        {
            ValidateRequest(partyId, pending);
        }
    }

    private static void ValidateRequest(string partyId, PartyActionRequestSaveData request)
    {
        if (!float.IsFinite(request.TimeToExecute) || request.TimeToExecute < 0)
        {
            throw new InvalidOperationException($"Party {partyId} action TimeToExecute cannot be negative.");
        }

        if (request.EndTime is not null && (!float.IsFinite(request.EndTime.Value) || request.EndTime.Value < 0))
        {
            throw new InvalidOperationException($"Party {partyId} action EndTime must be finite and non-negative.");
        }

        if (request.ItemWeight is not null && (!float.IsFinite(request.ItemWeight.Value) || request.ItemWeight.Value < 0))
        {
            throw new InvalidOperationException($"Party {partyId} action ItemWeight must be finite and non-negative.");
        }

        if (request.ActionType == "Travel" && request.Destination is null)
        {
            throw new InvalidOperationException($"Party {partyId} Travel action missing Destination.");
        }

        if ((request.ActionType == "Mine" || request.ActionType == "CutWood")
            && (string.IsNullOrWhiteSpace(request.ItemName) || request.ItemLevel is null || request.ItemWeight is null))
        {
            throw new InvalidOperationException($"Party {partyId} collection action missing item fields.");
        }

        if (request.ActionType == "TransferItem"
            && (string.IsNullOrWhiteSpace(request.TargetPartyId)
                || string.IsNullOrWhiteSpace(request.ItemName)
                || request.ItemLevel is null
                || request.Quantity is null))
        {
            throw new InvalidOperationException($"Party {partyId} TransferItem action missing transfer fields.");
        }
    }

    private static void ValidateState(string partyId, PartyActionStateSaveData state)
    {
        if (!float.IsFinite(state.CurrentTime) || !float.IsFinite(state.PassedTime) || state.CurrentTime < 0 || state.PassedTime < 0 || state.ExecutedCount < 0)
        {
            throw new InvalidOperationException($"Party {partyId} action state has invalid progress values.");
        }

        ValidateRequest(partyId, state.Request);
    }

    private static void ValidateAutomation(string partyId, AutomationSaveData automation)
    {
        var macroIds = new HashSet<string>(automation.Macros.Select(macro => macro.Id), StringComparer.Ordinal);
        foreach (var step in automation.Program.Steps)
        {
            if (!macroIds.Contains(step.MacroId))
            {
                throw new InvalidOperationException($"Party {partyId} Program step references missing Macro: {step.MacroId}.");
            }
        }
    }

    private static void ValidateInventory(string partyId, IReadOnlyList<ItemSaveData> items)
    {
        foreach (var item in items)
        {
            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException($"Party {partyId} inventory item has invalid quantity: {item.Name}.");
            }
        }
    }
}
