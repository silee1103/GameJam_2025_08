using System;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Dictionary<MapPos, MapInfo> FieldInfos = new Dictionary<MapPos, MapInfo>();
    public Dictionary<MapPos, GameObject> UnderObjectsInMap = new Dictionary<MapPos, GameObject>();
    public Dictionary<MapPos, GameObject> TopObjectsInMap = new Dictionary<MapPos, GameObject>();
    
    public static MapManager Instance { get { return _instance; } }
    private static MapManager _instance = null;

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

    public void AddMapInfo(MapPos pos, MapInfo mapInfo)
    {
        FieldInfos.Add(pos, mapInfo);
        //Debug.Log($"{mapInfo.FieldType} has been added to map list, {pos.x}/{pos.y}");
    }

    public void AddObjectInfo(MapPos pos, GameObject obj)
    {
        if (FieldInfos[pos].FieldType.Equals(Field_TYPE.GROUND)) TopObjectsInMap.Add(pos, obj);
        else UnderObjectsInMap.Add(pos, obj);
        
        //Debug.Log($"{obj} has been added to map list, {pos.x}/{pos.y}");
    }

    public bool OccurPush(MapPos pos, Vector2 dir)
    {
        int i = 0;
        while (TopObjectsInMap.ContainsKey(pos.Add(dir*i)))
        {
            i++;
        }
        Debug.Log(i);
        //i개의 블럭들 있음
        if (FieldInfos.ContainsKey(pos.Add(dir * (i))))
        {
            for (int k = i - 1; k >= 0; k--) //o~i-1
            {
                DoTopPush(pos.Add(dir*k), dir);
            }
            return true;
        }
        return false;
    }

    public void DoTopPush(MapPos pos, Vector2 dir)
    {
        GameObject go = TopObjectsInMap[pos];
        if (go.TryGetComponent(out WoodBox box))
        {
            box.pos = box.pos.Add(dir);
            go.transform.position += (Vector3)dir;
            if (!FieldInfos[pos.Add(dir)].FieldType.Equals(Field_TYPE.GROUND) && !UnderObjectsInMap.ContainsKey(pos.Add(dir)))
            {
                TopObjectsInMap.Remove(pos);
                UnderObjectsInMap.Add(pos.Add(dir), go);
                return;
            }
            TopObjectsInMap.Remove(pos);
            TopObjectsInMap.Add(pos.Add(dir), go);
        }
    }
}

public enum Field_TYPE
{
    GROUND,
    WATER,
    MOVINGWATER,
    
}

public class MapInfo
{
    public Field_TYPE FieldType;
    public GameObject MapObject;

    public MapInfo(Field_TYPE fieldType, GameObject mObject = null)
    {
        FieldType = fieldType;
        MapObject = mObject;
    }
}

[Serializable]
public class MapPos
{
    public int x;
    public int y;

    public MapPos(int X, int Y)
    {
        this.x = X;
        this.y = Y;
    }

    public MapPos(Vector2 position)
    {
        this.x = Mathf.RoundToInt(position.x);
        this.y = Mathf.RoundToInt(position.y);
    }

    public MapPos Add(MapPos pos1)
    {
        return new MapPos(pos1.x + x, pos1.y + y);
    }
    
    public MapPos Add(Vector2 dir)
    {
        return new MapPos(x + Mathf.RoundToInt(dir.x), y + Mathf.RoundToInt(dir.y));
    }

    public Vector2 ToVector2()
    {
        return new Vector2(this.x, this.y);
    }
    
    private bool Equals(MapPos pos)
    {
        return pos.x.Equals(x) && pos.y.Equals(y);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(x, y);
    }
    public override bool Equals(object obj)
    {
        return this.Equals(obj as MapPos);
    }
}