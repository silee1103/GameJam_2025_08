using System;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour, IListener
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

    void Start()
    {
        EventManager.Instance.AddListener(EVENT_TYPE.EObjMove, this);
    }

    public void AddMapInfo(MapPos pos, MapInfo mapInfo)
    {
        FieldInfos.Add(pos, mapInfo);
        //Debug.Log($"{mapInfo.FieldType} has been added to map list, {pos.x}/{pos.y}");
    }

    public void AddObjectInfo(MapPos pos, GameObject obj)
    {
        if (FieldInfos[pos].FieldType.Equals(Field_TYPE.GROUND)) TopObjectsInMap.Add(pos, obj);
        else
        {
            UnderObjectsInMap.Add(pos, obj);
            if (obj.TryGetComponent(out SpriteRenderer sr))
            {
                sr.color = Color.blue;
            }
        }
        
        //Debug.Log($"{obj} has been added to map list, {pos.x}/{pos.y}");
    }

    public bool OccurPush(MapPos pos, Vector2 dir, int maxPush = 100)
    {
        int i = 0;
        while (TopObjectsInMap.ContainsKey(pos.Add(dir*i)))
        {
            i++;
        }
        Debug.Log(i);
        if (i >= maxPush) 
        {
            Debug.Log($"Cant push more than {maxPush}");
            return false;
        }
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
        if (go.TryGetComponent(out Things thing))
        {
            thing.pos = thing.pos.Add(dir);
            go.transform.position += (Vector3)dir;
            if (!FieldInfos[pos.Add(dir)].FieldType.Equals(Field_TYPE.GROUND) && !UnderObjectsInMap.ContainsKey(pos.Add(dir))) //잠수
            {
                TopObjectsInMap.Remove(pos);
                if (!go.TryGetComponent(out WoodBox box))
                {
                    //things이면서 woodbox는 아닌 것들
                    //전부 파괴
                    Debug.Log($"{go.name} destroyed!");
                    Destroy(go);
                }
                UnderObjectsInMap.Add(pos.Add(dir), go);
                if (go.TryGetComponent(out SpriteRenderer sr))
                {
                    sr.color = Color.blue;
                }
                return;
            }
            TopObjectsInMap.Remove(pos);
            TopObjectsInMap.Add(pos.Add(dir), go);
        }
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EObjMove:
                ApplyMovingWater();
                break;
        }
    }

    private void ApplyMovingWater()
    {
        Dictionary<MapPos, GameObject> newUnderObjectsInMap = new Dictionary<MapPos, GameObject>();
        foreach (KeyValuePair<MapPos, GameObject> kvp in UnderObjectsInMap)
        {
            if (FieldInfos[kvp.Key].FieldType.Equals(Field_TYPE.MOVINGWATER)) //물 속 블럭이 해류와 겹쳐 있을 때
            {
                if (FieldInfos[kvp.Key].MapObject.TryGetComponent(out MovingWater mw)) //해류 저장
                {
                    if (FieldInfos[kvp.Key.Add(mw.movingDir)].FieldType.Equals(Field_TYPE.GROUND) || //해류타고 가는 곳이 땅이거나
                        UnderObjectsInMap.ContainsKey(kvp.Key.Add(mw.movingDir))) //해류타고 가는 곳에 무언가 이미 있거나
                    {
                        newUnderObjectsInMap.Add(kvp.Key, kvp.Value);
                        continue;
                    }
                    else //해류타고 가기
                    {
                        newUnderObjectsInMap.Add(kvp.Key.Add(mw.movingDir), kvp.Value);
                        kvp.Value.transform.position += (Vector3)mw.movingDir;
                        kvp.Value.GetComponent<Things>().pos
                            = kvp.Value.GetComponent<Things>().pos.Add(mw.movingDir);
                        
                        //해류타고 가는 상자 위에 물건이 있으면 함 께 감
                        if (TopObjectsInMap.ContainsKey(kvp.Key))
                        {
                            GameObject go = TopObjectsInMap[kvp.Key];
                            TopObjectsInMap.Remove(kvp.Key);
                            TopObjectsInMap.Add(kvp.Key.Add(mw.movingDir), go);
                            go.transform.position += (Vector3)mw.movingDir;
                            if (go.TryGetComponent(out Things things))
                            {
                                things.pos = things.pos.Add(mw.movingDir);
                                Debug.Log(things.pos.ToString());
                            }
                            else if (go.TryGetComponent(out Player player))
                            {
                                player.pos = player.pos.Add(mw.movingDir);
                                Debug.Log(player.pos.ToString());
                            }
                        }
                        
                        continue;
                    }
                }
                newUnderObjectsInMap.Add(kvp.Key, kvp.Value);
            }
            newUnderObjectsInMap.Add(kvp.Key, kvp.Value);
        }
        UnderObjectsInMap = newUnderObjectsInMap;
        GameManager.Instance.ChangeState(Game_State.READY_PHASE);
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

    public override string ToString()
    {
        return $"x:{x}, y:{y}";
    }
}