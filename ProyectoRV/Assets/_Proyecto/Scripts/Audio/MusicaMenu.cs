using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicaMenu : MonoBehaviour
{
    public static MusicaMenu Instancia;

    [System.Serializable]
    public class Pista
    {
        public string nombre;
        public AudioClip clip;
        public AudioMixerGroup grupo;          // Principal, Acomp_1 o Acomp_2
        public float retrasoInicio = 0f;       // segundos antes de empezar
        public bool inicioAleatorio = false;   // empieza en un punto al azar
        [HideInInspector] public AudioSource source;
        [HideInInspector] public bool iniciada;
        [HideInInspector] public bool arrancando;
    }

    [Header("Mixer")]
    public AudioMixer mixer;
    public string parametroMusica = "VolMusica";

    [Header("Pistas")]
    public Pista[] pistas;

    [Header("Escenas donde suena la música")]
    public MapaEscenas mapa;
    public Destino[] destinosConMusica = { Destino.Menu };

    [Header("Fundidos")]
    public float fadeIn = 3f;
    public float fadeOut = 1f;

    Coroutine fadeCo, pausaCo;
    float volumenMusicaActual = 0f;   // factor 0-1 sobre el nivel base
    
    bool musicaActiva;
    bool iniciado;

    void Awake()
    {
        if (Instancia != null) { Destroy(gameObject); return; }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += AlCargarEscena;


        foreach (var p in pistas)
        {
            if (!p.clip) continue;
            var s = gameObject.AddComponent<AudioSource>();
            s.clip = p.clip;
            s.outputAudioMixerGroup = p.grupo;
            s.loop = true;
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.volume = 1f;
            p.source = s;
        }
    }

    void OnDestroy()
    {
        if (Instancia == this)
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
            Instancia = null;
        }
    }

    void Start()
{
    if (Instancia != this) return;      // copia duplicada que se está destruyendo

    AjustesAudio.AplicarGuardados(mixer);
    StartCoroutine(ReaplicarTrasUnFrame());

    AplicarVolumenMusica(0f);
    iniciado = true;
    EvaluarEscena(SceneManager.GetActiveScene().name);
}

    IEnumerator ReaplicarTrasUnFrame()
    {
        yield return null;
        AjustesAudio.AplicarGuardados(mixer);
    }

    // ---------- Decidir según la escena ----------

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (!iniciado || modo != LoadSceneMode.Single) return;
        EvaluarEscena(escena.name);
    }

    bool EscenaConMusica(string nombreEscena)
    {
        if (!mapa) return true;             // sin mapa asignado, suena siempre
        foreach (var d in destinosConMusica)
            if (mapa.Nombre(d) == nombreEscena) return true;
        return false;
    }

    void EvaluarEscena(string nombreEscena)
    {
        if (EscenaConMusica(nombreEscena)) ActivarMusica();
        else DesactivarMusica();
    }

    void ActivarMusica()
    {
        musicaActiva = true;
        if (pausaCo != null) { StopCoroutine(pausaCo); pausaCo = null; }

        foreach (var p in pistas)
        {
            if (!p.source) continue;
            if (p.iniciada) p.source.UnPause();
            else if (!p.arrancando) StartCoroutine(Arrancar(p));
        }
        FadeMusica(1f, fadeIn);
    }

    void DesactivarMusica()
    {
        musicaActiva = false;
        if (pausaCo != null) StopCoroutine(pausaCo);
        pausaCo = StartCoroutine(SilenciarYPausar());
    }

    IEnumerator SilenciarYPausar()
    {
        FadeMusica(0f, fadeOut);
        yield return new WaitForSecondsRealtime(fadeOut);
        foreach (var p in pistas)
            if (p.source) p.source.Pause();
        pausaCo = null;
    }

    IEnumerator Arrancar(Pista p)
    {
        p.arrancando = true;
        if (p.retrasoInicio > 0f)
            yield return new WaitForSecondsRealtime(p.retrasoInicio);

        if (!musicaActiva) { p.arrancando = false; yield break; }   // te fuiste antes de que entrase

        if (p.inicioAleatorio)
            p.source.time = Random.Range(0f, p.clip.length * 0.9f);
        p.source.Play();
        p.iniciada = true;
        p.arrancando = false;
    }

    // ---------- Volumen ----------

    // factor 0 = silencio, 1 = el nivel que fijaste en el mixer
       void AplicarVolumenMusica(float factor)
   {
       volumenMusicaActual = factor;
       foreach (var p in pistas)
           if (p.source) p.source.volume = factor;
   }

    public void FadeMusica(float objetivo, float segundos)
    {
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeRoutine(objetivo, segundos));
    }

    IEnumerator FadeRoutine(float objetivo, float dur)
    {
        float inicio = volumenMusicaActual, t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / dur);
            AplicarVolumenMusica(Mathf.Lerp(inicio, objetivo, k));
            yield return null;
        }
        AplicarVolumenMusica(objetivo);
    }

    // Para botones de silencio o casos puntuales dentro de una escena con música
    public void Silenciar(float segundos = 1f) => FadeMusica(0f, segundos);
    public void Restaurar(float segundos = 1f) => FadeMusica(1f, segundos);
}