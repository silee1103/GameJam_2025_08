using UnityEngine;

[DisallowMultipleComponent]
public class Water : MonoBehaviour
{
    private static readonly System.Collections.Generic.HashSet<Vector2Int> _cells
        = new System.Collections.Generic.HashSet<Vector2Int>();

    public static bool IsWaterCell(Vector2Int grid) => _cells.Contains(grid);

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
        return MapManager.HasInstance
            ? MapManager.Instance.WorldToGrid(world)
            : Vector2Int.RoundToInt((Vector2)world);
    }
}