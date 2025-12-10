using UnityEngine;

public class SimpleInfinity : MonoBehaviour
{
    public float speed = 3f;
    public float scale = 200f;
    
    void Update()
    {
        float t = Time.time * speed;
        transform.position = new Vector3(
            Mathf.Sin(t) * scale + 734.5f,
            Mathf.Sin(2f * t) * (scale * 0.5f) + 576f,
            0
        );
    }
}