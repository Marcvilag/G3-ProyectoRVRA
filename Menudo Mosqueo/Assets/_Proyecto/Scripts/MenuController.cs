using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Métodos públicos para enlazar desde el OnClick de cada botón del menú.
/// Colócalo en un GameObject vacío de la escena (por ejemplo "MenuController").
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("Escenas (deben estar en File > Build Profiles > Scene List)")]
    [SerializeField] private string escenaJuego = "Juego";
    [SerializeField] private string escenaVideo360 = "Video360";
    [SerializeField] private string PruebasMenuVR = "PruebasMenuVR";

    [Header("Paneles (opcional)")]
    [Tooltip("Panel con los botones principales")]
    
    [Header("Opciones flotantes")]
    [SerializeField] private PanelOpcionesFlotante panelOpciones;

    public void Jugar() => SceneManager.LoadScene(escenaJuego);

    public void AbrirVideo360() => SceneManager.LoadScene(escenaVideo360);

    public void AbrirPruebaEscenario() => SceneManager.LoadScene("PruebaEscenario");
    public void AbrirPruebasMenuVR() => SceneManager.LoadScene(PruebasMenuVR);    
    public void AbrirOpciones()
    {
        if (panelOpciones != null) panelOpciones.Abrir();
    }

    public void VolverAlMenu()
    {
        if (panelOpciones != null) panelOpciones.Cerrar();
    }

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}