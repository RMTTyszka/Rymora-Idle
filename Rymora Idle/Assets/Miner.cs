using System.Collections;
using System.Collections.Generic;
using Heroes;
using Map;
using UnityEngine;

public class Miner
{
    public void Mine(Party party)
    {
        var mapTerrain = party.CurrentTile;
        if (mapTerrain.CanMine())
        {
            var mine = mapTerrain as Mine;
            var mineRoll = Random.Range(1, 101);
            var total = mineRoll + party.Skills.Mine.TotalValue();
        }
    }
}
