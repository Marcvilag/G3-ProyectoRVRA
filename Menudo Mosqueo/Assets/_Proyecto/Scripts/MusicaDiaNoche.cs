using System.Collections;
using UnityEngine;

public class MusicaDiaNoche : MonoBehaviour
{
    [SerializeField] AudioSource musicaDia;
    [SerializeField] AudioSource musicaNoche;
    [SerializeField, Range(0f, 1f)] float volumenMaximo = 1f;

    [Header("Fundido de entrada")]
    [SerializeField] float duracionEntrada = 3f;

    [Header("Efecto nocturno (aullido, búho...)")]
    [SerializeField] AudioSource efectoNoche;
    [SerializeField] float intervaloMin = 15f;
    [SerializeField] float intervaloMax = 40f;
    [SerializeField, Range(0f, 1f)] float umbralNoche = 0.8f; // desde qué progreso puede sonar
    [SerializeField, Range(0f, 1f)] float volumenEfecto = 0.7f;

    float entrada;
    float progreso;
    float multiplicador = 1f;

    void Start()
    {
        musicaDia.loop = musicaNoche.loop = true;
        musicaDia.Play();
        musicaNoche.Play();
        Aplicar();

        if (efectoNoche != null)
        {
            efectoNoche.loop = false;
            StartCoroutine(RutinaEfecto());
        }
    }

    void Update()
    {
        if (entrada < 1f)
        {
            entrada = Mathf.Min(1f, entrada + Time.unscaledDeltaTime / duracionEntrada);
            Aplicar();
        }
    }

    public void SetProgreso(float p)
    {
        progreso = p;
        Aplicar();
    }

    void Aplicar()
    {
        float angulo = progreso * Mathf.PI * 0.5f;
        musicaDia.volume = Mathf.Cos(angulo) * volumenMaximo * entrada * multiplicador;
        musicaNoche.volume = Mathf.Sin(angulo) * volumenMaximo * entrada * multiplicador;
    }

    IEnumerator RutinaEfecto()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(intervaloMin, intervaloMax));

            // Solo suena si ya es de noche y no se está reproduciendo ya
            if (progreso >= umbralNoche && !efectoNoche.isPlaying)
            {
                efectoNoche.volume = volumenEfecto;
                efectoNoche.pitch = Random.Range(0.95f, 1.05f);
                efectoNoche.Play();
            }
        }
    }

    // Si se hace de día mientras suena, se apaga con un fundido
    void LateUpdate()
    {
        if (efectoNoche != null && efectoNoche.isPlaying && progreso < umbralNoche)
            efectoNoche.volume = Mathf.MoveTowards(efectoNoche.volume, 0f, Time.deltaTime);
    }

    public void SetMultiplicador(float m)
    {
        multiplicador = Mathf.Clamp01(m);
        Aplicar();
    }
}