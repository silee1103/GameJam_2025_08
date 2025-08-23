using System.Collections;
using UnityEngine;

public class Box : MonoBehaviour, IListener
{
    void Start()
    {
        EventManager.Instance.AddListener(EVENT_TYPE.EUserMove, this);
        EventManager.Instance.AddListener(EVENT_TYPE.EUserSkip, this);
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.EUserMove:
                //box 이동 생각 + 물살 이동
                if (param != null && (Vector2)sender.transform.position + (Vector2)param == (Vector2)transform.position)
                {
                    MoveBox((Vector2)param);
                    Debug.Log(param);
                }
                break;
            case EVENT_TYPE.EUserSkip:
                //물살 이동
                break;
        }
    }

    private void MoveBox(Vector2 dir)
    {
        StartCoroutine(MoveCoroutine(dir, 1f));
    }

    IEnumerator MoveCoroutine(Vector2 dir, float duration)
    {
        yield return new WaitForSeconds(0.2f);
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
    }
}
