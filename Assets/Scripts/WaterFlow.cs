using UnityEngine;

public class WaterFlow : MonoBehaviour
{
    [SerializeField]
    private Vector2 floatingDir = Vector2.up;

    void Start()
    {
        transform.Rotate(floatingDir - Vector2.up);
    }
}
