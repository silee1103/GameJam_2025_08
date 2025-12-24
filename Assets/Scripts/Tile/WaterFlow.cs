using UnityEngine;
using System.Collections.Generic;

public enum Dir { Up, Down, Left, Right }

[DisallowMultipleComponent]
public class WaterFlow : MonoBehaviour
{
    [SerializeField] public Dir direction = Dir.Right;

    private static readonly Dictionary<Vector2Int, Dir> _dirByCell
        = new Dictionary<Vector2Int, Dir>();

    public static bool IsFlowCell(Vector2Int grid) => _dirByCell.ContainsKey(grid);

    public static bool TryGetDirectionAt(Vector2Int grid, out Dir dir)
        => _dirByCell.TryGetValue(grid, out dir);

    private Vector2Int _gridPos;

    private void OnEnable()
    {
        _gridPos = GridOf(transform.position);
        _dirByCell[_gridPos] = direction;
    }

    private void OnDisable()
    {
        _dirByCell.Remove(_gridPos);
    }

    private static Vector2Int GridOf(Vector3 world)
    {
        return MapManager.HasInstance
            ? MapManager.Instance.WorldToGrid(world)
            : Vector2Int.RoundToInt((Vector2)world);
    }
}