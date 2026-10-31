using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class OpcionesPanel : MonoBehaviour
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
    public Button botonCerrar;
    public AudioClip sonidoPrueba;     // se oye al mover el deslizador de Efectos

    float proximaPrueba;

    void Awake()
    {
        // Si la escena que hay debajo ya tiene EventSystem, desactiva el de esta
        var sistemas = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if (sistemas.Length > 1)
            foreach (var es in sistemas)
                if (es.gameObject.scene == gameObject.scene) es.gameObject.SetActive(false);
    }

    void Start()
    {
        foreach (var f in filas)
        {
            var fila = f;
            fila.slider.minValue = 0f;
            fila.slider.maxValue = 1f;
            fila.slider.wholeNumbers = false;
            fila.slider.SetValueWithoutNotify(AjustesAudio.Obtener(fila.canal));
            ActualizarTexto(fila);

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
        if (botonCerrar) botonCerrar.onClick.AddListener(Cerrar);
    }

    void ActualizarTexto(Fila f)
    {
        if (f.porcentaje) f.porcentaje.text = Mathf.RoundToInt(f.slider.value * 100f) + " %";
    }

    void Update()
    {
        // Tecla Escape o botón "Atrás" de Android
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Cerrar();
#else
        if (Input.GetKeyDown(KeyCode.Escape)) Cerrar();
#endif
    }

    public void Cerrar()
    {
        AjustesAudio.Guardar();
        OpcionesEscena.Cerrar();
    }

    void OnDestroy() => AjustesAudio.Guardar();
}