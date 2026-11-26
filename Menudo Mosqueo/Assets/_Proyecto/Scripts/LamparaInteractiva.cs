using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Random = System.Random;

public class LamparaInteractiva : MonoBehaviour
{
    [SerializeField] Light luz;
    [SerializeField] Renderer bombilla;
    [SerializeField] Color colorEmision = Color.white;

    [Header("Fundido")]
    [SerializeField] float duracionFundido = 0.5f;
    [SerializeField] float intensidadEncendida = 2f;

    [Header("Sonido")]
    [SerializeField] AudioSource audioSource;   // el SFXSource compartido
    [SerializeField] AudioClip clipEncender;
    [SerializeField] AudioClip clipApagar;      // opcional: si está vacío usa clipEncender
    [SerializeField, Range(0f, 0.2f)] float variacionPitch = 0.04f;


    [SerializeField] GameObject enjambre;

    public event Action alCambiar;
    public bool Encendida { get; private set; }

    XRSimpleInteractable interactable;
    Material matBombilla;
    float nivel;            // 0 = apagada, 1 = encendida
    Coroutine fundido;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(_ => Alternar());

        if (bombilla != null)
        {
            matBombilla = bombilla.material;
            matBombilla.EnableKeyword("_EMISSION");
        }

        AplicarNivel(0f);
    }

    public void Alternar()
{
    bool nuevoEstado = !Encendida;
    Poner(nuevoEstado, true);
    ReproducirSwitch(nuevoEstado);
}

    void ReproducirSwitch(bool encendida)
    {
        if (audioSource == null) return;

        AudioClip clip = encendida ? clipEncender : (clipApagar != null ? clipApagar : clipEncender);
        if (clip == null) return;

        audioSource.pitch = 1f + UnityEngine.Random.Range(-variacionPitch, variacionPitch);
        audioSource.PlayOneShot(clip);
    }

    // Se usa al inicio: apaga sin animación
    public void Apagar() => Poner(false, false);

    void Poner(bool encendida, bool animar)
    {
        Encendida = encendida;

        if (enjambre != null) enjambre.SetActive(encendida);

        if (fundido != null) StopCoroutine(fundido);

        if (animar)
            fundido = StartCoroutine(Fundir(encendida ? 1f : 0f));
        else
            AplicarNivel(encendida ? 1f : 0f);

        // Se avisa al gestor de inmediato para que empiece su transición a la vez
        alCambiar?.Invoke();
    }

    IEnumerator Fundir(float destino)
    {
        float inicio = nivel;
        float total = duracionFundido * Mathf.Abs(destino - inicio);
        float t = 0f;

        while (t < total)
        {
            t += Time.deltaTime;
            AplicarNivel(Mathf.Lerp(inicio, destino, t / total));
            yield return null;
        }

        AplicarNivel(destino);
        fundido = null;
    }

    void AplicarNivel(float n)
    {
        nivel = n;

        luz.intensity = intensidadEncendida * n;
        luz.enabled = n > 0.001f; // se desactiva del todo al apagarse, para ahorrar rendimiento

        if (matBombilla != null)
            matBombilla.SetColor("_EmissionColor", colorEmision * n);
    }
}