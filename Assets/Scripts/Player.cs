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
    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        //Debug.Log(input.ToString());
        if (GameManager.Instance.State == Game_State.READY_PHASE && input != Vector2.zero) //앞에 물이 있으면 못감
        {
            if (MapManager.Instance.OccurPush(pos.Add(input), input))
            {
                Debug.Log("Pushed!");
                //블럭 밀수 있음
                transform.position += (Vector3)input;
                pos = pos.Add(input);
            }
            else
            {
                Debug.Log("Not Pushed!");
                //블럭을 밀 수 없음
                return;
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