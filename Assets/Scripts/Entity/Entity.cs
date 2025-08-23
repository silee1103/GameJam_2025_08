using System.Collections.Generic;
using UnityEngine;


public abstract class Entity : MonoBehaviour
{
    public Vector2Int GridPos { get; private set; }


    protected virtual void Awake()
    {
        // Snap to grid on spawn
        GridPos = MapManager.Instance.WorldToGrid(transform.position);
        transform.position = MapManager.Instance.GridToWorld(GridPos);
    }


    public void SetGridPos(Vector2Int p)
    {
        GridPos = p;
        transform.position = MapManager.Instance.GridToWorld(p);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }
}

