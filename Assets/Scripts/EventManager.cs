using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get { return _instance; } }
    
    private static EventManager _instance = null;
    private Dictionary<EVENT_TYPE, List<IListener>> Listeners =
        new Dictionary<EVENT_TYPE, List<IListener>>();

    void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        DestroyImmediate(gameObject);
    }
    
    public void AddListener(EVENT_TYPE eventType, IListener Listener) //이벤트타입에 해당하는 리스트에 리스너 추가
    {
        List<IListener> ListenList = null;
        
        if(Listeners.TryGetValue(eventType, out ListenList))
        {
            ListenList.Add(Listener);
            return;
        }
        
        ListenList = new List<IListener>();
        ListenList.Add(Listener);
        Listeners.Add(eventType, ListenList);
    }

    public void RemoveListener(EVENT_TYPE eventType, IListener Listener)
    {
        List<IListener> ListenList = null;
        
        if (Listeners.TryGetValue(eventType, out ListenList))
        {
            ListenList.Remove(Listener);
        }
    }
    
    public void RemoveEvent(EVENT_TYPE eventType) => Listeners.Remove(eventType); //쓰지 않는 이벤트 삭제
    
    public void PostNotification(EVENT_TYPE eventType, Component sender, object param = null) //이벤트 개시
    {
        //Debug.Log(eventType.ToString());
        List<IListener> ListenList = null;

        if (!Listeners.TryGetValue(eventType, out ListenList))
            return;
        
        for (int i = 0; i < ListenList.Count; i++)
        {
            ListenList?[i].OnEvent(eventType, sender, param);
        }
    }
}