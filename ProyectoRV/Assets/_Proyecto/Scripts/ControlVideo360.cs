using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControlVideo360 : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject botonPlayPause;

    public Image imagenBoton;
    public Sprite spritePlay;
    public Sprite spritePause;

    void Start()
    {
        botonPlayPause.SetActive(false);
    }

    void Update()
    {
        // Actualizar el icono según el estado del vídeo
        imagenBoton.sprite = videoPlayer.isPlaying
            ? spritePause : spritePlay;

        // Mostrar u ocultar el botón
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

    public void AlternarPlayPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }
    }
}