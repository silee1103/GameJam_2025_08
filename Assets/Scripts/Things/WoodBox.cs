using UnityEngine;

public class WoodBox : Things
{
    void Start()
    {
        pos = new MapPos(transform.position);
        //Debug.Log(pos);
        MapManager.Instance.AddObjectInfo(new MapPos(transform.position), gameObject);
        EventManager.Instance.AddListener(EVENT_TYPE.EObjMove, this);
    }

    public override void Destroyed()
    {
        
        Debug.Log("WoodBox destroyed");
    }
    
    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EObjMove:
                //EventManager.Instance.SendObjAnimDone();
                break;
        }
    }
}
