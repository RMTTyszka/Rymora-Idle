using System;
using Godot;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Combat;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Content;
using RymoraLandOfHeroes.Core.Party;
using RymoraLandOfHeroes.Core.World;
using RymoraLandOfHeroes.GodotAdapter.Content;
using RymoraLandOfHeroes.GodotAdapter.Presentation;
using RymoraLandOfHeroes.GodotAdapter.World;

namespace RymoraLandOfHeroes.GodotAdapter.Bootstrap;

public partial class Bootstrap : Node2D
{
    private enum ContextAction
    {
        Move = 1,
        Mine = 2,
        CutWood = 3
    }

    private GameApplication? _application;
    private WorldTileMapAdapter? _worldAdapter;
    private PartyPresenter? _partyPresenter;
    private CombatPresenter? _combatPresenter;
    private HudPresenter? _hudPresenter;
    private MacrosPresenter? _macrosPresenter;
    private Button? _macrosButton;
    private PopupMenu? _contextMenu;
    private GameContent? _content;
    private MaterialItem? _startupMineMaterial;
    private TilePosition _contextMenuTarget;
    private CombatInstance? _observedCombat;
    private bool _miningLogged;
    private bool _wasInCombat;
    private float _elapsed;

    public override void _Ready()
    {
        var terrainLayer = GetNode<TileMapLayer>("TerrainLayer");
        var regionLayer = GetNode<TileMapLayer>("RegionLayer");
        var zoneLayer = GetNode<TileMapLayer>("ZoneLayer");
        var demoMapBuilder = GetNode<DemoTileMapBuilder>("DemoTileMapBuilder");
        _worldAdapter = GetNode<WorldTileMapAdapter>("WorldTileMapAdapter");
        _partyPresenter = GetNode<PartyPresenter>("PartyPresenter");
        _combatPresenter = GetNode<CombatPresenter>("CombatLayer/CombatPresenter");
        _hudPresenter = GetNode<HudPresenter>("UiLayer/Hud");
        _macrosPresenter = GetNodeOrNull<MacrosPresenter>("UiLayer/MacrosModal");
        _macrosButton = GetNodeOrNull<Button>("UiLayer/MenuRail/Rows/MacrosButton");
        _contextMenu = GetNode<PopupMenu>("UiLayer/ContextMenu");
        _contextMenu.IdPressed += OnContextMenuIdPressed;
        if (_macrosButton is not null)
        {
            _macrosButton.Pressed += OpenMacrosModal;
        }

        if (_macrosPresenter is not null)
        {
            _macrosPresenter.Closed += OnMacrosModalClosed;
        }

        _content = JsonGameContentLoader.LoadDefault();

        regionLayer.Visible = false;
        zoneLayer.Visible = false;

        demoMapBuilder.Configure(_content.TerrainTiles, _content.Regions, _content.Zones);
        demoMapBuilder.TerrainLayer = terrainLayer;
        demoMapBuilder.RegionLayer = regionLayer;
        demoMapBuilder.ZoneLayer = zoneLayer;
        demoMapBuilder.BuildIfEmpty();

        _worldAdapter.Configure(_content.TerrainTiles, _content.Regions, _content.Zones);
        _worldAdapter.TerrainLayer = terrainLayer;
        _worldAdapter.RegionLayer = regionLayer;
        _worldAdapter.ZoneLayer = zoneLayer;
        var world = _worldAdapter.CreateWorld();
        var startPosition = _worldAdapter.FindFirstPosition(TerrainType.Mine);
        _startupMineMaterial = _worldAdapter.GetMaterialForAction(
            startPosition,
            PartyActionType.Mine,
            _content.Materials);
        if (_startupMineMaterial is null)
        {
            throw new InvalidOperationException("Starting mine tile has no mining material.");
        }

        _application = BootstrapCoreFactory.CreateApplication(world, startPosition, _content);
        _application.SelectParty(BootstrapCoreFactory.PartyId);

        var queued = _application.EnqueueAction(
            BootstrapCoreFactory.PartyId,
            new PartyActionRequest(
                PartyActionType.Mine,
                PartyActionEndType.ByCount,
                _application.Config.Collection.MiningActionTime,
                LimitCount: 1,
                ItemName: _startupMineMaterial.Name,
                ItemLevel: _startupMineMaterial.Level,
                ItemWeight: _startupMineMaterial.Weight));

        _partyPresenter.Sync(_application.Parties.Get(BootstrapCoreFactory.PartyId), _worldAdapter);
        _macrosPresenter?.Bind(_application, _application.Parties.Get(BootstrapCoreFactory.PartyId));

        GD.Print($"Rymora Godot bootstrap ready. World from TileMapLayer. Mine queued: {queued}.");
    }

    public override void _Process(double delta)
    {
        if (_application is null || _worldAdapter is null || _partyPresenter is null || _combatPresenter is null || _hudPresenter is null)
        {
            return;
        }

        _elapsed += (float)delta;
        _application.Update((float)delta);

        var party = _application.Parties.Get(BootstrapCoreFactory.PartyId);
        _partyPresenter.Sync(party, _worldAdapter);
        _hudPresenter.Sync(_application, party);
        _macrosPresenter?.Bind(_application, party);
        SyncCombatOverlay(party.IsInCombat);

        if (_startupMineMaterial is null)
        {
            return;
        }

        var quantity = party.Inventory.GetQuantity(_startupMineMaterial.Name, _startupMineMaterial.Level);
        if (_miningLogged || quantity <= 0)
        {
            return;
        }

        _miningLogged = true;
        GD.Print($"Rymora Core loop OK. {_startupMineMaterial.Name}={quantity}. Elapsed={_elapsed:0.00}s.");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_application is null || _worldAdapter is null || @event is not InputEventMouseButton mouseButton)
        {
            return;
        }

        if (!mouseButton.Pressed)
        {
            return;
        }

        if (_application.Parties.Get(BootstrapCoreFactory.PartyId).IsInCombat)
        {
            return;
        }

        if (mouseButton.ButtonIndex == MouseButton.Right)
        {
            ShowContextMenu(mouseButton);
            return;
        }

        if (mouseButton.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        if (_application.Parties.Get(BootstrapCoreFactory.PartyId).Automation.Recording is not null)
        {
            return;
        }

        EnqueueTravel(_worldAdapter.ToTilePosition(GetGlobalMousePosition()), clearQueue: true);
    }

    private void ShowContextMenu(InputEventMouseButton mouseButton)
    {
        if (_application is null || _worldAdapter is null || _contextMenu is null)
        {
            return;
        }

        _contextMenuTarget = _worldAdapter.ToTilePosition(GetGlobalMousePosition());
        _contextMenu.Clear();

        if (_worldAdapter.HasTile(_contextMenuTarget))
        {
            var terrain = _application.World.GetTerrain(_contextMenuTarget);
            if (terrain.IsWalkable)
            {
                _contextMenu.AddItem("Move", (int)ContextAction.Move);
            }

            if (terrain.AllowsMining)
            {
                _contextMenu.AddItem("Mine", (int)ContextAction.Mine);
            }

            if (terrain.AllowsWoodcutting)
            {
                _contextMenu.AddItem("Cut Wood", (int)ContextAction.CutWood);
            }
        }

        _contextMenu.Position = new Vector2I((int)mouseButton.Position.X, (int)mouseButton.Position.Y);
        _contextMenu.Popup();
    }

    private void OpenMacrosModal()
    {
        _hudPresenter?.Hide();
        _macrosPresenter?.Show();
    }

    private void OnMacrosModalClosed()
    {
        _hudPresenter?.Show();
    }

    private void OnContextMenuIdPressed(long id)
    {
        var action = (ContextAction)id;
        if (_application?.Parties.Get(BootstrapCoreFactory.PartyId).Automation.Recording is not null)
        {
            RecordContextAction(action);
            return;
        }

        switch (action)
        {
            case ContextAction.Move:
                EnqueueTravel(_contextMenuTarget, clearQueue: true);
                break;
            case ContextAction.Mine:
                EnqueueGather(_contextMenuTarget, PartyActionType.Mine);
                break;
            case ContextAction.CutWood:
                EnqueueGather(_contextMenuTarget, PartyActionType.CutWood);
                break;
        }
    }

    private void RecordContextAction(ContextAction action)
    {
        if (_application is null || _worldAdapter is null || _content is null)
        {
            return;
        }

        switch (action)
        {
            case ContextAction.Move:
                _application.RecordMoveAction(BootstrapCoreFactory.PartyId, _contextMenuTarget);
                GD.Print($"Recorded MoveTo ({_contextMenuTarget.X}, {_contextMenuTarget.Y}).");
                break;
            case ContextAction.Mine:
                RecordGatherAction(MacroActionKind.Mine, PartyActionType.Mine);
                break;
            case ContextAction.CutWood:
                RecordGatherAction(MacroActionKind.CutWood, PartyActionType.CutWood);
                break;
        }
    }

    private void RecordGatherAction(MacroActionKind macroKind, PartyActionType partyActionType)
    {
        if (_application is null || _worldAdapter is null || _content is null)
        {
            return;
        }

        var material = _worldAdapter.GetMaterialForAction(_contextMenuTarget, partyActionType, _content.Materials);
        if (material is null)
        {
            GD.Print($"Cannot record {macroKind}. Tile has no material.");
            return;
        }

        _application.RecordGatherAction(
            BootstrapCoreFactory.PartyId,
            _contextMenuTarget,
            macroKind,
            material.Name,
            material.Level,
            material.Weight);
        GD.Print($"Recorded {macroKind} at ({_contextMenuTarget.X}, {_contextMenuTarget.Y}).");
    }

    private bool EnqueueTravel(TilePosition destination, bool clearQueue)
    {
        if (_application is null)
        {
            return false;
        }

        var party = _application.Parties.Get(BootstrapCoreFactory.PartyId);
        var isWalkable = _application.World.IsWalkable(destination);
        var path = _application.World.FindPath(party.Position, destination);
        if (!isWalkable || (path.Count == 0 && party.Position != destination))
        {
            GD.Print($"Travel rejected. Current=({party.Position.X}, {party.Position.Y}). Destination=({destination.X}, {destination.Y}). Walkable={isWalkable}. Path={path.Count}.");
            return false;
        }

        if (clearQueue)
        {
            _application.ClearActionQueueForManualAction(BootstrapCoreFactory.PartyId);
        }

        var queued = _application.EnqueueAction(
            BootstrapCoreFactory.PartyId,
            new PartyActionRequest(
                PartyActionType.Travel,
                PartyActionEndType.ByCount,
                _application.Config.Travel.ActionTime,
                Destination: destination));

        var status = queued ? "queued" : "rejected";
        GD.Print($"Travel {status}. Current=({party.Position.X}, {party.Position.Y}). Destination=({destination.X}, {destination.Y}). Path={path.Count}.");
        return queued;
    }

    private void EnqueueGather(
        TilePosition target,
        PartyActionType actionType)
    {
        if (_application is null || _worldAdapter is null || _content is null || !_worldAdapter.HasTile(target))
        {
            return;
        }

        var terrain = _application.World.GetTerrain(target);
        var canGather = actionType == PartyActionType.Mine ? terrain.AllowsMining : terrain.AllowsWoodcutting;
        if (!canGather)
        {
            GD.Print($"{actionType} rejected. Tile=({target.X}, {target.Y}).");
            return;
        }

        var material = _worldAdapter.GetMaterialForAction(target, actionType, _content.Materials);
        if (material is null)
        {
            GD.Print($"{actionType} rejected. Tile has no material. Tile=({target.X}, {target.Y}).");
            return;
        }

        var party = _application.Parties.Get(BootstrapCoreFactory.PartyId);
        var path = _application.World.FindPath(party.Position, target);
        if (party.Position != target && path.Count == 0)
        {
            GD.Print($"{actionType} rejected. No path to ({target.X}, {target.Y}).");
            return;
        }

        _application.ClearActionQueueForManualAction(BootstrapCoreFactory.PartyId);
        if (party.Position != target && !EnqueueTravel(target, clearQueue: false))
        {
            return;
        }

        var queued = _application.EnqueueAction(
            BootstrapCoreFactory.PartyId,
            new PartyActionRequest(
                actionType,
                PartyActionEndType.ByCount,
                GetGatherActionTime(actionType),
                LimitCount: 1,
                ItemName: material.Name,
                ItemLevel: material.Level,
                ItemWeight: material.Weight));

        var status = queued ? "queued" : "rejected";
        GD.Print($"{actionType} {status}. Target=({target.X}, {target.Y}). Item={material.Name}.");
    }

    private float GetGatherActionTime(PartyActionType actionType)
    {
        return actionType switch
        {
            PartyActionType.Mine => _application!.Config.Collection.MiningActionTime,
            PartyActionType.CutWood => _application!.Config.Collection.WoodcuttingActionTime,
            _ => throw new ArgumentOutOfRangeException(nameof(actionType), "Unknown gather action type.")
        };
    }

    private void SyncCombatOverlay(bool isInCombat)
    {
        if (_application is null || _combatPresenter is null)
        {
            return;
        }

        if (isInCombat && _application.ActiveCombats.TryGetValue(BootstrapCoreFactory.PartyId, out var combat))
        {
            ObserveCombat(combat);
            _combatPresenter.Sync(combat);
            _wasInCombat = true;
            return;
        }

        _combatPresenter.Sync(null);
        StopObservingCombat();
        if (_wasInCombat)
        {
            _wasInCombat = false;
            GD.Print("Combat finished. Returned to map.");
        }
    }

    private void ObserveCombat(CombatInstance combat)
    {
        if (ReferenceEquals(_observedCombat, combat))
        {
            return;
        }

        StopObservingCombat();
        _observedCombat = combat;
        _observedCombat.OnEvent += OnCombatEvent;
        _combatPresenter?.ClearHistory();
        GD.Print("Encounter started. Combat screen active.");
    }

    private void StopObservingCombat()
    {
        if (_observedCombat is null)
        {
            return;
        }

        _observedCombat.OnEvent -= OnCombatEvent;
        _observedCombat = null;
    }

    private void OnCombatEvent(CombatEvent combatEvent)
    {
        _combatPresenter?.AddEvent(combatEvent);
        GD.Print($"Combat {combatEvent.Type}: {combatEvent.Source.Creature.Name} -> {combatEvent.Target.Creature.Name}, dmg={combatEvent.Damage:0.0}.");
    }
}
