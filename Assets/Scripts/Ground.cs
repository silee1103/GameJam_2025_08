using UnityEngine;

public class Ground : MonoBehaviour
{
    protected virtual void Start()
    {
        MapManager.Instance.AddMapInfo(this.gameObject, MAP_TYPE.GROUND);
    }
}
