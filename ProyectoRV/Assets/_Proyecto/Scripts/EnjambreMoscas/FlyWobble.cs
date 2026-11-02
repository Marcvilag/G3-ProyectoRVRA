using UnityEngine;

public class FlyWobble : MonoBehaviour
{
    public float amplitude = 0.3f;      // desvío máximo respecto al camino
    public float frequency = 1.2f;      // rapidez del vaivén
    public float rotationAmount = 15f;  // grados de inclinación aleatoria

    float seed;
    Vector3 basePos;
    Quaternion baseRot;

    void Start()
    {
        seed = Random.value * 100f;     // cada mosca con un ruido distinto
        basePos = transform.localPosition;
        baseRot = transform.localRotation;
    }

    void Update()
    {
        float t = Time.time * frequency;

        Vector3 offset = new Vector3(N(t, 0f), N(t, 10f), N(t, 20f));
        transform.localPosition = basePos + offset * amplitude;

        Vector3 tilt = new Vector3(N(t * 1.3f, 30f), N(t * 1.3f, 40f), N(t * 1.3f, 50f));
        transform.localRotation = baseRot * Quaternion.Euler(tilt * rotationAmount);
    }

    // Perlin devuelve 0..1, lo paso a -1..1
    float N(float t, float channel)
    {
        return (Mathf.PerlinNoise(seed + channel, t) - 0.5f) * 2f;
    }
}