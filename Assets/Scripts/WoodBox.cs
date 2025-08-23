using System.Collections;
using UnityEngine;

public class WoodBox : Movable, IListener
{
    public bool hasPlayer = false;
    
    void Start()
    {
        EventManager.Instance.AddListener(EVENT_TYPE.EUserMove, this);
        EventManager.Instance.AddListener(EVENT_TYPE.EUserSkip, this);
        EventManager.Instance.AddListener(EVENT_TYPE.EObjMove, this);
        EventManager.Instance.AddListener(EVENT_TYPE.EAnimDone, this);
        
        isInWater = TryGetOverlappedWater(transform.position);
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EUserMove:
                //플레이어 올라옴
                if ((Vector2)sender.transform.position + (Vector2)param == (Vector2)transform.position)
                {
                    hasPlayer = true;
                }
                //box 이동
                else if (!isInWater && param != null && (Vector2)sender.transform.position + (Vector2)param == (Vector2)transform.position)
                {
                    MovingTo((Vector2)param);
                    //Debug.Log(param);
                }
                break;
            case EVENT_TYPE.EUserSkip:
                //플레이어 가만히 있움
                if ((Vector2)sender.transform.position == (Vector2)transform.position)
                {
                    hasPlayer = true;
                }
                break;
            case EVENT_TYPE.EObjMove:
                //물살 이동
                MoveByWaterFlow();
                break;
            case EVENT_TYPE.EAnimDone:
                TryGetOverlappedWater(transform.position);
                break;
        }
    }

    private void MoveByWaterFlow()
    {
        Water water = null;
        if (TryGetOverlappedWater(transform.position, out water) && water.isFloating)
        {
            water.isWalkable = false;
            if (hasPlayer)
            {
                MovingTo(water.GetComponent<WaterFlow>().FloatingDir, GameManager.Instance.player.transform);
                hasPlayer = false;
            }
            else MovingTo(water.GetComponent<WaterFlow>().FloatingDir);
        }
        else
        {
            hasPlayer = false;
            EventManager.Instance.SendObjAnimDone();
        }
    }
}
