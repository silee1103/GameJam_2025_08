
using UnityEngine;
using UnityEngine.Tilemaps; // 선택: Tilemap 없으면 자동 폴백

public enum TileKind { Empty, Ground, Water, WaterFlow }

[DefaultExecutionOrder(-1000)]
public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    [Header("좌표 변환 기준 (둘 중 하나 권장)")]
    [Tooltip("있으면 타일맵의 셀 중심 좌표를 사용합니다.")]
    [SerializeField] private Tilemap primaryTilemap;   // 선택
    [Tooltip("없으면 GridLayout의 WorldToCell을 사용합니다.")]
    [SerializeField] private GridLayout gridLayout;    // 선택

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!gridLayout) gridLayout = FindFirstObjectByType<GridLayout>();
        if (!primaryTilemap) primaryTilemap = FindFirstObjectByType<Tilemap>();
    }

    public Vector2Int WorldToGrid(Vector3 world)
    {
        if (primaryTilemap)
        {
            Vector3Int cell = primaryTilemap.WorldToCell(world);
            return new Vector2Int(cell.x, cell.y);
        }
        if (gridLayout)
        {
            Vector3Int cell = gridLayout.WorldToCell(world);
            return new Vector2Int(cell.x, cell.y);
        }
        // 최후 폴백: 1x1 정수 격자
        return Vector2Int.RoundToInt((Vector2)world);
    }

    public Vector3 GridToWorld(Vector2Int grid)
    {
        var cell = new Vector3Int(grid.x, grid.y, 0);
        if (primaryTilemap)
            return primaryTilemap.GetCellCenterWorld(cell);

        if (gridLayout)
        {
            // GridLayout은 '셀 원점'을 반환; 중심을 추정해 보정
            var world = gridLayout.CellToWorld(cell);
            var size  = gridLayout.cellSize;
            return world + new Vector3(size.x * 0.5f, size.y * 0.5f, 0f);
        }
        // 1x1 정수 격자 가정
        return new Vector3(grid.x + 0.5f, grid.y + 0.5f, 0f);
    }

    public TileKind GetTileKind(Vector2Int grid)
    {
        // 우선순위: Flow > Water > Ground
        if (WaterFlow.IsFlowCell(grid)) return TileKind.WaterFlow;
        if (Water.IsWaterCell(grid))    return TileKind.Water;
        if (Ground.IsGroundCell(grid))  return TileKind.Ground;
        return TileKind.Empty;
    }

    public bool IsWalkableForPlayer(Vector2Int grid)
    {
        switch (GetTileKind(grid))
        {
            case TileKind.Ground:
                return true;
            case TileKind.Water:
            case TileKind.WaterFlow:
                var occ = Occupancy.GetCell(grid);
                return occ != null && occ.Bottom is BoxBase; // 상자 위만 가능
            default:
                return false;
        }
    }

    public bool IsPlaceableForBox(Vector2Int grid)
    {
        var k = GetTileKind(grid);
        return k == TileKind.Ground || k == TileKind.Water || k == TileKind.WaterFlow;
    }
}
