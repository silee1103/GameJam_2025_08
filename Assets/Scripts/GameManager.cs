using System;
using UnityEngine;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    public Player player = null;
    
    public static GameManager Instance { get { return _instance; } }
    private static GameManager _instance = null;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        
        if(_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        DestroyImmediate(gameObject);
    }

}
