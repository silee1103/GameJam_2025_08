using UnityEngine;

public class MovingWater : Water
{
    [SerializeField]
    public Vector2 movingDir = Vector2.up;
    
    void Start()
    {
        transform.Rotate(Vector3.forward, (movingDir.x + movingDir.y) * Vector2.Angle(Vector2.up, movingDir));
        pos = new MapPos(transform.position);
        
        MapManager.Instance.AddMapInfo(new MapPos(transform.position), new MapInfo(Field_TYPE.MOVINGWATER, gameObject));
    }
}
