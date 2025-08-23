using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


public class Player : Entity
{
    [SerializeField] private float moveCooldown = 0.08f; // optional debounce
    private float _lastMoveTime;


    protected override void Awake()
    {
        base.Awake();
        // place the player in occupancy as a Top occupant
        Occupancy.PlaceTop(this, GridPos);
    }


    private bool CanAct()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Ready) return false;
        if (Time.time - _lastMoveTime < moveCooldown) return false;
        return true;
    }
    
    private void TryMove(Vector2Int dir)
    {
        Debug.Log("TryMove");
        if (!CanAct()) return;

        var from = GridPos;
        var to = from + dir;
        Debug.Log($"from {from.x},{from.y} to {to.x},{to.y}");

        // 목적지 점유 상태
        var destCell = Occupancy.GetCell(to);
        Debug.Log($"Occupancy: {destCell}");

        // ==== Case 1: 목적지에 아무도 없음 ====
        if (destCell == null)
        {
            // 빈 칸이면 해당 타일이 플레이어가 설 수 있는지만 보면 됨
            if (Occupancy.CanPlayerStand(to))
            {
                Occupancy.MoveTop(this, to);
                _lastMoveTime = Time.time;
                EventManager.Publish(GameEvent.PlayerActed, null);
                return;
            }
            // 물/흐름 같은 곳이면 상자 없이 못 들어감
            return;
        }

        // ==== 목적지에 무엇인가가 있음 ====
        // Top에 누가 있으면 진입 불가
        // ==== 목적지에 무엇인가가 있음 ====
// Top에 누가 있으면 기본은 진입 불가지만,
// ▶ 예외: Top이 WoodBox이고, 그 칸의 Bottom이 존재(=스택)하며, 옆으로 밀 곳이 비어있으면 "Top-push" 허용.
        if (destCell.Top != null)
        {
            var topBox = destCell.Top as WoodBox;
            if (topBox != null)
            {
                // 스택인지 확인 (보통 물/흐름 위 Bottom=WoodBox)
                bool isStack = destCell.Bottom is WoodBox;

                // 밀 방향
                var pushTo = to + dir;

                // Top-push 가능 조건:
                //  - 스택(Top과 Bottom이 동시에 존재)
                //  - pushTo가 박스를 놓을 수 있는 타일
                //  - pushTo 셀은 완전히 비어 있음 (Bottom/Top 모두 null)
                if (isStack && Occupancy.CanPlaceBox(pushTo))
                {
                    var nextOcc = Occupancy.GetCell(pushTo);
                    bool nextEmpty = (nextOcc == null || (nextOcc.Bottom == null && nextOcc.Top == null));
                    if (nextEmpty)
                    {
                        // ✅ Top에 있던 나무 상자를 옆 칸의 Bottom으로 내려 밀어 넣는다
                        Occupancy.MoveTopToBottom(topBox, pushTo);

                        // 그리고 플레이어는 원래 그 칸으로 진입
                        // (물/흐름이라도 destCell.Bottom이 남아 있으므로 플레이어는 설 수 있음)
                        if (Occupancy.CanPlayerStand(to))
                        {
                            Occupancy.MoveTop(this, to);
                            _lastMoveTime = Time.time;
                            EventManager.Publish(GameEvent.PlayerActed, null);
                        }
                        return;
                    }
                }
            }

            // Top이 있지만 Top-push 조건이 안 맞으면 막힘
            return;
        }

        // Bottom이 박스인지 확인
        var box = destCell.Bottom as BoxBase;
        if (box == null)
        {
            // Bottom은 비었고 Top도 비었으면 destCell이 null이어야 하지만,
            // 방어적으로 CanPlayerStand 재확인
            if (Occupancy.CanPlayerStand(to))
            {
                Occupancy.MoveTop(this, to);
                _lastMoveTime = Time.time;
                EventManager.Publish(GameEvent.PlayerActed, null);
            }
            return;
        }

        // ==== Bottom에 박스가 있는 경우 ====
        // 타일 종류에 따라 행동이 달라진다.
        var tk = MapManager.Instance.GetTileKind(to);

        if (tk == TileKind.Ground)
        {
            // ✅ Ground에서는 "밀기 우선" 정책
            if (box.IsPushable)
            {
                var pushTo = to + dir;

                // --- A) 스택 푸시: 다음 칸이 물/흐름 & 그 칸 Bottom=WoodBox & Top 비어있으면
                var tk2 = MapManager.Instance.GetTileKind(pushTo);
                var nextOccForStack = Occupancy.GetCell(pushTo);
                bool canStack =
                    (tk2 == TileKind.Water || tk2 == TileKind.WaterFlow) &&
                    nextOccForStack != null &&
                    nextOccForStack.Top == null &&
                    nextOccForStack.Bottom is WoodBox;

                if (canStack)
                {
                    // 첫 박스(지상)를 "물 위 박스"의 Top으로 올린다
                    Occupancy.MoveBottomToTop(box, pushTo);
                    // 플레이어는 방금 비워진 칸(to)로 들어감
                    Occupancy.MoveTop(this, to);
                    _lastMoveTime = Time.time;
                    EventManager.Publish(GameEvent.PlayerActed, null);
                    return;
                }

                // --- B) 일반 푸시: 다음 칸이 완전히 비어 있으면
                if (Occupancy.CanPlaceBox(pushTo))
                {
                    var nextOcc = Occupancy.GetCell(pushTo);

                    // (명시 가드) 연속 박스면 금지
                    if (nextOcc != null && nextOcc.Bottom is BoxBase) return;

                    bool nextEmpty = (nextOcc == null || (nextOcc.Bottom == null && nextOcc.Top == null));
                    if (nextEmpty)
                    {
                        Occupancy.MoveBottom(box, pushTo);
                        Occupancy.MoveTop(this, to);
                        _lastMoveTime = Time.time;
                        EventManager.Publish(GameEvent.PlayerActed, null);
                        return;
                    }
                }
            }

            // 여기까지 왔으면 밀기 실패 → 지상에서는 올라서지 않고 막힘
            return;
        }
        else if (tk == TileKind.Water || tk == TileKind.WaterFlow)
        {
            // ✅ 물/흐름에서는 박스를 "밀지 않음".
            // 박스 위로 올라설 수 있다(Top이 비어있다는 조건은 위에서 이미 확인).
            // (WoodBox는 물에서 정지, 흐름은 턴 해소 때만 이동)
            if (MapManager.Instance.IsWalkableForPlayer(to))
            {
                Occupancy.MoveTop(this, to);
                _lastMoveTime = Time.time;
                EventManager.Publish(GameEvent.PlayerActed, null);
                return;
            }
            return;
        }
        else
        {
            // Empty 등 기타: 진입 불가
            return;
        }
    }

    
    
#if ENABLE_INPUT_SYSTEM
    // New Input System callback
    private void OnMove(InputValue value)
    {
        Vector2 v = value.Get<Vector2>();
        Vector2Int dir = Vector2Int.zero;
        if (v.x > 0.5f) dir = Vector2Int.right;
        else if (v.x < -0.5f) dir = Vector2Int.left;
        else if (v.y > 0.5f) dir = Vector2Int.up;
        else if (v.y < -0.5f) dir = Vector2Int.down;
        if (dir != Vector2Int.zero) TryMove(dir);
    }
#else
    private void Update()
    {
        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2Int.right;
        if (dir != Vector2Int.zero) TryMove(dir);
    }
#endif
}
    