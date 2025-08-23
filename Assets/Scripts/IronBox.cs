using System.Collections;
using UnityEngine;

public class IronBox : Movable, IListener
{
    void Start()
    {
        EventManager.Instance.AddListener(EVENT_TYPE.EUserMove, this);
        EventManager.Instance.AddListener(EVENT_TYPE.EUserSkip, this);
        //EventManager.Instance.AddListener(EVENT_TYPE.EObjMove, this); //혼자 움직일 일이 없음!
        EventManager.Instance.AddListener(EVENT_TYPE.EAnimDone, this);
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EUserMove:
                //box 이동 생각
                if (!isInWater && param != null && (Vector2)sender.transform.position + (Vector2)param == (Vector2)transform.position)
                {
                    MovingTo((Vector2)param);
                    Debug.Log(param);
                }
                break;
            case EVENT_TYPE.EUserSkip:
                TryGetOverlappedWater(transform.position);
                break;
        }
    }
}
