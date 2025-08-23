using UnityEngine;

public class Ground : MonoBehaviour
{
    public MapPos pos;
    
    void Start()
    {
        pos = new MapPos(transform.position);
        
        MapManager.Instance.AddMapInfo(new MapPos(transform.position), new MapInfo(Field_TYPE.GROUND, gameObject));
    }
}
