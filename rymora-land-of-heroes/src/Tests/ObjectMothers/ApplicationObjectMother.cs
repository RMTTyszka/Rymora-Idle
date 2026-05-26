using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;
using RymoraLandOfHeroes.Core.Party;
using GameParty = RymoraLandOfHeroes.Core.Party.Party;

namespace RymoraLandOfHeroes.Core.Tests.ObjectMothers;

internal static class ApplicationObjectMother
{
    public static MiningUpdateScenario ApplicationWithMineActionQueued()
    {
        var party = new GameParty("party-1", new TilePosition(0, 0));
        party.AddMember(TestObjectMother.CreateCreature("Miner"));
        var application = new GameApplication(
            TestObjectMother.CreateWorld(
                random: new SequenceRandomSource(1),
                minePositions: new[] { new TilePosition(0, 0) }),
            new[] { party },
            TestObjectMother.CreateGameConfig(),
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));
        application.EnqueueAction("party-1", new PartyActionRequest(
            PartyActionType.Mine,
            PartyActionEndType.ByCount,
            TimeToExecute: 1,
            LimitCount: 1,
            ItemName: "Iron",
            ItemLevel: 1,
            ItemWeight: 3));

        return new MiningUpdateScenario(
            InputApplication: application,
            InputDeltaTime: 1,
            AssertParty: party,
            AssertItemName: "Iron",
            AssertItemLevel: 1,
            ExpectedQuantity: 1);
    }

    public static TransferUpdateScenario ApplicationWithTransferActionQueued()
    {
        var source = new GameParty("source", new TilePosition(0, 0));
        var target = new GameParty("target", new TilePosition(0, 0));
        source.Inventory.AddItem(new Item("Iron", 1, 3, 2));
        var application = new GameApplication(
            TestObjectMother.CreateWorld(random: new SequenceRandomSource(1)),
            new[] { source, target },
            TestObjectMother.CreateGameConfig(),
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));
        application.EnqueueAction("source", new PartyActionRequest(
            PartyActionType.TransferItem,
            PartyActionEndType.ByCount,
            TimeToExecute: 1,
            LimitCount: 1,
            ItemName: "Iron",
            ItemLevel: 1,
            Quantity: 2,
            TargetPartyId: "target"));

        return new TransferUpdateScenario(
            InputApplication: application,
            InputDeltaTime: 1,
            AssertTargetParty: target,
            AssertItemName: "Iron",
            AssertItemLevel: 1,
            ExpectedQuantity: 2);
    }

    public static TravelUpdateScenario ApplicationWithTravelActionQueued(float encounterProbability = 0)
    {
        var party = new GameParty("party-1", new TilePosition(0, 0));
        var hero = TestObjectMother.CreateCreature("Hero");
        hero.Equipment.MainHand = TestObjectMother.CreateWeapon("Greatsword", attackSpeed: 1, baseDamage: 100);
        party.AddMember(hero);
        var application = new GameApplication(
            TestObjectMother.CreateWorld(random: new SequenceRandomSource(1, 0)),
            new[] { party },
            TestObjectMother.CreateGameConfig(encounterProbability),
            _ => TestObjectMother.CreateCreature("Goblin", new LifeConfig(1, 0)),
            new SequenceRandomSource(1, 1));
        application.SelectParty("party-1");
        application.EnqueueAction("party-1", new PartyActionRequest(
            PartyActionType.Travel,
            PartyActionEndType.ByCount,
            TimeToExecute: 1,
            Destination: new TilePosition(1, 0)));

        return new TravelUpdateScenario(
            InputApplication: application,
            InputDeltaTime: 1,
            AssertParty: party,
            ExpectedPosition: new TilePosition(1, 0),
            ExpectedIsInCombat: encounterProbability > 0);
    }

    public static CombatUpdateScenario ApplicationWithActiveWinningCombat()
    {
        var scenario = ApplicationWithTravelActionQueued(encounterProbability: 100);
        scenario.InputApplication.Update(scenario.InputDeltaTime);

        return new CombatUpdateScenario(
            InputApplication: scenario.InputApplication,
            InputDeltaTime: 1,
            ExpectedActiveCombats: 0);
    }

    public static AutomationProgramScenario ApplicationWithMiningProgram()
    {
        var party = new GameParty("party-1", new TilePosition(0, 0));
        party.AddMember(TestObjectMother.CreateCreature("Miner"));
        var macro = new PartyMacro("macro-1", "Mine Iron");
        macro.AddAction(new MoveToMacroAction("move-1", new TilePosition(1, 0)));
        macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Once));
        party.Automation.AddMacro(macro);
        party.Automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        var application = new GameApplication(
            TestObjectMother.CreateWorld(
                random: new SequenceRandomSource(1),
                minePositions: new[] { new TilePosition(1, 0) }),
            new[] { party },
            TestObjectMother.CreateGameConfig(),
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        return new AutomationProgramScenario(application, party);
    }

    public static AutomationProgramScenario ApplicationWithInvalidMiningProgram()
    {
        var party = new GameParty("party-1", new TilePosition(0, 0));
        party.AddMember(TestObjectMother.CreateCreature("Miner"));
        var macro = new PartyMacro("macro-1", "Invalid Mine");
        macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Once));
        party.Automation.AddMacro(macro);
        party.Automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        var application = new GameApplication(
            TestObjectMother.CreateWorld(random: new SequenceRandomSource(1)),
            new[] { party },
            TestObjectMother.CreateGameConfig(),
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        return new AutomationProgramScenario(application, party);
    }

    public static AutomationProgramScenario ApplicationWithMiningProgramWithoutHero()
    {
        var party = new GameParty("party-1", new TilePosition(0, 0));
        var macro = new PartyMacro("macro-1", "Mine Iron");
        macro.AddAction(new MoveToMacroAction("move-1", new TilePosition(1, 0)));
        macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Once));
        party.Automation.AddMacro(macro);
        party.Automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        var application = new GameApplication(
            TestObjectMother.CreateWorld(
                random: new SequenceRandomSource(1),
                minePositions: new[] { new TilePosition(1, 0) }),
            new[] { party },
            TestObjectMother.CreateGameConfig(),
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        return new AutomationProgramScenario(application, party);
    }

    public static AutomationProgramScenario ApplicationWithMiningProgramWithDeadHero()
    {
        var party = new GameParty("party-1", new TilePosition(0, 0));
        var hero = TestObjectMother.CreateCreature("Miner");
        hero.TakeDamage(hero.MaxLife);
        party.AddMember(hero);
        var macro = new PartyMacro("macro-1", "Mine Iron");
        macro.AddAction(new MoveToMacroAction("move-1", new TilePosition(1, 0)));
        macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Once));
        party.Automation.AddMacro(macro);
        party.Automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        var application = new GameApplication(
            TestObjectMother.CreateWorld(
                random: new SequenceRandomSource(1),
                minePositions: new[] { new TilePosition(1, 0) }),
            new[] { party },
            TestObjectMother.CreateGameConfig(),
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        return new AutomationProgramScenario(application, party);
    }
}

internal sealed record MiningUpdateScenario(
    GameApplication InputApplication,
    float InputDeltaTime,
    GameParty AssertParty,
    string AssertItemName,
    int AssertItemLevel,
    int ExpectedQuantity);

internal sealed record TransferUpdateScenario(
    GameApplication InputApplication,
    float InputDeltaTime,
    GameParty AssertTargetParty,
    string AssertItemName,
    int AssertItemLevel,
    int ExpectedQuantity);

internal sealed record TravelUpdateScenario(
    GameApplication InputApplication,
    float InputDeltaTime,
    GameParty AssertParty,
    TilePosition ExpectedPosition,
    bool ExpectedIsInCombat);

internal sealed record CombatUpdateScenario(GameApplication InputApplication, float InputDeltaTime, int ExpectedActiveCombats);

internal sealed record AutomationProgramScenario(GameApplication InputApplication, GameParty AssertParty);
