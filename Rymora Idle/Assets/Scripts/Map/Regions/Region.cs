
using System.Collections.Generic;
using Map;
using UnityEngine;

[CreateAssetMenu(fileName = "Terrain", menuName = "Tiles/Regions/Region")]
public class Region : RegionTile
{
    [SerializeField]
    public List<Encounter> encounters;

    public bool safeSpot;
}
