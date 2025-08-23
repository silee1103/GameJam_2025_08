using System.Collections;
using UnityEngine;

public abstract class Movable : MonoBehaviour
{ 
    public bool isInWater = false;
    
    protected virtual void MovingTo(Vector2 dir, Transform target = null)
    {
        StartCoroutine(MoveCoroutine(dir, 1f, target));
    }

    protected virtual IEnumerator MoveCoroutine(Vector2 dir, float duration, Transform target = null)
    {
        yield return new WaitForSecondsRealtime(0.2f);
        duration -= 0.2f;
        Vector2 destination = (Vector2)transform.position + dir;
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / duration;
            transform.position += (Vector3)dir * Time.deltaTime / duration;
            if (target != null) target.transform.position = transform.position;
            yield return null;
        }
        transform.position = destination;
        isInWater = TryGetOverlappedWater(transform.position);
        EventManager.Instance.SendObjAnimDone();
    }

    protected virtual bool TryGetOverlappedWater(Vector2 pos, out Water water)
    {
        if (!MapManager.Instance.TryGetMapInPos(pos, out MapInfo mapInfo) || mapInfo.MapType == MAP_TYPE.GROUND)
        {
            water = null;
            return false;
        }
        else
        {
            if (mapInfo.water.isWalkable)
            {
                water = null;
                return false;
            }
            mapInfo.water.isWalkable = true;
            water = mapInfo.water;
            return true;    
        }
    }
    
    protected virtual bool TryGetOverlappedWater(Vector2 pos)
    {
        if (!MapManager.Instance.TryGetMapInPos(pos, out MapInfo mapInfo)) return false;
        if (mapInfo.MapType == MAP_TYPE.WATER)
        {
            if (mapInfo.water.isWalkable)
            {
                return false;
            }
            mapInfo.water.isWalkable = true;
            return true;    
        }
        else
        {
            return false;
        }
    }
}
