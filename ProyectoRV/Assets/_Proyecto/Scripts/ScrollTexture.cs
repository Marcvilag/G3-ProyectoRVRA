using UnityEngine;

public class ScrollTexture : MonoBehaviour
{
    public float speed = 0.01f;   // vueltas de textura por segundo
    Material mat;
    float offset;
    static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    void Awake()
    {
        mat = GetComponent<Renderer>().material;   // copia propia, no toca el asset
    }

    void Update()
    {
        offset = Mathf.Repeat(offset + speed * Time.deltaTime, 1f);
        mat.SetTextureOffset(BaseMap, new Vector2(offset, 0f));
    }

    void OnDestroy()
    {
        if (mat) Destroy(mat);
    }
}