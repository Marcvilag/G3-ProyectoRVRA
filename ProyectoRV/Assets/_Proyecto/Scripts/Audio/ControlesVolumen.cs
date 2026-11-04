using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class ControlesVolumen : MonoBehaviour
{
    [System.Serializable]
    public class Fila
    {
        public Canal canal;
        public Slider slider;
        public TMP_Text porcentaje;
    }

    public AudioMixer mixer;
    public Fila[] filas;
    public AudioClip sonidoPrueba;      // suena al mover el deslizador de Efectos

    float proximaPrueba;

    void Awake()
    {
        foreach (var f in filas)
        {
            var fila = f;
            fila.slider.minValue = 0f;
            fila.slider.maxValue = 1f;
            fila.slider.wholeNumbers = false;

            fila.slider.onValueChanged.AddListener(v =>
            {
                AjustesAudio.Establecer(mixer, fila.canal, v);
                ActualizarTexto(fila);

                if (fila.canal == Canal.Efectos && sonidoPrueba && Time.unscaledTime > proximaPrueba)
                {
                    EfectosAudio.Play(sonidoPrueba);
                    proximaPrueba = Time.unscaledTime + 0.25f;
                }
            });
        }
    }

    // Se refresca cada vez que el panel se activa, por si el valor cambió desde otro sitio
    void OnEnable() => Refrescar();

    public void Refrescar()
    {
        foreach (var f in filas)
        {
            f.slider.SetValueWithoutNotify(AjustesAudio.Obtener(f.canal));
            ActualizarTexto(f);
        }
    }

    void ActualizarTexto(Fila f)
    {
        if (f.porcentaje) f.porcentaje.text = Mathf.RoundToInt(f.slider.value * 100f) + " %";
    }

    void OnDisable() => AjustesAudio.Guardar();
}