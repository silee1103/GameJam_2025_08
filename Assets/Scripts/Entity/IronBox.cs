using UnityEngine;

public class IronBox : BoxBase
{
// Same as WoodBox for pushing/placement, but does NOT get moved by WaterFlow.
    protected override void Awake()
    {
        base.Awake();
        Occupancy.PlaceBottom(this, GridPos);
    }
}