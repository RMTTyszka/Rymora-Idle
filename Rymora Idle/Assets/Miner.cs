using System.Collections;
using System.Collections.Generic;
using Heroes;
using Map;
using UnityEngine;

public class Miner
{
    public void Mine(PartyNode partyNode)
    {
        var mapTerrain = partyNode.CurrentTile;
        if (mapTerrain.CanMine())
        {
            var mine = mapTerrain as Mine;
            var mineRoll = Random.Range(1, 101);
            var total = mineRoll + partyNode.Party.Hero.Skills.Mine.TotalValue();
        }
    }
}
