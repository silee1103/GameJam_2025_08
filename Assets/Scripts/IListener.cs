using UnityEngine;

public enum EVENT_TYPE
{
    EGameStart, //start the game
    EGameEnd,   //end the game
    EUserMove,   //player move
    EUserSkip,  //player use skip
    EObjMoveCaculate, //object caculate
    EObjMove,   //object moves
    EAnimDone,
}

public interface IListener
{
    void OnEvent(EVENT_TYPE eventType, Component sender, object param = null);
}