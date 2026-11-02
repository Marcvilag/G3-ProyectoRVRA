using UnityEngine;
using UnityEngine.Splines;

public class FlySwarm : MonoBehaviour
{
    [Header("Moscas")]
    public GameObject[] flyPrefabs;
    public SplineContainer path;              // UN solo spline
    public int count = 50;
    public Vector2 scaleRange = new Vector2(0.9f, 1.1f);
    public Vector3 modelRotationOffset = Vector3.zero; // corrige hacia dónde mira el modelo

    [Header("Línea de tiempo (30 s)")]
    public float totalTime = 30f;
    public float entryStagger = 3f;           // las moscas entran repartidas en estos segundos

    [Tooltip("Eje X: tiempo normalizado (0-1). Eje Y: posición en el spline (0-1)")]
    public AnimationCurve progress = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.13f, 0.15f),   // entra por la ventana
        new Keyframe(0.40f, 0.22f),   // revolotea en la habitación (casi plano)
        new Keyframe(0.65f, 0.85f),   // recorre el pasillo
        new Keyframe(1f, 1f));        // revolotea al final

    [Tooltip("Cuánto se abre la nube en cada momento. Alto = revoloteo, bajo = en fila")]
    public AnimationCurve spreadOverTime = new AnimationCurve(
        new Keyframe(0f, 0.3f),
        new Keyframe(0.10f, 0.3f),
        new Keyframe(0.17f, 1f),
        new Keyframe(0.38f, 1f),
        new Keyframe(0.45f, 0.3f),
        new Keyframe(0.60f, 0.3f),
        new Keyframe(0.72f, 1f),
        new Keyframe(1f, 1f));

    [Header("Nube / orgánico")]
    public float spread = 1.0f;               // radio de la nube en el revoloteo
    public Vector2 noiseAmplitudeRange = new Vector2(0.3f, 0.8f);
    public Vector2 noiseFrequencyRange = new Vector2(0.8f, 1.8f);

    [Header("Depuración")]
    [Range(0f, 1f)] public float debugT = 0f; // mueve el slider para ver la posición en el spline

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject root = new GameObject("Fly_" + i);
            root.transform.SetParent(transform);

            var f = root.AddComponent<FlyPathFollower>();
            f.path = path;
            f.progress = progress;
            f.spreadOverTime = spreadOverTime;
            f.totalTime = totalTime;
            f.delay = Random.Range(0f, entryStagger);
            f.fixedOffset = Random.insideUnitSphere * spread;
            f.noiseAmp = Random.Range(noiseAmplitudeRange.x, noiseAmplitudeRange.y);
            f.noiseFreq = Random.Range(noiseFrequencyRange.x, noiseFrequencyRange.y);

            GameObject prefab = flyPrefabs[Random.Range(0, flyPrefabs.Length)];
            GameObject visual = Instantiate(prefab, root.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(modelRotationOffset);
            visual.transform.localScale *= Random.Range(scaleRange.x, scaleRange.y);

            var oldAnim = visual.GetComponent<SplineAnimate>();
            if (oldAnim != null) Destroy(oldAnim);

            var animator = visual.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.speed = Random.Range(0.9f, 1.15f);
                animator.Play(0, 0, Random.value);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (path == null) return;
        path.Evaluate(debugT, out Unity.Mathematics.float3 pos, out _, out _);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere((Vector3)pos, 0.15f);
    }
}