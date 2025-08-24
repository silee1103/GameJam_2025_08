using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    // --- [추가] 애니메이션 관련 ---
    [Header("Anim")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer sr;   // 없다면 GetComponent<SpriteRenderer>()로 대체
    [SerializeField] private string pIsMoving = "IsMoving";
    [SerializeField] private string pMoveX = "MoveX";
    [SerializeField] private string pMoveY = "MoveY";

    [Header("Push Sprites")]
    [SerializeField] private Sprite pushUp;
    [SerializeField] private Sprite pushDown;
    [SerializeField] private Sprite pushLeft;
    [SerializeField] private Sprite pushRight;
    
    private Vector2 lastLook = Vector2.down;
    
    private GameManager _gameManager;
    private Transform transform;

    public MapPos pos;

    private void Awake()
    {
        transform = this.GetComponent<Transform>();
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (!animator) animator = GetComponent<Animator>();
    }

    private void Start()
    {
        pos = new MapPos(transform.position);
        
        _gameManager = GameManager.Instance.GetComponent<GameManager>();
        MapManager.Instance.AddObjectInfo(new MapPos(transform.position), gameObject);
    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        //Debug.Log(input.ToString());
        if (GameManager.Instance.State == Game_State.READY_PHASE && input != Vector2.zero) 
        {
            var nextPos = pos.Add(input);

            // 앞칸이 Top에 뭐가 있으면 이번 이동은 "미는 중"
            bool isPushing = MapManager.Instance.TopObjectsInMap.ContainsKey(nextPos);

            if (MapManager.Instance.FieldInfos[pos.Add(input)].FieldType.Equals(Field_TYPE.GROUND) //앞에가 땅이거나
                || (!MapManager.Instance.FieldInfos[pos.Add(input)].FieldType.Equals(Field_TYPE.GROUND) &&
                    MapManager.Instance.UnderObjectsInMap.ContainsKey(pos.Add(input)))) //물이지만 블럭이 있음
            {
                if (MapManager.Instance.OccurPush(pos.Add(input), input))
                {
                    GameManager.Instance.ChangeState(Game_State.WALKING_PHASE);
                    Debug.Log("Can Go!");
                    MapManager.Instance.TopObjectsInMap.Remove(pos); 
                    pos = pos.Add(input);
                    MapManager.Instance.TopObjectsInMap.Add(pos, gameObject);
                    
                    //transform.position += (Vector3)input;
                    StartMoveAnim(input, isPushing);
                    MovingAnimation(input, isPushing);
                }
                else //끝에 벽에 막힘
                {
                    lastLook = input;
                    SetIdleFacing(lastLook);
                }
            }
            else
            {
                //Debug.Log(MapManager.Instance.FieldInfos[pos.Add(input)].FieldType.Equals(Field_TYPE.GROUND));
                //Debug.Log(!MapManager.Instance.FieldInfos[pos.Add(input)].FieldType.Equals(Field_TYPE.GROUND));
                //Debug.Log(MapManager.Instance.UnderObjectsInMap.ContainsKey(pos.Add(input)));
                Debug.Log("Cant Go");
            }
        }
    }
    private void StartMoveAnim(Vector2 dir, bool isPushing)
    {
        lastLook = dir.normalized;

        if (isPushing)
        {
            // 미는 동안: 지정 스프라이트 고정(Animator 잠깐 끄기)
            if (animator) animator.enabled = false;
            if (sr) sr.sprite = GetPushSprite(lastLook);
        }
        else
        {
            // 걷기 애니메이션 재생
            if (animator)
            {
                animator.enabled = true;
                animator.speed = 1f;
                animator.SetBool(pIsMoving, true);
                animator.SetFloat(pMoveX, Mathf.RoundToInt(lastLook.x));
                animator.SetFloat(pMoveY, Mathf.RoundToInt(lastLook.y));
                PlayWalkState(lastLook, 0f); // 첫 프레임에서 시작
            }
        }
    }

    void MovingAnimation(Vector2 dir, bool isPushing)
    {
        if (dir.y == 0) StartCoroutine(MoveCoroutine(dir, .8f, 0.6f, isPushing));
        else            StartCoroutine(MoveCoroutine(dir, .65f, 0f,   isPushing));
    }

    IEnumerator MoveCoroutine(Vector2 dir, float duration, float heightOffset = 0, bool isPushing = false)
    {
        Vector3 dest = transform.position + (Vector3)dir;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.fixedDeltaTime / duration;
            transform.position += (Vector3)dir * (Time.fixedDeltaTime / duration)
                                  + new Vector3(0, 0.5f - t, 0) * (Time.fixedDeltaTime * heightOffset);
            yield return null;
        }

        transform.position = dest;

        // 이동 끝 → idle 동안 "바라보던 방향"의 첫 프레임으로 고정
        if (isPushing)
        {
            // 다시 Animator 켜고 idle 고정
            if (animator) animator.enabled = true;
            SetIdleFacing(lastLook);
        }
        else
        {
            if (animator)
            {
                animator.SetBool(pIsMoving, false);
                SetIdleFacing(lastLook);
            }
        }

        GameManager.Instance.ChangeState(Game_State.OBJECT_PHASE);
    }
    
    void OnSkip(InputValue value)
    {
        if (GameManager.Instance.State == Game_State.READY_PHASE)
        {
            //GameManager.Instance.ChangeState(Game_State.WALKING_PHASE);
            GameManager.Instance.ChangeState(Game_State.OBJECT_PHASE);
        }
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EUserMove:
                break;
            case EVENT_TYPE.EUserSkip:
                break;
            case EVENT_TYPE.EAnimDone:
                break;
        }
    }
    
    private void SetIdleFacing(Vector2 dir)
    {
        if (!animator) return;

        // 해당 방향 걷기 state를 "첫 프레임"에서 정지
        PlayWalkState(dir, 0f);
        animator.speed = 0f; // 고정
    }
    
    private void PlayWalkState(Vector2 dir, float normalizedTime)
    {
        if (!animator) return;
        string state = GetWalkStateName(dir);
        animator.Play(state, 0, normalizedTime);
    }

    private string GetWalkStateName(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x > 0 ? "WalkRight" : "WalkLeft";
        else
            return dir.y > 0 ? "WalkUp" : "WalkDown";
    }

    private Sprite GetPushSprite(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x > 0 ? pushRight : pushLeft;
        else
            return dir.y > 0 ? pushUp : pushDown;
    }
}