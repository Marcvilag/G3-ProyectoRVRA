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

    [Header("Sol y luna")]
    [SerializeField] Transform orbita;          // CieloOrbita
    [SerializeField] Light luzSol;
    [SerializeField] Light luzLuna;
    [SerializeField] float elevacionDia = 50f;    // grados sobre el horizonte
    [SerializeField] float elevacionNoche = -50f; // el sol acaba bajo el horizonte
    [SerializeField] float azimut = 30f;          // dirección en la que sale el recorrido
    [SerializeField] float intensidadSolDia = 1.2f;
    [SerializeField] float intensidadLunaNoche = 0.25f;

    [Header("Música")]
    [SerializeField] MusicaDiaNoche musica;

    Material materialSuelo;
    Material skyboxRuntime;
    string propiedadColor;
    float progreso;          // 0 = día, 1 = noche
    Coroutine transicion;
    Transform cam;

    void Awake()
    {
        materialSuelo = suelo.material;
        propiedadColor = materialSuelo.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";

        skyboxRuntime = new Material(skyboxNormal);
        RenderSettings.skybox = skyboxRuntime;

        if (luzSol != null) RenderSettings.sun = luzSol;
    }

    void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : null;

        foreach (var l in lamparas)
            l.Apagar();

        Aplicar(0f);

        foreach (var l in lamparas)
            l.alCambiar += Comprobar;
    }

    // El pivote sigue la cabeza del jugador: así sol y luna parecen infinitamente lejos
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

        skyboxRuntime.Lerp(skyboxNormal, skyboxNoche, p);
        DynamicGI.UpdateEnvironment();

        materialSuelo.SetColor(propiedadColor,
            Color.Lerp(colorSueloDia, colorSueloNoche, p));

        AplicarAstros(p);

        if (musica != null) musica.SetProgreso(p);
    }

    void AplicarAstros(float p)
    {
        if (orbita == null) return;

        // Elevación del sol: de elevacionDia a elevacionNoche. La luna va justo opuesta (-elev)
        float elevSol = Mathf.Lerp(elevacionDia, elevacionNoche, p);
        orbita.rotation = Quaternion.Euler(-elevSol, azimut, 0f);

        // Cada luz se atenúa al acercarse al horizonte y se apaga por debajo
        if (luzSol != null)
        {
            float f = Mathf.InverseLerp(-5f, 15f, elevSol);
            luzSol.intensity = intensidadSolDia * f;
            luzSol.enabled = f > 0.001f;
        }

        if (luzLuna != null)
        {
            float f = Mathf.InverseLerp(-5f, 15f, -elevSol);
            luzLuna.intensity = intensidadLunaNoche * f;
            luzLuna.enabled = f > 0.001f;
        }
    }
}