using System.Collections;
using UnityEngine;

public class WoodBox : Movable, IListener
{
    void Start()
    {
        EventManager.Instance.AddListener(EVENT_TYPE.EUserMove, this);
        EventManager.Instance.AddListener(EVENT_TYPE.EUserSkip, this);
        
        isInWater = TryGetOverlappedWater(transform.position);
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EUserMove:
                //box 이동 생각 + 물살 이동
                if (isInWater)
                {
                    //물살 이동
                    MoveByWaterFlow();
                }
                else if (param != null && (Vector2)sender.transform.position + (Vector2)param == (Vector2)transform.position)
                {
                    MovingTo((Vector2)param);
                    Debug.Log(param);
                }
                break;
            case EVENT_TYPE.EUserSkip:
                //물살 이동
                MoveByWaterFlow();
                break;
        }
    }

    private void MoveByWaterFlow()
    {
        Water water = TryGetOverlappedWater(transform.position);
        if (water.isFloating)
        {
            MovingTo(water.GetComponent<WaterFlow>().FloatingDir);    
        }
    }
}
