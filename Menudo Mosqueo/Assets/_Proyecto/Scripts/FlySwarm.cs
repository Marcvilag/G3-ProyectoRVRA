using UnityEngine;
using UnityEngine.Splines;

public class FlySwarm : MonoBehaviour
{
    [Header("Moscas")]
    public float sizeFactor = 0.05f; 
    public GameObject[] flyPrefabs;
    public SplineContainer path;
    public int count = 8;
    public Vector2 scaleRange = new Vector2(0.9f, 1.1f);
    public Vector3 modelRotationOffset = Vector3.zero;

    [Header("Zumbido")]
    [SerializeField] AudioSource zumbido;
    [SerializeField] Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("Órbita")]
    [Tooltip("Segundos que tarda una mosca en dar una vuelta")]
    public Vector2 loopTimeRange = new Vector2(5f, 10f);
    [Range(0f, 1f)] public float reverseChance = 0.3f;   // proporción que vuela en sentido contrario

    [Header("Nube / orgánico")]
    public float spread = 0.2f;                          // dispersión alrededor del spline
    public Vector2 noiseAmplitudeRange = new Vector2(0.05f, 0.2f);
    public Vector2 noiseFrequencyRange = new Vector2(0.8f, 1.8f);

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject root = new GameObject("Fly_" + i);
            root.transform.SetParent(transform);

            var f = root.AddComponent<FlyPathFollower>();
            f.path = path;
            f.loopTime = Random.Range(loopTimeRange.x, loopTimeRange.y);
            f.phase = Random.value;
            f.direction = Random.value < reverseChance ? -1 : 1;
            f.fixedOffset = Random.insideUnitSphere * spread;
            f.noiseAmp = Random.Range(noiseAmplitudeRange.x, noiseAmplitudeRange.y);
            f.noiseFreq = Random.Range(noiseFrequencyRange.x, noiseFrequencyRange.y);

            GameObject prefab = flyPrefabs[Random.Range(0, flyPrefabs.Length)];
            GameObject visual = Instantiate(prefab, root.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(modelRotationOffset);
            visual.transform.localScale *= sizeFactor * Random.Range(scaleRange.x, scaleRange.y);
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

    void Update()
    {
        if (zumbido == null) return;
        float n = Mathf.PerlinNoise(Time.time * 0.5f, 0f);
        zumbido.pitch = Mathf.Lerp(pitchRange.x, pitchRange.y, n);
    }
}