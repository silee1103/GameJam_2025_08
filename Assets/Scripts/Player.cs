using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    private GameManager _gameManager;
    private Transform transform;

    public MapPos pos;

    private void Awake()
    {
        transform = this.GetComponent<Transform>();
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
            if (MapManager.Instance.FieldInfos[pos.Add(input)].FieldType.Equals(Field_TYPE.GROUND) //앞에가 땅이거나
                || (!MapManager.Instance.FieldInfos[pos.Add(input)].FieldType.Equals(Field_TYPE.GROUND) &&
                    MapManager.Instance.UnderObjectsInMap.ContainsKey(pos.Add(input)))) //물이지만 블럭이 있음
            {
                if (MapManager.Instance.OccurPush(pos.Add(input), input))
                {
                    GameManager.Instance.ChangeState(Game_State.WALKING_PHASE);
                    Debug.Log("Can Go!");
                    MapManager.Instance.TopObjectsInMap.Remove(pos);
                    transform.position += (Vector3)input;
                    pos = pos.Add(input);
                    MapManager.Instance.TopObjectsInMap.Add(pos, gameObject);
                    GameManager.Instance.ChangeState(Game_State.OBJECT_PHASE);
                }
                else //끝에 벽에 막힘
                {
                    
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

    void MovingAnimation(Vector2 dir)
    {
        
    }

    IEnumerator MoveCoroutine(Vector2 dir, float duration)
    {
        yield return null;
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
}