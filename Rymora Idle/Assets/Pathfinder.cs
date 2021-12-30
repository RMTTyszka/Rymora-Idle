using System;
using System.Collections;
using System.Collections.Generic;
using Map;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour {

    public Tilemap ground;
    public Tilemap walls;
    public Tilemap enviroment;
    public List<Vector2Int> newWp;
    public Queue<Vector2Int> frontier;
    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public IEnumerator GetWaypoints(Vector2 origin, Vector2 target, MapMover monster) {
        newWp = new List<Vector2Int>();
        frontier = new Queue<Vector2Int>();
        Vector2Int endPos = Vector2Int.FloorToInt(target);
        frontier.Enqueue(Vector2Int.FloorToInt(origin));

        Dictionary<Vector2Int, Vector2Int> wp = new Dictionary<Vector2Int, Vector2Int>();
        wp.Add(Vector2Int.FloorToInt(origin), Vector2Int.FloorToInt(origin));
        while (frontier.Count >= 1) {
            Vector2Int current = frontier.Dequeue();
            foreach (Vector2Int next in getNeighbors(Vector2Int.FloorToInt(current))) {
                //yield return 0;
                if (!wp.ContainsKey(next)) {
                    frontier.Enqueue(next);
                   //Instantiate(pref, new Vector3(next.x+0.5f, next.y+0.5f, 0f), Quaternion.identity);
                    if (!wp.ContainsKey(next)) {
                        wp.Add(next, current);
                    }
                    if (next == Vector2Int.FloorToInt(target)) {
                       // Debug.Log("Achooo");
                        frontier.Clear();
                        break;
                    }
                }
            }
        }

        Vector2Int newCurrent = Vector2Int.FloorToInt(target);
        

        while (newCurrent != Vector2Int.FloorToInt(origin)) {
            newWp.Add(newCurrent);
            if (!wp.ContainsKey(newCurrent)) {
                break;
            }
            newCurrent = wp[newCurrent];
        }
        newWp.Reverse();
        monster.waypoints.Clear();
        foreach (Vector2Int pos in newWp) {
            monster.waypoints.Enqueue(new Vector3(pos.x, pos.y, 0));
        }
        if (monster.waypoints.Count > 0)
        {
            monster.waypoint = monster.waypoints.Dequeue();
        }
        else {
            monster.waypoint = monster.PartyNode.transform.position;
        }


        for (int i = 0; i < newWp.Count-1; i++)
        {
            Vector3 ori = new Vector3(newWp[i].x, newWp[i].y, 0);
            Vector3 dest = new Vector3(newWp[i+1].x, newWp[i+1].y, 0);
            Debug.DrawLine(ori, dest, Color.red, 3f);
        }

        yield return 0;

        








    }

    public List<Vector2Int> getNeighbors(Vector2Int origin) {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        List<Vector2Int> positions = new List<Vector2Int>();


        for (int i = 0; i < 6; i++)
        {
            positions.Add(pointy_hex_corner(origin, 1, i));
        }
        foreach (Vector2Int tilePos in positions) 
        {
            Vector3Int position = new Vector3Int(tilePos.x, tilePos.y, 0);
            MapTerrain tile = ground.GetTile(position) as MapTerrain;
            if (tile != null && tile.isWalkable) 
            {
                Wall tile2 = walls.GetTile(position) as Wall;
                if (tile2 == null) 
                {
                      neighbors.Add(tilePos);
                }
            }
        }

        return neighbors;

    }

    private Vector2Int pointy_hex_corner(Vector2Int center, int size, int i)
    {
        var angle_deg = 60 * i - 30;
        var angle_rad = Math.PI / 180 * angle_deg;
        return Vector2Int.FloorToInt(new Vector2(center.x + size * (float)Math.Cos(angle_rad),
            center.y + size * (float)Math.Sin(angle_rad)));
    } 
    }

