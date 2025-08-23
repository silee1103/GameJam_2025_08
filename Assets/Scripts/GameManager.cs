using System;
using UnityEngine;
using UnityEngine.XR;

[Serializable]
public enum Game_State
{
    READY_PHASE,
    WALKING_PHASE,
    OBJECT_PHASE,
    END_PHASE,
}

public class GameManager : MonoBehaviour
{
    public Player player = null;
    
    public static GameManager Instance { get { return _instance; } }
    private static GameManager _instance = null;
    
    [SerializeField]
    public Game_State State {  get { return _currState; } }
    private Game_State _currState = Game_State.READY_PHASE;
    private Game_State _prevState = Game_State.READY_PHASE;

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

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }

    public void ChangeState(Game_State newState)
    {
        _prevState = _currState;
        _currState = newState;
    }
}
