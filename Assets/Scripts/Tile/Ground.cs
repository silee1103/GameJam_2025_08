using UnityEngine;

[DisallowMultipleComponent]
public class Ground : MonoBehaviour
{
    private static readonly System.Collections.Generic.HashSet<Vector2Int> _cells
        = new System.Collections.Generic.HashSet<Vector2Int>();

    public static bool IsGroundCell(Vector2Int grid) => _cells.Contains(grid);

    private void OnEnable()
    {
        _cells.Add(GridOf(transform.position));
    }

    private void OnDisable()
    {
        _cells.Remove(GridOf(transform.position));
    }

    private static Vector2Int GridOf(Vector3 world)
    {
        // MapManager가 있으면 그 좌표계를 사용, 없으면 1x1 정수 격자 가정
        return MapManager.HasInstance
            ? MapManager.Instance.WorldToGrid(world)
            : Vector2Int.RoundToInt((Vector2)world);
    }
}