
using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class ControlVideo360 : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public TMP_Text textoBoton;

    public void AlternarPlayPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            textoBoton.text = "Reanudar";
        }
        else
        {
            videoPlayer.Play();
            textoBoton.text = "Pausar";
        }
    }
}
