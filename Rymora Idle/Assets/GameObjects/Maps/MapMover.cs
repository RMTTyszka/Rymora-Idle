using System.Collections.Generic;
using System.IO;
using System.Linq;
using Heroes;
using Map;
using UnityEngine;

public class MapMover : MonoBehaviour {

    public Pathfinder Pathfinder { get; set; }
    public Queue<Vector3> waypoints;
    public Vector3 target;
    public Vector3 waypoint;
    public Party Party { get; set; }
    private float waypointTimer = 0f;
    private bool hasArrived = false;


    // Use this for initialization
    void Start () {
        Pathfinder = FindAnyObjectByType<Pathfinder>();
        Party = FindAnyObjectByType<Party>();
        waypoints = new Queue<Vector3>();
    }

    public void GoToClosetSafeSpot()
    {
        Vector3? safeSpots = null;
        foreach (var position in Pathfinder.regions.cellBounds.allPositionsWithin) 
        {
            if (!Pathfinder.regions.HasTile(position)) {
                continue;
            }

            var tile = Pathfinder.regions.GetTile(position) as Region;
            if (tile is not null && tile.safeSpot)
            {
                if (safeSpots is null)
                {
                    safeSpots = position;
                }
                else
                {
                    var distanceBetweenCurrentSafeStop = Vector3.Distance(position, transform.position);
                    var distanceBetweenNewSafeStop = Vector3.Distance(position, transform.position);

                    if (distanceBetweenNewSafeStop < distanceBetweenCurrentSafeStop)
                    {
                        safeSpots = position;
                    }
                }
            }
        }
        waypoints.Clear();
        Party.InitiateMovement(safeSpots.GetValueOrDefault());

    }

    // Update is called once per frame
	void Update () {
        //se chegou no waypoint, pega o proximo

        if (!hasArrived && Vector3.Distance(transform.position, waypoint) == 0) {
            if (waypoints.Any())
            {
                waypoint = waypoints.Dequeue();
            }
            else {
                hasArrived = true;
            }
        }

        // Pega novo caminho para o alvo, e se nao ta perto, se move na diferencao dele
        if (target != Vector3.zero) {
            waypointTimer += Time.deltaTime;
            if (waypointTimer >= 0.2f) {
                StartCoroutine(Pathfinder.GetWaypoints(transform.position, target, this));
                waypointTimer = 0f;
            }
            if (Vector3.Distance(transform.position, target) > 0)
            {
                hasArrived = false;
                if (Vector3.Distance(transform.position, target) < 1)
                {
                    MoveToTarget(target);
                }
                else
                {
                    MoveToTarget(waypoint);
                }

            }
            else {
                hasArrived = true;
                target = Vector3.zero;
                waypoint = Vector3.zero;
            }
        }

	}

    public void MoveToTarget(Vector3 wp) {
        MapTerrain floor = Pathfinder.ground.GetTile(Vector3Int.FloorToInt(transform.position)) as MapTerrain;

        Party.transform.position = Vector3.MoveTowards(transform.position, wp, Party.Speed(floor.moveSpeed) * Time.deltaTime * (float)floor.moveSpeed);
    }

}
