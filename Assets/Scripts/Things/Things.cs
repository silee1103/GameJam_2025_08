using UnityEngine;

public class Things : MonoBehaviour, IListener
{
    public MapPos pos;

    void Start()
    {
        pos = new MapPos(transform.position);
        //Debug.Log(pos);
        MapManager.Instance.AddObjectInfo(new MapPos(transform.position), gameObject);
        EventManager.Instance.AddListener(EVENT_TYPE.EObjMove, this);
    }

    public virtual void Destroyed()
    {
        //파괴되는 연출
        Destroy(gameObject);
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
