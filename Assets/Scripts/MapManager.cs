using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour, IListener
{
    public Dictionary<MapPos, MapInfo> FieldInfos = new Dictionary<MapPos, MapInfo>();
    public Dictionary<MapPos, GameObject> UnderObjectsInMap = new Dictionary<MapPos, GameObject>();
    public Dictionary<MapPos, GameObject> TopObjectsInMap = new Dictionary<MapPos, GameObject>();
    
    public static MapManager Instance { get { return _instance; } }
    private static MapManager _instance = null;
    
    private bool IsKey(GameObject go)      => go && go.GetComponent<Key>();
    private bool IsTreasure(GameObject go) => go && go.GetComponent<Treasure>();


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
        // 0) 인접 합체 체크: pos(첫 칸)와 pos2(그 다음 칸)
        if (TopObjectsInMap.TryGetValue(pos, out var first))
        {
            var pos2 = pos.Add(dir);
            if (TopObjectsInMap.TryGetValue(pos2, out var second))
            {
                bool isPair = (IsKey(first) && IsTreasure(second)) || (IsTreasure(first) && IsKey(second));
                if (isPair)
                {
                    // 일반 체인 푸시 금지하고 즉시 합체 처리
                    MergeKeyTreasureAndClear(pos, pos2);
                    return true; // 플레이어 이동은 가능
                }
            }
        }
        
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
            
            //go.transform.position += (Vector3)dir;

            // MovingAnimation(go, dir, /*heightOffset*/ 0.9f, /*signalDone*/ false);

            //MovingAnimation(go, dir);
            
            if (!FieldInfos[pos.Add(dir)].FieldType.Equals(Field_TYPE.GROUND) && !UnderObjectsInMap.ContainsKey(pos.Add(dir))) //잠수
            {
                TopObjectsInMap.Remove(pos);
                if (!go.TryGetComponent(out WoodBox box))
                {
                    //things이면서 woodbox는 아닌 것들
                    //전부 파괴
                    
                    //Debug.Log($"{go.name} destroyed!");
                    //Destroy(go);
                    
                    thing.Destroyed();
                }
                else
                {
                    MovingAnimation(go, dir);
                    UnderObjectsInMap.Add(pos.Add(dir), go);
                    if (go.TryGetComponent(out SpriteRenderer sr))
                    {
                        sr.color = new Color(125/255f,175/255f,255/255f);

                        sr.GetComponent<Renderer>().sortingOrder = 0;
                    }
                }
                CheckStageClear();
                return;
            }
            MovingAnimation(go, dir);
            TopObjectsInMap.Remove(pos);
            TopObjectsInMap.Add(pos.Add(dir), go);
        }
        CheckStageClear();
    }
    
    // MapManager.MovingAnimation에서 이렇게 호출
    void MovingAnimation(GameObject go, Vector2 dir, float heightOffset = 0.9f, bool signalDone = true)
    {
        float dur = .8f;
        if (dir.y != 0) heightOffset = 0f;

        var routine = MoveCoroutine(go, dir, dur, heightOffset, signalDone);
        if (go.TryGetComponent(out Things t))
            t.StartMove(routine, this);
        else
            StartCoroutine(routine); // fallback
    }


    IEnumerator MoveCoroutine(GameObject go, Vector2 dir, float duration, float heightOffset = 0, bool signalDone = true)
    {
        Vector3 dest = go.transform.position + (Vector3)dir;
        
        float time = 0;
        while (time < 1f)
        {
            time += Time.fixedDeltaTime / duration;
            go.transform.position += (Vector3)dir * Time.fixedDeltaTime / duration +
                                     new Vector3(0, 0.5f - time, 0) * (Time.fixedDeltaTime * heightOffset);
            yield return null;
        }
        
        go.transform.position = dest;

        yield return new WaitForSeconds(0.15f);
        if (signalDone) EventManager.Instance.SendObjAnimDone();
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
        var newUnder = new Dictionary<MapPos, GameObject>();
        int moves = 0;

        foreach (var kvp in UnderObjectsInMap)
        {
            var from = kvp.Key;
            var go   = kvp.Value;

            if (FieldInfos[from].FieldType.Equals(Field_TYPE.MOVINGWATER) &&
                FieldInfos[from].MapObject.TryGetComponent(out MovingWater mw))
            {
                var to = from.Add(mw.movingDir);

                // 못 가는 경우: 그대로 유지. (여기서 Done 쏘지 않음!)
                if ( FieldInfos[to].FieldType.Equals(Field_TYPE.GROUND) ||
                     UnderObjectsInMap.ContainsKey(to) )
                {
                    newUnder.Add(from, go);
                    continue;
                }

                // 가는 경우: 애니메이션 코루틴이 끝나며 Done을 "한 번" 쏨
                newUnder.Add(to, go);
                if (go.TryGetComponent(out Things t)) t.pos = t.pos.Add(mw.movingDir);
                MovingAnimation(go, mw.movingDir, 0, true);
                moves++;
                    
                // 위에 탑승자도 함께 이동 + 코루틴으로 이동 (Done 중복 X)
                if (TopObjectsInMap.ContainsKey(from))
                {
                    var rider = TopObjectsInMap[from];
                    TopObjectsInMap.Remove(from);
                    TopObjectsInMap.Add(to, rider);
                    MovingAnimation(rider, mw.movingDir, 0, true);
                    moves++;
                    if (rider.TryGetComponent(out Things rt)) rt.pos = rt.pos.Add(mw.movingDir);
                    else if (rider.TryGetComponent(out Player rp)) rp.pos = rp.pos.Add(mw.movingDir);
                }
            }
            else
            {
                newUnder.Add(from, go);
            }
        }

        UnderObjectsInMap = newUnder;
        if (moves == 0)
        {
            // 아무도 안 움직였으면 딱 한 번만
            EventManager.Instance.SendObjAnimDone();
        }
        else
        {
            EventManager.Instance.AddExtraAnim(moves - 1);
        }
        CheckStageClear();
    }

    
    // MapManager 클래스 내부에 추가
    private void CheckStageClear()
    {
        MapPos keyPos = null;
        MapPos treasurePos = null;
        GameObject treasureGO = null;

        // Top(땅 위)에서 탐색
        foreach (var kvp in TopObjectsInMap)
        {
            var go = kvp.Value;
            if (go == null) continue;
            if (go.GetComponent<Key>())       keyPos = kvp.Key;
            if (go.GetComponent<Treasure>())  { treasurePos = kvp.Key; treasureGO = go; }
        }
        
        // Under(물 위)에서도 혹시 모를 상황 대비해 탐색 (일반 규칙상 물로 가면 Things 중 Wood 이외는 파괴되지만 안전망으로 둠)
        foreach (var kvp in UnderObjectsInMap)
        {
            var go = kvp.Value;
            if (go == null) continue;
            if (go.GetComponent<Key>()) keyPos = kvp.Key;
            if (go.GetComponent<Treasure>()) { treasurePos = kvp.Key; treasureGO = go; }
    }

        if (keyPos != null && treasurePos != null && keyPos.Equals(treasurePos))
        {
            
            Debug.Log("Stage Clear");
        }
    }
    
    private void MergeKeyTreasureAndClear(MapPos a, MapPos b)
    {
        if (!TopObjectsInMap.TryGetValue(a, out var goA)) return;
        if (!TopObjectsInMap.TryGetValue(b, out var goB)) return;

        // 누가 Key/Treasure인지 식별
        bool aIsKey = goA.GetComponent<Key>() != null;
        bool bIsKey = goB.GetComponent<Key>() != null;
        bool aIsTreasure = goA.GetComponent<Treasure>() != null;
        bool bIsTreasure = goB.GetComponent<Treasure>() != null;

        GameObject keyGO = aIsKey ? goA : (bIsKey ? goB : null);
        GameObject treGO = aIsTreasure ? goA : (bIsTreasure ? goB : null);
        if (keyGO == null || treGO == null) return;

        // 스냅: A를 B 위치로
        if (goA.TryGetComponent(out Things tA)) tA.pos = new MapPos(b.x, b.y);
        goA.transform.position = goB.transform.position;

        // Treasure 열기
        if (treGO.TryGetComponent(out Animator treAnim))
            treAnim.SetBool("isOpen", true);

        // 맵 딕셔너리 정리
        TopObjectsInMap.Remove(a);
        if (keyGO == goB) TopObjectsInMap[b] = treGO; // b칸이 key였다면 treasure로 교체

        Debug.Log("Key + Treasure merged → Stage Clear");
        // GameManager.Instance.ChangeState(Game_State.END_PHASE);  // 원하면 즉시 종료
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