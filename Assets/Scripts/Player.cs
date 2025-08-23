using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour, IListener
{
    private GameManager _gameManager;
    private Transform transform;
    private bool isOnBox;

    private void Awake()
    {
        transform = this.GetComponent<Transform>();
    }

    private void Start()
    {
        _gameManager = GameManager.Instance.GetComponent<GameManager>();
    }
    
    private void Update()
    {
        
    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        if (input == Vector2.zero) return;
        if (MapManager.Instance.TryGetMapInPos((Vector2)transform.position + input, out var info)) //만약 앞에 물이면 못감
        {
            //TODO...
        }
        EventManager.Instance.PostNotification(EVENT_TYPE.EUserMove, this, input);
        MovingAnimation(input);
    }

    void MovingAnimation(Vector2 dir)
    {
        StartCoroutine(MoveCoroutine(dir, 1f));
    }

    IEnumerator MoveCoroutine(Vector2 dir, float duration)
    {
        Vector2 dest = (Vector2)transform.position + dir;
        float time = 0f;
        while (time < 1.0f)
        {
            time += Time.deltaTime / duration;
            transform.position += (Vector3)dir * Time.deltaTime / duration;
            yield return null;
        }
        transform.position = dest;
    }

    void OnSkip(InputValue value)
    {
        EventManager.Instance.PostNotification(EVENT_TYPE.EUserSkip, this);
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EUserMove:
                break;
            case EVENT_TYPE.EUserSkip:
                break;
        }
    }
}