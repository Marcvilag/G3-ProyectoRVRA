using UnityEngine;
using UnityEngine.Splines;

public class FlyPathFollower : MonoBehaviour
{
    [HideInInspector] public SplineContainer path;
    [HideInInspector] public float loopTime = 8f;      // segundos por vuelta
    [HideInInspector] public float phase;               // posición inicial (0-1)
    [HideInInspector] public int direction = 1;         // 1 o -1
    [HideInInspector] public Vector3 fixedOffset;
    [HideInInspector] public float noiseAmp = 0.15f;
    [HideInInspector] public float noiseFreq = 1f;
    [HideInInspector] public float turnSpeed = 6f;

    float t;
    float seed;
    Vector3 lastPos;
    bool started;

    void Start()
    {
        seed = Random.value * 100f;
        t = phase;
    }

    void Update()
    {
        if (path == null) return;

        // Avanza y da la vuelta con Repeat: nunca se detiene
        t = Mathf.Repeat(t + direction * Time.deltaTime / loopTime, 1f);
        path.Evaluate(t, out Unity.Mathematics.float3 pos, out _, out _);

        float tn = Time.time * noiseFreq;
        Vector3 noise = new Vector3(N(tn, 0f), N(tn, 10f), N(tn, 20f));
        Vector3 newPos = (Vector3)pos + fixedOffset + noise * noiseAmp;

        if (!started)
        {
            lastPos = newPos;
            transform.position = newPos;
            started = true;
            return;
        }

        Vector3 vel = (newPos - lastPos) / Mathf.Max(Time.deltaTime, 0.0001f);
        if (vel.sqrMagnitude > 0.05f)
        {
            Quaternion target = Quaternion.LookRotation(vel.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
        }

        transform.position = newPos;
        lastPos = newPos;
    }

    float N(float t, float channel) =>
        (Mathf.PerlinNoise(seed + channel, t) - 0.5f) * 2f;
}