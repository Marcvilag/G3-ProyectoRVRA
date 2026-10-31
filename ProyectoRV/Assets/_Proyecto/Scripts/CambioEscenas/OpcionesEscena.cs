using UnityEngine;
using UnityEngine.SceneManagement;

public static class OpcionesEscena
{
    public const string Escena = "Opciones";   // el nombre de la escena debe ser exactamente este

    public static bool Abierta { get; private set; }
    static bool cargando;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reiniciar()
    {
        Abierta = false;
        cargando = false;
        SceneManager.sceneUnloaded -= AlDescargar;
        SceneManager.sceneUnloaded += AlDescargar;
    }

    // Si se carga otra escena "Single" con las opciones abiertas, se descargan solas
    static void AlDescargar(Scene s)
    {
        if (s.name == Escena) Abierta = false;
    }

    public static void Abrir()
    {
        if (Abierta || cargando) return;
        if (!Application.CanStreamedLevelBeLoaded(Escena))
        {
            Debug.LogError($"OpcionesEscena: la escena '{Escena}' no existe o no está en Build Settings.");
            return;
        }
        cargando = true;
        var op = SceneManager.LoadSceneAsync(Escena, LoadSceneMode.Additive);
        op.completed += _ => { cargando = false; Abierta = true; };
    }

    public static void Cerrar()
    {
        if (!Abierta) return;
        Abierta = false;
        SceneManager.UnloadSceneAsync(Escena);
    }
}