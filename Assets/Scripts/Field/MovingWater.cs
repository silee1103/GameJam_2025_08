using UnityEngine;

public class MovingWater : Water
{
    [SerializeField]
    private Vector2 movingDir = Vector2.up;
    
    void Start()
    {
        pos = new MapPos(transform.position);
        
        MapManager.Instance.AddMapInfo(new MapPos(transform.position), new MapInfo(Field_TYPE.MOVINGWATER, gameObject));
    }
}
