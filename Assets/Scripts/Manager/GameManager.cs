using System.Collections;
using UnityEngine;

public enum GameState
{
    Ready,      // waiting for player input
    Resolving,  // resolving environment updates (flows, etc.)
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState state = GameState.Ready;
    [SerializeField, Min(0f)] private float stepDelay = 0.2f; // set > 0 to visualize steps

    public GameState State => state;

    private Coroutine _turnRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        EventManager.Subscribe(GameEvent.PlayerActed, OnPlayerActed);
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.PlayerActed, OnPlayerActed);
    }

    private void OnPlayerActed(object _)
    {
        if (state != GameState.Ready) return;
        if (_turnRoutine != null) return; // 재진입 방지
        _turnRoutine = StartCoroutine(ResolveTurn());
    }

    private IEnumerator ResolveTurn()
    {
        state = GameState.Resolving;
        EventManager.Publish(GameEvent.TurnStarted, null);

        if (stepDelay > 0f) yield return new WaitForSeconds(stepDelay);

        // 1) Resolve waterflow (and other global environment rules in future)
        WaterFlowSystem.ResolveAllFlows();
        // 아래 딜레이/이벤트/상태복구는 예외가 나도 보장
        if (stepDelay > 0f) yield return new WaitForSeconds(stepDelay);

        EventManager.Publish(GameEvent.TurnResolved, null);
        state = GameState.Ready;
        _turnRoutine = null;
    }
}