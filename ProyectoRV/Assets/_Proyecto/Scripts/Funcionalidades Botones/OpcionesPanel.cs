using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class OpcionesPanel : MonoBehaviour
{
    public Button botonCerrar;

    void Start()
    {
        if (botonCerrar) botonCerrar.onClick.AddListener(Cerrar);
    }

    void Update()
    {
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
}