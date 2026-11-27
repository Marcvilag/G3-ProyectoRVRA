using System.Collections;
using UnityEngine;

public class GestorLamparas : MonoBehaviour
{
    [SerializeField] LamparaInteractiva[] lamparas;

    [Header("Transición")]
    [SerializeField] float duracion = 2f;
    [SerializeField] AnimationCurve curva = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Skybox (mismo shader en ambos)")]
    [SerializeField] Material skyboxNormal;
    [SerializeField] Material skyboxNoche;

    [Header("Suelo")]
    [SerializeField] Renderer suelo;
    [SerializeField] Color colorSueloDia = new Color(0.2f, 0.6f, 0.2f);
    [SerializeField] Color colorSueloNoche = new Color(0.03f, 0.12f, 0.05f);

    [Header("Sol y luna: órbita y luces")]
    [SerializeField] Transform orbita;              // CieloOrbita
    [SerializeField] Light luzSol;
    [SerializeField] Light luzLuna;
    [SerializeField] float elevacionDia = 50f;      // grados sobre el horizonte
    [SerializeField] float elevacionNoche = -50f;   // el sol acaba bajo el horizonte
    [SerializeField] float azimut = 30f;            // dirección del recorrido
    [SerializeField] float intensidadSolDia = 1.2f;
    [SerializeField] float intensidadLunaNoche = 0.25f;

    [Header("Sol y luna: emisión (opcional)")]
    [SerializeField] Renderer rendererSol;
    [SerializeField] Renderer rendererLuna;
    [SerializeField, ColorUsage(false, true)] Color emisionSol = new Color(1f, 0.72f, 0.3f) * 3f;
    [SerializeField, ColorUsage(false, true)] Color emisionLuna = new Color(0.78f, 0.85f, 1f) * 1.5f;

    [Header("Música (respaldo si no hay instancia persistente)")]
    [SerializeField] MusicaDiaNoche musica;

    Material materialSuelo;
    Material skyboxRuntime;
    Material matSol;
    Material matLuna;
    string propiedadColor;
    float progreso;          // 0 = día, 1 = noche
    Coroutine transicion;
    Transform cam;

    // Siempre se pide a la instancia persistente: la copia de la escena puede estar destruida
    MusicaDiaNoche Musica =>
        MusicaPersistente.Instance != null ? MusicaPersistente.Instance.Control : musica;

    void Awake()
    {
        materialSuelo = suelo.material;   // instancia: no toca el material del proyecto
        propiedadColor = materialSuelo.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";

        skyboxRuntime = new Material(skyboxNormal);
        RenderSettings.skybox = skyboxRuntime;

        if (luzSol != null) RenderSettings.sun = luzSol;

        // Se cachean las instancias de material para no crear una por frame
        if (rendererSol != null)
        {
            matSol = rendererSol.material;
            matSol.EnableKeyword("_EMISSION");
        }
        if (rendererLuna != null)
        {
            matLuna = rendererLuna.material;
            matLuna.EnableKeyword("_EMISSION");
        }
    }

    void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : null;

        foreach (var l in lamparas)
            l.Apagar();

        Aplicar(0f);   // estado inicial: día, sin animación

        foreach (var l in lamparas)
            l.alCambiar += Comprobar;
    }

    // El pivote sigue la cabeza del jugador: sol y luna parecen infinitamente lejos
    void LateUpdate()
    {
        if (orbita != null && cam != null)
            orbita.position = cam.position;
    }

    void OnDestroy()
    {
        foreach (var l in lamparas)
            if (l != null) l.alCambiar -= Comprobar;

        if (skyboxRuntime != null) Destroy(skyboxRuntime);
        if (matSol != null) Destroy(matSol);
        if (matLuna != null) Destroy(matLuna);
    }

    void Comprobar()
    {
        bool noche = true;
        foreach (var l in lamparas)
            if (!l.Encendida) noche = false;

        if (transicion != null) StopCoroutine(transicion);
        transicion = StartCoroutine(Transicionar(noche ? 1f : 0f));
    }

    IEnumerator Transicionar(float destino)
    {
        float inicio = progreso;
        // si se interrumpe a mitad, la duración es proporcional a lo que falta
        float tiempoTotal = duracion * Mathf.Abs(destino - inicio);
        float tiempo = 0f;

        while (tiempo < tiempoTotal)
        {
            tiempo += Time.deltaTime;
            float t = curva.Evaluate(Mathf.Clamp01(tiempo / tiempoTotal));
            Aplicar(Mathf.Lerp(inicio, destino, t));
            yield return null;
        }

        Aplicar(destino);
        transicion = null;
    }

    void Aplicar(float p)
    {
        progreso = p;

        // Skybox
        skyboxRuntime.Lerp(skyboxNormal, skyboxNoche, p);
        DynamicGI.UpdateEnvironment();

        // Suelo
        materialSuelo.SetColor(propiedadColor, Color.Lerp(colorSueloDia, colorSueloNoche, p));

        // Sol y luna
        AplicarAstros(p);

        // Música
        var m = Musica;
        if (m != null) m.SetProgreso(p);
    }

    void AplicarAstros(float p)
    {
        if (orbita == null) return;

        // Elevación del sol de día a noche; la luna va justo opuesta (-elevación)
        float elevSol = Mathf.Lerp(elevacionDia, elevacionNoche, p);
        orbita.rotation = Quaternion.Euler(-elevSol, azimut, 0f);

        // Cada astro se atenúa al acercarse al horizonte y se apaga por debajo
        float fSol = Mathf.InverseLerp(-5f, 15f, elevSol);
        float fLuna = Mathf.InverseLerp(-5f, 15f, -elevSol);

        if (luzSol != null)
        {
            luzSol.intensity = intensidadSolDia * fSol;
            luzSol.enabled = fSol > 0.001f;
        }

        if (luzLuna != null)
        {
            luzLuna.intensity = intensidadLunaNoche * fLuna;
            luzLuna.enabled = fLuna > 0.001f;
        }

        if (matSol != null)
            matSol.SetColor("_EmissionColor", emisionSol * fSol);

        if (matLuna != null)
            matLuna.SetColor("_EmissionColor", emisionLuna * fLuna);
    }
}