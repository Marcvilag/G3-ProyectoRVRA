using UnityEngine;

public class HintIcon : MonoBehaviour
{
    public float bobHeight = 0.15f, bobSpeed = 2f, idleSeconds = 4f;

    SpriteRenderer sr;
    Vector3 basePos;
    float lastTouch;
    Camera cam;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        basePos = transform.localPosition;
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount > 0 || Input.GetMouseButton(0)) lastTouch = Time.time;

        float target = (Time.time - lastTouch > idleSeconds) ? 1f : 0f;
        var c = sr.color;
        c.a = Mathf.MoveTowards(c.a, target, Time.deltaTime * 2f);
        sr.color = c;

        transform.localPosition = basePos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.forward = cam.transform.forward;
    }
}