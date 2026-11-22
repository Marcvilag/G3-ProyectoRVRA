using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicaAmbiente : MonoBehaviour
{
    [SerializeField] float duracionFundido = 3f;
    [SerializeField, Range(0f, 1f)] float volumenFinal = 1f;

    AudioSource fuente;

    void Awake()
    {
        fuente = GetComponent<AudioSource>();
        fuente.loop = true;
        fuente.volume = 0f;
    }

    void Start()
    {
        if (!fuente.isPlaying) fuente.Play();
        StartCoroutine(Fundir());
    }

    IEnumerator Fundir()
    {
        float t = 0f;
        while (t < duracionFundido)
        {
            t += Time.unscaledDeltaTime;
            fuente.volume = Mathf.Lerp(0f, volumenFinal, t / duracionFundido);
            yield return null;
        }
        fuente.volume = volumenFinal;
    }
}