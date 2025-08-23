using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WaterFlowSystem
{
    private static Vector2Int DirToVec(Dir d) => d switch
    {
        Dir.Up    => Vector2Int.up,
        Dir.Down  => Vector2Int.down,
        Dir.Left  => Vector2Int.left,
        Dir.Right => Vector2Int.right,
        _ => Vector2Int.zero
    };

    /// <summary>
    /// 턴당 다중 스윕으로 해소. 각 박스는 최대 1칸만 이동.
    /// 기차처럼 앞이 비켜주면 뒤도 같은 턴에 한 칸 들어갈 수 있음.
    /// </summary>
    public static void ResolveAllFlows()
    {
        var map = MapManager.Instance;
        if (map == null) return;

        // 이 턴에 이미 움직인 박스들
        var moved = new HashSet<WoodBox>();

        // 안전 제한: 최대 박스 수만큼 스윕하면 반드시 종료
        int maxSweeps = Mathf.Max(1, WoodBox.All.Count);

        for (int sweep = 0; sweep < maxSweeps; sweep++)
        {
            // 1) 현재 점유 상태로 이동 후보 수집
            var candidates = new List<(WoodBox box, Vector2Int from, Vector2Int to, bool hasRider)>();

            foreach (var box in WoodBox.All)
            {
                if (!box || moved.Contains(box)) continue;

                var from = box.GridPos;
                if (map.GetTileKind(from) != TileKind.WaterFlow) continue;
                if (!WaterFlow.TryGetDirectionAt(from, out var dir)) continue;

                var fromCell = Occupancy.GetCell(from);
                // 상단에 박스가 올라타면 이동 금지(플레이어는 가능)
                if (fromCell != null && fromCell.Top is BoxBase) continue;

                var to = from + DirToVec(dir);

                if (!map.IsPlaceableForBox(to)) continue;

                var toCell = Occupancy.GetCell(to);
                // 목적지 바닥/탑 점유 시 금지 (동시 스윕에서는 "현재" 점유만 본다)
                if (toCell != null && (toCell.Bottom != null || toCell.Top != null)) continue;

                bool hasRider = (fromCell != null && fromCell.Top is Player);
                candidates.Add((box, from, to, hasRider));
            }

            if (candidates.Count == 0)
            {
                // 이번 스윕에서 아무도 못 움직이면 더 이상 진행 불가 → 종료
                break;
            }

            // 2) 동일 목적지 충돌 제거
            var toCount = new Dictionary<Vector2Int, int>();
            foreach (var c in candidates)
                toCount[c.to] = (toCount.TryGetValue(c.to, out var n) ? n : 0) + 1;

            // 3) 적용: 이번 스윕에 충돌 없는 것만 동시 이동, 그리고 'moved'에 마킹
            bool anyMovedThisSweep = false;

            foreach (var c in candidates)
            {
                if (toCount[c.to] != 1) continue; // 충돌

                var fromCell = Occupancy.GetCell(c.from);
                if (fromCell != null && fromCell.Top is BoxBase) continue; // 직전 순간에 올라탐 방어

                Occupancy.MoveBottom(c.box, c.to);
                moved.Add(c.box);
                anyMovedThisSweep = true;

                if (c.hasRider && fromCell != null && fromCell.Top is Player rider)
                    Occupancy.MoveTop(rider, c.to);
            }

            // 이번 스윕에 움직인 게 없으면 종료
            if (!anyMovedThisSweep) break;
            // 움직인 게 있더라도 다음 스윕에서는 'moved'에 찍혀 두 번 이동하진 않음.
        }
    }

    /// <summary>
    /// (옵션) 시각화용: 스윕마다 딜레이를 주며 해소하는 코루틴.
    /// GameManager에서 stepDelay를 스윕 간 지연으로 쓰고 싶을 때 사용.
    /// </summary>
    public static IEnumerator ResolveAllFlowsStepped(float perSweepDelay)
    {
        var map = MapManager.Instance;
        if (map == null) yield break;

        var moved = new HashSet<WoodBox>();
        int maxSweeps = Mathf.Max(1, WoodBox.All.Count);

        for (int sweep = 0; sweep < maxSweeps; sweep++)
        {
            var candidates = new List<(WoodBox box, Vector2Int from, Vector2Int to, bool hasRider)>();

            foreach (var box in WoodBox.All)
            {
                if (!box || moved.Contains(box)) continue;

                var from = box.GridPos;
                if (map.GetTileKind(from) != TileKind.WaterFlow) continue;
                if (!WaterFlow.TryGetDirectionAt(from, out var dir)) continue;

                var fromCell = Occupancy.GetCell(from);
                if (fromCell != null && fromCell.Top is BoxBase) continue;

                var to = from + DirToVec(dir);
                if (!map.IsPlaceableForBox(to)) continue;

                var toCell = Occupancy.GetCell(to);
                if (toCell != null && (toCell.Bottom != null || toCell.Top != null)) continue;

                bool hasRider = (fromCell != null && fromCell.Top is Player);
                candidates.Add((box, from, to, hasRider));
            }

            if (candidates.Count == 0) yield break;

            var toCount = new Dictionary<Vector2Int, int>();
            foreach (var c in candidates)
                toCount[c.to] = (toCount.TryGetValue(c.to, out var n) ? n : 0) + 1;

            bool anyMovedThisSweep = false;

            foreach (var c in candidates)
            {
                if (toCount[c.to] != 1) continue;

                var fromCell = Occupancy.GetCell(c.from);
                if (fromCell != null && fromCell.Top is BoxBase) continue;

                Occupancy.MoveBottom(c.box, c.to);
                moved.Add(c.box);
                anyMovedThisSweep = true;

                if (c.hasRider && fromCell != null && fromCell.Top is Player rider)
                    Occupancy.MoveTop(rider, c.to);
            }

            if (!anyMovedThisSweep) yield break;

            if (perSweepDelay > 0f) yield return new WaitForSeconds(perSweepDelay);
        }
    }
}
