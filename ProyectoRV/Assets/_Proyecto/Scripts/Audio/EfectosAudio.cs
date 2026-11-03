using UnityEngine;
using UnityEngine.Audio;

public class EfectosAudio : MonoBehaviour
{
    public static EfectosAudio Instancia;

    public AudioMixerGroup grupoEfectos;
    public int voces = 4;               // sonidos que pueden solaparse

    AudioSource[] fuentes;
    int siguiente;

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(this); return; }
        Instancia = this;

        fuentes = new AudioSource[voces];
        for (int i = 0; i < voces; i++)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.ignoreListenerPause = true;
            s.loop = false;
            s.spatialBlend = 0f;                    // sonido 2D
            s.outputAudioMixerGroup = grupoEfectos;
            fuentes[i] = s;
        }
    }

    // Se llama desde cualquier script: EfectosAudio.Play(clip, volumen, pitch)
    public static void Play(AudioClip clip, float volumen = 1f, float pitch = 1f)
    {
        if (!clip) return;

        if (Instancia == null)
        {
            // Sin objeto de audio (por ejemplo, probando otra escena sola)
            AudioSource.PlayClipAtPoint(clip, Vector3.zero, volumen);
            return;
        }
        Instancia.Reproducir(clip, volumen, pitch);
    }

    void Reproducir(AudioClip clip, float volumen, float pitch)
    {
        var s = fuentes[siguiente];
        siguiente = (siguiente + 1) % fuentes.Length;
        s.pitch = pitch;
        s.PlayOneShot(clip, volumen);
    }
}