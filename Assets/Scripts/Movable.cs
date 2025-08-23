using System.Collections;
using UnityEngine;

public abstract class Movable : MonoBehaviour
{ 
    public bool isInWater = false;
    
    protected virtual void MovingTo(Vector2 dir)
    {
        StartCoroutine(MoveCoroutine(dir, 1f));
    }

    protected virtual IEnumerator MoveCoroutine(Vector2 dir, float duration)
    {
        yield return new WaitForSecondsRealtime(0.2f);
        duration -= 0.2f;
        Vector2 destination = (Vector2)transform.position + dir;
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / duration;
            transform.position += (Vector3)dir * Time.deltaTime / duration;
            yield return null;
        }
        transform.position = destination;
        isInWater = TryGetOverlappedWater(transform.position);
        EventManager.Instance.SendObjAnimDone();
    }

    protected virtual bool TryGetOverlappedWater(Vector2 pos, out Water water)
    {
        MapManager.Instance.TryGetMapInPos(pos, out MapInfo mapInfo);
        water = mapInfo.water;
        return water != null;
    }
    
    protected virtual bool TryGetOverlappedWater(Vector2 pos)
    {
        MapManager.Instance.TryGetMapInPos(pos, out MapInfo mapInfo);
        return mapInfo.water != null;
    }
}
