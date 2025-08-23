using System.Collections;
using UnityEngine;

public class WoodBox : Movable, IListener
{
    void Start()
    {
        EventManager.Instance.AddListener(EVENT_TYPE.EUserMove, this);
        EventManager.Instance.AddListener(EVENT_TYPE.EUserSkip, this);
        EventManager.Instance.AddListener(EVENT_TYPE.EObjMove, this);
        
        isInWater = TryGetOverlappedWater(transform.position);
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EUserMove:
                //box 이동
                if (!isInWater && param != null && (Vector2)sender.transform.position + (Vector2)param == (Vector2)transform.position)
                {
                    MovingTo((Vector2)param);
                    //Debug.Log(param);
                }
                break;
            case EVENT_TYPE.EObjMove: 
                //물살 이동
                MoveByWaterFlow();
                break;
        }
    }

    private void MoveByWaterFlow()
    {
        Water water = null;
        if (TryGetOverlappedWater(transform.position, out water) && water.isFloating)
        {
            MovingTo(water.GetComponent<WaterFlow>().FloatingDir); 
        }
        else
        {
            EventManager.Instance.SendObjAnimDone();
        }
    }
}
