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
                sr.color = new Color(125/255f,175/255f,255/255f);
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
                else
                {
                    UnderObjectsInMap.Add(pos.Add(dir), go);    
                    if (go.TryGetComponent(out SpriteRenderer sr))
                    {
                        sr.color = new Color(125/255f,175/255f,255/255f);
                    }
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
        // ADD: 이번 턴에 이미 이동한 Under 박스 추적(턴당 1칸 보장)
        var movedUnder = new HashSet<GameObject>(); // ADD

        // 한 턴에 최대 Under 개수만큼 스윕(안전 캡)
        int maxSweeps = Mathf.Max(1, UnderObjectsInMap.Count);

        for (int sweep = 0; sweep < maxSweeps; sweep++)
        {
            // 1) 현재 상태 기준으로 이동 후보 수집
            var candidates = new List<(MapPos from, MapPos to, GameObject box, Vector2 dir, GameObject rider)>();

            foreach (var kvp in UnderObjectsInMap)
            {
                var from = kvp.Key;
                var boxGO = kvp.Value;

                // ADD: 이미 이 턴에 한 칸 이동한 박스는 스킵(연속 이동 방지)
                if (movedUnder.Contains(boxGO)) continue; // ADD

                // 해류 칸인지 확인
                if (!FieldInfos.TryGetValue(from, out var info)) continue;
                if (!info.FieldType.Equals(Field_TYPE.MOVINGWATER)) continue;
                if (!info.MapObject || !info.MapObject.TryGetComponent(out MovingWater mw)) continue;

                var dir = mw.movingDir;
                var to = from.Add(dir);

                // 목적지 유효성: 맵 안, 땅이 아니고, Under 미점유
                if (!FieldInfos.TryGetValue(to, out var toInfo)) continue;
                if (toInfo.FieldType.Equals(Field_TYPE.GROUND)) continue;
                if (UnderObjectsInMap.ContainsKey(to)) continue;

                // 위에 타고 있는 게 박스면 이동 금지(규칙)
                GameObject rider = null;
                if (TopObjectsInMap.TryGetValue(from, out var topGO))
                {
                    if (topGO.GetComponent<WoodBox>() )//|| topGO.GetComponent<IronBox>())
                        continue; // 윗박스/아이언 올라타면 하부 이동 금지
                    rider = topGO;  // 플레이어 등은 함께 이동
                }

                // (선택 가드) 목적지 Top에 누가 서있으면 충돌로 간주해서 이동 금지
                if (TopObjectsInMap.ContainsKey(to)) continue;

                candidates.Add((from, to, boxGO, dir, rider));
            }

            if (candidates.Count == 0)
                break; // 이번 스윕에 움직일 게 없음 → 종료

            // 2) 동일 목적지 충돌 제거
            var toCount = new Dictionary<MapPos, int>();
            foreach (var c in candidates)
                toCount[c.to] = (toCount.TryGetValue(c.to, out var n) ? n : 0) + 1;

            // 3) 동시 적용
            int applied = 0;

            // 먼저 from 제거(충돌 없는 후보만 대상으로)
            foreach (var c in candidates)
            {
                if (toCount[c.to] != 1) continue;
                UnderObjectsInMap.Remove(c.from);
                if (c.rider != null) TopObjectsInMap.Remove(c.from);
            }

            // 그 다음 배치/좌표 갱신
            foreach (var c in candidates)
            {
                if (toCount[c.to] != 1) continue;

                // Under 이동
                UnderObjectsInMap[c.to] = c.box;
                c.box.transform.position += (Vector3)c.dir;
                if (c.box.TryGetComponent(out Things underThings))
                    underThings.pos = underThings.pos.Add(c.dir);

                // ADD: 이 턴에 이미 이동한 것으로 마킹
                movedUnder.Add(c.box); // ADD

                // Rider(플레이어 등) 동승
                if (c.rider != null)
                {
                    TopObjectsInMap[c.to] = c.rider;
                    c.rider.transform.position += (Vector3)c.dir;

                    if (c.rider.TryGetComponent(out Things t))
                        t.pos = t.pos.Add(c.dir);
                    else if (c.rider.TryGetComponent(out Player p))
                        p.pos = p.pos.Add(c.dir);
                }

                applied++;
            }

            // 이번 스윕에서 실제로 적용된 게 없으면 종료
            if (applied == 0) break;
            // 적용된 게 있으면 다음 스윕으로 넘어가서 "기차처럼" 뒤가 한 칸 더 따라옴
            // (앞 박스는 movedUnder 때문에 같은 턴에 2칸 이상 이동하지 않음)
        }

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