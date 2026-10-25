using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    public GameObject[] cloudPrefabs;
    public int count = 12;
    public Vector2 scaleRange = new Vector2(0.8f, 1.5f);   // relativo al tamaño del prefab
    public Vector2 speedRange = new Vector2(0.2f, 0.6f);
    public int seed = 7;

    void Start()
    {
        // La zona es el propio cubo: su posición y su escala en el mundo
        Vector3 size = transform.lossyScale;
        var rng = new System.Random(seed);
        float R(float a, float b) => Mathf.Lerp(a, b, (float)rng.NextDouble());

        // Contenedor propio con escala 1, para que las nubes no hereden la del cubo
        var container = new GameObject("NubesGeneradas").transform;

        for (int i = 0; i < count; i++)
        {
            var prefab = cloudPrefabs[rng.Next(cloudPrefabs.Length)];
            var pos = transform.position + new Vector3(
                R(-size.x / 2, size.x / 2),
                R(-size.y / 2, size.y / 2),
                R(-size.z / 2, size.z / 2));

            var go = Instantiate(prefab, pos, Quaternion.Euler(0, R(0, 360), 0), container);
            go.transform.localScale *= R(scaleRange.x, scaleRange.y);

            var d = go.AddComponent<CloudDrift>();
            d.speed = R(speedRange.x, speedRange.y);
            d.minX = transform.position.x - size.x / 2;
            d.maxX = transform.position.x + size.x / 2;
        }
    }
}
public class CloudDrift : MonoBehaviour
{
    public float speed, minX, maxX;

    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;
        if (transform.position.x > maxX)
        {
            var p = transform.position; p.x = minX; transform.position = p;
        }
    }
}