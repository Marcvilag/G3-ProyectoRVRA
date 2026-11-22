using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AjustesAudio : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] AudioMixer mixer;

    [Header("Sliders")]
    [SerializeField] Slider sliderMaster;
    [SerializeField] Slider sliderMusica;
    [SerializeField] Slider sliderEfectos;

    [Header("Depuración")]
    [SerializeField] bool mostrarLogs = true;

    const string ParamMaster = "MasterVol";
    const string ParamMusica = "MusicVol";
    const string ParamEfectos = "SFXVol";
    const float VolumenPorDefecto = 1f;
    const float VolumenMinimo = 0.0001f;

    void Start()
    {
        Configurar(sliderMaster, ParamMaster);
        Configurar(sliderMusica, ParamMusica);
        Configurar(sliderEfectos, ParamEfectos);

        StartCoroutine(AplicarTrasUnFrame());
    }

    // El mixer puede descartar SetFloat en los primeros frames (el Snapshot lo
    // sobrescribe), así que se aplica de nuevo tras esperar un poco.
    IEnumerator AplicarTrasUnFrame()
    {
        yield return null;
        AplicarTodos();

        yield return new WaitForSecondsRealtime(0.1f);
        AplicarTodos();
    }

    void AplicarTodos()
    {
        Aplicar(ParamMaster);
        Aplicar(ParamMusica);
        Aplicar(ParamEfectos);
    }

    void Aplicar(string parametro)
    {
        float v = PlayerPrefs.GetFloat(parametro, VolumenPorDefecto);
        bool ok = mixer.SetFloat(parametro, ADecibelios(v));

        if (mostrarLogs)
            Debug.Log($"[AjustesAudio] {parametro}: {v:F2} -> {ADecibelios(v):F1} dB, aplicado = {ok}");
    }

    void Configurar(Slider slider, string parametro)
    {
        slider.minValue = VolumenMinimo;
        slider.maxValue = 1f;

        // Coloca el slider en el valor guardado sin disparar el evento
        slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(parametro, VolumenPorDefecto));

        slider.onValueChanged.AddListener(v =>
        {
            mixer.SetFloat(parametro, ADecibelios(v));
            PlayerPrefs.SetFloat(parametro, v);
        });
    }

    static float ADecibelios(float v) =>
        Mathf.Log10(Mathf.Clamp(v, VolumenMinimo, 1f)) * 20f;

    void OnDisable() => PlayerPrefs.Save();

    // En Quest la app suele pausarse/cerrarse sin pasar por OnDisable
    void OnApplicationPause(bool pausa)
    {
        if (pausa) PlayerPrefs.Save();
    }
}