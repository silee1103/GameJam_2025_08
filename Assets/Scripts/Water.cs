using UnityEngine;

public class Water : MonoBehaviour
{
    public Vector2 pos;
    public bool isFloating = false;
    public bool isWalkable = false;

    protected virtual void Awake()
    {
        pos = transform.position;
    }

    protected virtual void Start()
    {
        MapManager.Instance.AddMapInfo(this.gameObject, MAP_TYPE.WATER, this);
    }
}
