using UnityEngine;

public class Things : MonoBehaviour
{
    public MapPos pos;

    void Start()
    {
        pos = new MapPos(transform.position);
        //Debug.Log(pos);
        MapManager.Instance.AddObjectInfo(new MapPos(transform.position), gameObject);
    }
}
