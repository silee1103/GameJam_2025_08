using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get { return _instance; } }
    private static MapManager _instance = null;
    
    public Dictionary<Vector2, MapInfo> mapInfos = new Dictionary<Vector2, MapInfo>();

    void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        DestroyImmediate(gameObject);
    }

    public void AddMapInfo(GameObject obj, MAP_TYPE type, Water water = null)
    {
        mapInfos.Add((Vector2)obj.transform.position, new MapInfo(type, water));
        //Debug.Log($"{obj.name} has been added to map list, {(Vector2)obj.transform.position}");
    }

    public bool TryGetMapInPos(Vector2 pos, out MapInfo mapInfo)
    {
        mapInfo = mapInfos[pos];
        return mapInfo != null;
    }
}

public enum MAP_TYPE
{
    GROUND,
    WATER,
}

public class MapInfo
{
    public MAP_TYPE MapType;
    public Water water;

    public MapInfo(MAP_TYPE mapType, Water water = null)
    {
        MapType = mapType;
        this.water = water;
    }
}
