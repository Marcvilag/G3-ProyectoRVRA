using UnityEngine;
using UnityEngine.Splines;

public class FlySwarm : MonoBehaviour
{
    [Header("Variedad")]
    public GameObject[] flyPrefabs;          // tus 3 FBX/prefabs
    public SplineContainer[] paths;

    [Header("Enjambre")]
    public int count = 50;
    public Vector2 durationRange = new Vector2(6f, 12f);
    public float spread = 0.5f;              // dispersión inicial respecto al camino
    public Vector2 scaleRange = new Vector2(0.9f, 1.1f);

    [Header("Orientación sobre el spline")]
    public SplineComponent.AlignAxis forwardAxis = SplineComponent.AlignAxis.ZAxis;
    public SplineComponent.AlignAxis upAxis = SplineComponent.AlignAxis.YAxis;

    [Header("Movimiento orgánico (Perlin)")]
    public bool useNoise = true;
    public Vector2 noiseAmplitudeRange = new Vector2(0.15f, 0.4f);
    public Vector2 noiseFrequencyRange = new Vector2(0.8f, 1.8f);
    public float noiseRotation = 15f;

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            // 1. Contenedor que recorre el spline
            GameObject root = new GameObject("Fly_" + i);
            root.transform.SetParent(transform);

            var anim = root.AddComponent<SplineAnimate>();
            anim.Container = paths[Random.Range(0, paths.Length)];
            anim.ObjectForwardAxis = forwardAxis;
            anim.ObjectUpAxis = upAxis;
            anim.Duration = Random.Range(durationRange.x, durationRange.y);
            anim.Loop = SplineAnimate.LoopMode.Loop;
            anim.NormalizedTime = Random.value;

            // 2. Modelo visual (uno de los prefabs al azar)
            GameObject prefab = flyPrefabs[Random.Range(0, flyPrefabs.Length)];
            GameObject visual = Instantiate(prefab, root.transform);
            visual.transform.localPosition = Random.insideUnitSphere * spread;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale *= Random.Range(scaleRange.x, scaleRange.y);

            // Si el prefab traía su propio SplineAnimate, se quita para no duplicar
            var oldAnim = visual.GetComponent<SplineAnimate>();
            if (oldAnim != null) Destroy(oldAnim);

            // 3. Ruido Perlin
            if (useNoise)
            {
                var wobble = visual.AddComponent<FlyWobble>();
                wobble.amplitude = Random.Range(noiseAmplitudeRange.x, noiseAmplitudeRange.y);
                wobble.frequency = Random.Range(noiseFrequencyRange.x, noiseFrequencyRange.y);
                wobble.rotationAmount = noiseRotation;
            }

            // 4. Aleteo desfasado
            var animator = visual.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.speed = Random.Range(0.9f, 1.15f);
                animator.Play(0, 0, Random.value);
            }

            anim.Play();
        }
    }
}