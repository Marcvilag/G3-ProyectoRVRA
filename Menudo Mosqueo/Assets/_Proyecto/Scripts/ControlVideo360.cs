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
///  - Al terminar el vídeo (sin bucle) se vuelve al menú automáticamente.
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

    [Header("Final del vídeo")]
    [Tooltip("Si está activo, el vídeo no se repite y al terminar vuelve al menú.")]
    public bool volverAlMenuAlTerminar = true;

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

        videoPlayer.loopPointReached += AlTerminarVideo;
    }

    void OnDisable()
    {
        accionPlayPause.performed -= AlPulsarMando;
        accionPlayPause.Disable();

        accionMenu.performed -= AlPulsarMenu;
        accionMenu.Disable();

        videoPlayer.loopPointReached -= AlTerminarVideo;
    }

    void Start()
    {
        botonPlayPause.SetActive(false);

        // Sin bucle: así el vídeo termina y salta loopPointReached
        if (volverAlMenuAlTerminar) videoPlayer.isLooping = false;
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

    private void AlTerminarVideo(VideoPlayer vp)
    {
        if (volverAlMenuAlTerminar) IrAlMenu();
    }

    private void AlPulsarMenu(InputAction.CallbackContext ctx) => IrAlMenu();

    private void IrAlMenu()
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
            PausarAudio(true);
            imagenBoton.sprite = spritePlay;
        }
        else
        {
            videoPlayer.Play();
            PausarAudio(false);
            imagenBoton.sprite = spritePause;
        }
    }

    // Pausa/reanuda los Audio Source asignados a las pistas del Video Player,
    // por si siguen sonando al pausar el vídeo.
    private void PausarAudio(bool pausar)
    {
        if (videoPlayer.audioOutputMode != VideoAudioOutputMode.AudioSource) return;

        for (ushort i = 0; i < videoPlayer.controlledAudioTrackCount; i++)
        {
            AudioSource fuente = videoPlayer.GetTargetAudioSource(i);
            if (fuente == null) continue;

            if (pausar) fuente.Pause();
            else fuente.UnPause();
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