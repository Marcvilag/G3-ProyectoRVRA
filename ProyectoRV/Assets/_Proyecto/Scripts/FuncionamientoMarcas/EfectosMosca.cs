using UnityEngine;
using System.Collections;

public class EfectosMosca : MonoBehaviour
{
    [Header("Cuerpo Visible de la Mosca")]
    public Renderer miRenderer;
    private Color[] coloresOriginales;

    [Header("Efectos de Sonido")]
    public AudioClip sonidoPositivo;
    public AudioClip sonidoNegativo;
    private AudioSource reproductor;

    void Start()
    {
        if (miRenderer != null)
        {
            coloresOriginales = new Color[miRenderer.materials.Length];
            for (int i = 0; i < miRenderer.materials.Length; i++)
            {
                coloresOriginales[i] = miRenderer.materials[i].color;
            }
            Debug.Log($"<color=green>[MOSCA INICIO]</color> Todo OK. Mosca encontró su Renderer con {miRenderer.materials.Length} materiales.");
        }
        else
        {
            Debug.LogError("<color=red>[MOSCA ERROR]</color> Faltó asignar 'Mi Renderer' en el Inspector de la mosca.");
        }

        reproductor = GetComponent<AudioSource>();
        if (reproductor == null) reproductor = gameObject.AddComponent<AudioSource>();
    }

    public void RecibirDano()
    {
        Debug.Log("<color=orange>[MOSCA AVISO]</color> ¡La mosca ha recibido la orden de RecibirDano() desde un objeto malo!");

        if (reproductor != null && sonidoNegativo != null) reproductor.PlayOneShot(sonidoNegativo);

        if (miRenderer != null)
        {
            Debug.Log("<color=red>[MOSCA EFECTO]</color> Iniciando la corrutina de parpadeo...");
            StartCoroutine(ParpadearRojo());
        }
        else
        {
            Debug.LogError("<color=red>[MOSCA FALLO]</color> No se puede parpadear porque miRenderer está vacío.");
        }
    }

    public void RecibirPositivo()
    {
        Debug.Log("<color=cyan>[MOSCA AVISO]</color> La mosca ha tocado un objeto bueno.");
        if (reproductor != null && sonidoPositivo != null) reproductor.PlayOneShot(sonidoPositivo);
    }

    IEnumerator ParpadearRojo()
    {
        int parpadeos = 3;
        float velocidad = 0.15f;

        for (int i = 0; i < parpadeos; i++)
        {
            Debug.Log($"<color=yellow>[MOSCA PARPADEO]</color> Cambiando a rojo. Ciclo {i + 1}");

            // Pintar de rojo
            for (int j = 0; j < miRenderer.materials.Length; j++)
            {
                miRenderer.materials[j].color = Color.red;
            }

            yield return new WaitForSeconds(velocidad);

            // Restaurar color
            for (int j = 0; j < miRenderer.materials.Length; j++)
            {
                miRenderer.materials[j].color = coloresOriginales[j];
            }

            yield return new WaitForSeconds(velocidad);
        }
        Debug.Log("<color=green>[MOSCA FIN]</color> El parpadeo ha terminado correctamente.");
    }
}