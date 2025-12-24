using UnityEngine;

public class OccupancyCell
{
    public Vector2Int Pos;
    public BoxBase Bottom; // a box occupying the ground of this cell
    public Entity Top; // optional top occupant (player or a box riding on a wood box)
}

