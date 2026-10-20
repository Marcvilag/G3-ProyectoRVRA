using UnityEngine;
using UnityEngine.Splines;

public class FlyPathFollower : MonoBehaviour
{
    [HideInInspector] public SplineContainer path;
    [HideInInspector] public AnimationCurve progress;
    [HideInInspector] public AnimationCurve spreadOverTime;
    [HideInInspector] public float totalTime = 30f;
    [HideInInspector] public float delay;
    [HideInInspector] public Vector3 fixedOffset;
    [HideInInspector] public float noiseAmp = 0.4f;
    [HideInInspector] public float noiseFreq = 1f;
    [HideInInspector] public float turnSpeed = 6f;

    float elapsed;
    float seed;
    Vector3 lastPos;
    bool started;

    void Start()
    {
        seed = Random.value * 100f;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float n = Mathf.Clamp01((elapsed - delay) / totalTime);

        float p = Mathf.Clamp01(progress.Evaluate(n));
        path.Evaluate(p, out Unity.Mathematics.float3 pos, out _, out _);

        float amp = spreadOverTime.Evaluate(n);
        float t = elapsed * noiseFreq;
        Vector3 noise = new Vector3(N(t, 0f), N(t, 10f), N(t, 20f));
        Vector3 newPos = (Vector3)pos + (fixedOffset + noise * noiseAmp) * amp;

        if (!started)
        {
            lastPos = newPos;
            transform.position = newPos;
            started = true;
            return;
        }

        // Orientación según la velocidad real (en el revoloteo mira hacia donde se desvía)
        Vector3 vel = (newPos - lastPos) / Mathf.Max(Time.deltaTime, 0.0001f);
        if (vel.sqrMagnitude > 0.05f)
        {
            Quaternion target = Quaternion.LookRotation(vel.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
        }

        transform.position = newPos;
        lastPos = newPos;
    }

    float N(float t, float channel)
    {
        return (Mathf.PerlinNoise(seed + channel, t) - 0.5f) * 2f;
    }
}