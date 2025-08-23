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
}