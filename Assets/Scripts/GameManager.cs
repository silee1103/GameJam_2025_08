using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public enum Game_State
{
    READY_PHASE,
    WALKING_PHASE,
    OBJECT_PHASE,
    END_PHASE,
    PAUSE_PHASE,
}

public class GameManager : MonoBehaviour
{
    public List<Player> players = new List<Player>();
    private int mainPlayerIdx = 0;
    
    public static GameManager Instance { get { return _instance; } }
    private static GameManager _instance = null;
    
    [SerializeField]
    public Game_State State {  get { return _currState; } }
    [SerializeField]
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            players[mainPlayerIdx].GetComponent<Player>().isPlayable = false;
            players[mainPlayerIdx].GetComponent<PlayerInput>().enabled = false;
            mainPlayerIdx++;
            mainPlayerIdx %= players.Count;
            players[mainPlayerIdx].GetComponent<Player>().isPlayable = true;
            players[mainPlayerIdx].GetComponent<PlayerInput>().enabled = true;
        }
    }

    void Start()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
        {
            players.Add(go.GetComponent<Player>());
            go.GetComponent<PlayerInput>().enabled = false;
        }

        players[mainPlayerIdx].GetComponent<Player>().isPlayable = true;
        players[mainPlayerIdx].GetComponent<PlayerInput>().enabled = true;
    }

    public void ChangeState(Game_State newState)
    {
        _prevState = _currState;
        _currState = newState;

        if (newState == Game_State.OBJECT_PHASE)
        {
            EventManager.Instance.PostNotification(EVENT_TYPE.EObjMove, this);
        }
    }
}
