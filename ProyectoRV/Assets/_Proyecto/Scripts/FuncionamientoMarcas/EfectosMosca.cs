using UnityEngine;
using System.Collections;

public class EfectosMosca : MonoBehaviour
{
    private Renderer miRenderer;
    private Color colorOriginal;

    void Start()
    {
        // Busca el Renderer en la mosca o en sus hijos (el modelo 3D)
        miRenderer = GetComponentInChildren<Renderer>();
        if (miRenderer != null)
        {
            colorOriginal = miRenderer.material.color;
        }
    }

    public void RecibirDano()
    {
        if (miRenderer != null)
        {
            StartCoroutine(ParpadearRojo());
        }
    }

    IEnumerator ParpadearRojo()
    {
        int parpadeos = 3;
        float velocidad = 0.15f;

        for (int i = 0; i < parpadeos; i++)
        {
            miRenderer.material.color = Color.red;
            yield return new WaitForSeconds(velocidad);
            miRenderer.material.color = colorOriginal;
            yield return new WaitForSeconds(velocidad);
        }
    }
}