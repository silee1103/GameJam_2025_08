using UnityEngine;

public class WoodBox : Things
{
    void Start()
    {
        
        pos = new MapPos(transform.position);
        //Debug.Log(pos);
        MapManager.Instance.AddObjectInfo(new MapPos(transform.position), gameObject);
    }
}
