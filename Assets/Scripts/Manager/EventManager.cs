using System;
using System.Collections.Generic;


public enum GameEvent
{
    PlayerActed, // after the player successfully performs an action (move/push)
    TurnStarted, // environment resolution begins
    TurnResolved, // environment resolution ends
}


public static class EventManager
{
    private static readonly Dictionary<GameEvent, Action<object>> _table = new();


    public static void Publish(GameEvent evt, object payload)
    {
        if (_table.TryGetValue(evt, out var del)) del?.Invoke(payload);
    }


    public static void Subscribe(GameEvent evt, Action<object> handler)
    {
        if (_table.ContainsKey(evt)) _table[evt] += handler;
        else _table[evt] = handler;
    }


    public static void Unsubscribe(GameEvent evt, Action<object> handler)
    {
        if (_table.ContainsKey(evt)) _table[evt] -= handler;
    }
}