using System.Collections.Generic;
using UnityEngine;


public static class Occupancy
{
    // Keep track of all occupied cells
    private static readonly Dictionary<Vector2Int, OccupancyCell> _cells = new();


    public static OccupancyCell GetCell(Vector2Int p)
    {
        _cells.TryGetValue(p, out var c);
        return c;
    }


    public static void EnsureCell(Vector2Int p)
    {
        if (!_cells.ContainsKey(p)) _cells[p] = new OccupancyCell { Pos = p };
    }


    public static void RemoveIfEmpty(Vector2Int p)
    {
        if (_cells.TryGetValue(p, out var c))
        {
            if (c.Bottom == null && c.Top == null) _cells.Remove(p);
        }
    }


    public static bool CanPlayerStand(Vector2Int p)
    {
        return MapManager.Instance.IsWalkableForPlayer(p);
    }


    public static bool CanPlaceBox(Vector2Int p)
    {
        if (!MapManager.Instance.IsPlaceableForBox(p)) return false;
        var c = GetCell(p);
        if (c == null) return true; // empty
        // If there's already a bottom box, we allow a top occupant only if rules permit (wood raft)
        if (c.Bottom == null) return true;
        if (c.Top == null)
        {
            // Allow stacking a box on top only when bottom is wood and tile is water/waterflow
            var tk = MapManager.Instance.GetTileKind(p);
            if (tk == TileKind.Water || tk == TileKind.WaterFlow)
            {
                if (c.Bottom is WoodBox) return true;
            }
        }
        return false;
    }
    public static void PlaceBottom(BoxBase box, Vector2Int p)
    {
        EnsureCell(p);
        var c = _cells[p];
        c.Bottom = box;
        box.SetGridPos(p);
    }


    public static void PlaceTop(Entity ent, Vector2Int p)
    {
        EnsureCell(p);
        var c = _cells[p];
        c.Top = ent;
        ent.SetGridPos(p);
    }


    public static void ClearTop(Vector2Int p)
    {
        var c = GetCell(p);
        if (c != null) c.Top = null;
        RemoveIfEmpty(p);
    }


    public static void ClearBottom(Vector2Int p)
    {
        var c = GetCell(p);
        if (c != null) c.Bottom = null;
        RemoveIfEmpty(p);
    }


    public static void MoveTop(Entity top, Vector2Int to)
    {
        // remove from old
        var from = top.GridPos;
        var cFrom = GetCell(from);
        if (cFrom != null && cFrom.Top == top) cFrom.Top = null;
        RemoveIfEmpty(from);


        // place new
        EnsureCell(to);
        var cTo = _cells[to];
        // safety: must be empty top
        cTo.Top = top;
        top.SetGridPos(to);
    }


    public static void MoveBottom(BoxBase box, Vector2Int to)
    {
        var from = box.GridPos;
        var cFrom = GetCell(from);
        if (cFrom != null && cFrom.Bottom == box) cFrom.Bottom = null;
        RemoveIfEmpty(from);


        EnsureCell(to);
        var cTo = _cells[to];
        cTo.Bottom = box;
        box.SetGridPos(to);
    }
    
    // Occupancy.cs 내부에 추가
    public static void MoveBottomToTop(BoxBase box, Vector2Int to)
    {
        // 1) from 셀에서 Bottom 해제
        var from = box.GridPos;
        var cFrom = GetCell(from);
        if (cFrom != null && cFrom.Bottom == box) cFrom.Bottom = null;
        RemoveIfEmpty(from);

        // 2) to 셀 Top으로 배치
        EnsureCell(to);
        var cTo = _cells[to];

        // 안전 가드: Top이 비어 있어야 함
        if (cTo.Top != null)
        {
            Debug.LogWarning($"[Occupancy] MoveBottomToTop target top occupied at {to}");
            return;
        }

        cTo.Top = box;

        // 3) 엔티티 그리드/트랜스폼 갱신
        box.SetGridPos(to);
    }
    
    // Occupancy.cs 내부에 추가
    public static void MoveTopToBottom(BoxBase topBox, Vector2Int to)
    {
        if (topBox == null) return;

        // 1) from 셀 Top에서 제거
        var from = topBox.GridPos;
        var cFrom = GetCell(from);
        if (cFrom != null && cFrom.Top == topBox)
        {
            cFrom.Top = null;
            RemoveIfEmpty(from);
        }

        // 2) to 셀 Bottom에 배치 (비어있다는 전제 하에서 호출됨)
        EnsureCell(to);
        var cTo = _cells[to];
        if (cTo.Bottom != null || cTo.Top != null)
        {
            Debug.LogWarning($"[Occupancy] MoveTopToBottom target not empty at {to}");
            return;
        }

        cTo.Bottom = topBox;

        // 3) 엔티티 좌표/트랜스폼 갱신
        topBox.SetGridPos(to);
    }


}