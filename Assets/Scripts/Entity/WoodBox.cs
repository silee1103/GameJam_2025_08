using System.Collections.Generic;
using UnityEngine;

public class WoodBox : BoxBase
{
    public static readonly HashSet<WoodBox> All = new();


    protected override void Awake()
    {
        base.Awake();
        All.Add(this);
        // place as bottom by default (boxes occupy bottom layer)
        Occupancy.PlaceBottom(this, GridPos);
    }


    private void OnDestroy()
    {
        All.Remove(this);
    }
}