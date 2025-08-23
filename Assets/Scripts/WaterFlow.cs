using System;
using UnityEngine;

public class WaterFlow : Water
{
    private void Awake()
    {
        pos = transform.position;
        isFloating = true;
    }

    public Vector2 FloatingDir
    {
        get { return floatingDir; }
    }
    [SerializeField]
    private Vector2 floatingDir = Vector2.up;

    void Start()
    {
        transform.Rotate(Vector3.forward, (floatingDir.x + floatingDir.y) * Vector2.Angle(Vector2.up, floatingDir));
        MapManager.Instance.AddMapInfo(this.gameObject, MAP_TYPE.WATER, this);
    }
}
