using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class GrassScatter : MonoBehaviour
{
    [Header("Qué dibujar")]
    public GameObject grassPrefab;          // el prefab de hierba tal cual

    [Header("Zona (centrada en este objeto, a su altura)")]
    public Vector2 area = new Vector2(40, 30);
    public float spacing = 0.5f;
    public int seed = 12345;

    [Header("Variación")]
    public Vector2 scaleRange = new Vector2(0.7f, 1.3f);
    public float heightMultiplier = 0.5f;   // 1 = altura original, 0.5 = la mitad
    public float patchNoiseScale = 0.12f;
    [Range(0, 1)] public float patchThreshold = 0.35f;

    [Header("Zonas sin hierba")]
    public Transform[] exclusions;
    public float exclusionRadius = 1.5f;

    class Part
    {
        public Mesh mesh; public int sub; public RenderParams rp; public Matrix4x4 local;
    }

    List<Part> parts = new List<Part>();
    Matrix4x4[] instances;
    Matrix4x4[][] buffers;

    void Start()
    {
        if (!grassPrefab) { Debug.LogError("GrassScatter: falta el prefab de hierba.", this); return; }

        // 1. Extraer mallas, materiales y su transformación relativa dentro del prefab
        var root = grassPrefab.transform;
        foreach (var mf in grassPrefab.GetComponentsInChildren<MeshFilter>())
        {
            var mr = mf.GetComponent<MeshRenderer>();
            if (!mr || !mf.sharedMesh) continue;

            Matrix4x4 local = Matrix4x4.identity;
            for (var t = mf.transform; t != root && t != null; t = t.parent)
                local = Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale) * local;

            var mats = mr.sharedMaterials;
            for (int i = 0; i < mats.Length && i < mf.sharedMesh.subMeshCount; i++)
            {
                if (!mats[i]) continue;
                var mat = new Material(mats[i]) { enableInstancing = true };
                parts.Add(new Part
                {
                    mesh = mf.sharedMesh, sub = i, local = local,
                    rp = new RenderParams(mat) { shadowCastingMode = ShadowCastingMode.Off, receiveShadows = true }
                });
            }
        }
        if (parts.Count == 0) { Debug.LogError("GrassScatter: el prefab no tiene MeshFilter/MeshRenderer.", this); return; }

        // 2. Repartir las matas
        var rng = new System.Random(seed);
        var list = new List<Matrix4x4>();
        Vector3 c = transform.position;

        for (float x = -area.x / 2; x < area.x / 2; x += spacing)
        for (float z = -area.y / 2; z < area.y / 2; z += spacing)
        {
            float px = c.x + x + (float)rng.NextDouble() * spacing;
            float pz = c.z + z + (float)rng.NextDouble() * spacing;

            if (Mathf.PerlinNoise(px * patchNoiseScale + 100, pz * patchNoiseScale + 100) < patchThreshold) continue;
            if (IsExcluded(px, pz)) continue;

            float s = Mathf.Lerp(scaleRange.x, scaleRange.y, (float)rng.NextDouble());
            var rot = Quaternion.Euler(0, (float)rng.NextDouble() * 360f, 0);
            list.Add(Matrix4x4.TRS(new Vector3(px, c.y, pz), rot,
            new Vector3(s, s * heightMultiplier, s)));
        }
        instances = list.ToArray();

        // 3. Una matriz final por parte e instancia (instancia * transformación local de la parte)
        buffers = new Matrix4x4[parts.Count][];
        for (int p = 0; p < parts.Count; p++)
        {
            buffers[p] = new Matrix4x4[instances.Length];
            for (int i = 0; i < instances.Length; i++)
                buffers[p][i] = instances[i] * parts[p].local;
        }
        Debug.Log($"GrassScatter: {instances.Length} matas, {parts.Count} partes.");
    }

    bool IsExcluded(float x, float z)
    {
        foreach (var t in exclusions)
        {
            if (!t) continue;
            float dx = x - t.position.x, dz = z - t.position.z;
            if (dx * dx + dz * dz < exclusionRadius * exclusionRadius) return true;
        }
        return false;
    }

    void Update()
    {
        if (buffers == null) return;
        for (int p = 0; p < parts.Count; p++)
            for (int i = 0; i < buffers[p].Length; i += 1000)
                Graphics.RenderMeshInstanced(parts[p].rp, parts[p].mesh, parts[p].sub,
                    buffers[p], Mathf.Min(1000, buffers[p].Length - i), i);
    }

    void OnDrawGizmosSelected() =>
        Gizmos.DrawWireCube(transform.position, new Vector3(area.x, 0.2f, area.y));
}