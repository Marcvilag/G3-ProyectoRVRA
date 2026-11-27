using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Play/Pause del vídeo 360.
///  - VR: A/X (botón primario) alterna play/pause y muestra el icono unos segundos.
///        B/Y (botón secundario) vuelve al menú (MenuController.AbrirPruebasMenuVR).
///  - Editor/PC: se mantiene el comportamiento con ratón (clic para mostrar/ocultar
///        el botón y clic en el botón para alternar).
/// </summary>
public class ControlVideo360 : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject botonPlayPause;

    public Image imagenBoton;
    public Sprite spritePlay;
    public Sprite spritePause;

    [Header("Mandos VR")]
    [Tooltip("Botón que alterna play/pause. Sin bindings = A/X (botón primario).")]
    public InputAction accionPlayPause = new InputAction("PlayPause", InputActionType.Button);

    [Tooltip("Botón que vuelve al menú. Sin bindings = B/Y (botón secundario).")]
    public InputAction accionMenu = new InputAction("Menu", InputActionType.Button);

    [Tooltip("Se autocompleta con el MenuController del mismo GameObject.")]
    public MenuController menuController;

    [Tooltip("Segundos que el icono se queda visible tras pulsar el mando (0 = no se oculta).")]
    public float segundosIconoVisible = 1.5f;

    private Coroutine rutinaOcultar;

    void Awake()
    {
        // Bindings por defecto si no se han configurado en el Inspector
        if (accionPlayPause.bindings.Count == 0)
        {
            accionPlayPause.AddBinding("<XRController>{RightHand}/{PrimaryButton}");   // A
            accionPlayPause.AddBinding("<XRController>{LeftHand}/{PrimaryButton}");    // X
        }

        if (accionMenu.bindings.Count == 0)
        {
            accionMenu.AddBinding("<XRController>{RightHand}/{SecondaryButton}");      // B
            accionMenu.AddBinding("<XRController>{LeftHand}/{SecondaryButton}");       // Y
        }

        if (menuController == null) menuController = GetComponent<MenuController>();
    }

    void OnEnable()
    {
        accionPlayPause.performed += AlPulsarMando;
        accionPlayPause.Enable();

        accionMenu.performed += AlPulsarMenu;
        accionMenu.Enable();
    }

    void OnDisable()
    {
        accionPlayPause.performed -= AlPulsarMando;
        accionPlayPause.Disable();

        accionMenu.performed -= AlPulsarMenu;
        accionMenu.Disable();
    }

    void Start()
    {
        botonPlayPause.SetActive(false);
    }

    void Update()
    {
        // Actualizar el icono según el estado del vídeo
        imagenBoton.sprite = videoPlayer.isPlaying ? spritePause : spritePlay;

        // Ratón (editor/PC): mostrar u ocultar el botón
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            botonPlayPause.SetActive(!botonPlayPause.activeSelf);
        }
    }

    private void AlPulsarMando(InputAction.CallbackContext ctx)
    {
        AlternarPlayPause();
        MostrarIconoTemporal();
    }

    private void AlPulsarMenu(InputAction.CallbackContext ctx)
    {
        if (menuController != null)
            menuController.AbrirPruebasMenuVR();
        else
            Debug.LogWarning("ControlVideo360: falta asignar el MenuController.");
    }

    public void AlternarPlayPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            imagenBoton.sprite = spritePlay;
        }
        else
        {
            videoPlayer.Play();
            imagenBoton.sprite = spritePause;
        }
    }

    private void MostrarIconoTemporal()
    {
        botonPlayPause.SetActive(true);

        if (rutinaOcultar != null) StopCoroutine(rutinaOcultar);
        if (segundosIconoVisible > 0f)
            rutinaOcultar = StartCoroutine(OcultarTras(segundosIconoVisible));
    }

    private IEnumerator OcultarTras(float segundos)
    {
        yield return new WaitForSecondsRealtime(segundos);
        botonPlayPause.SetActive(false);
        rutinaOcultar = null;
    }
}