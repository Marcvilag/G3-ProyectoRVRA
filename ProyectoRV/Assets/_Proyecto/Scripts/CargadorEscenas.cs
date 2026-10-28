using UnityEngine;
using UnityEngine.SceneManagement;

public static class CargadorEscenas
{
    static bool cargando;

    public static void Cargar(string nombre)
    {
        if (cargando) return;                       // evita doble pulsación

        if (string.IsNullOrEmpty(nombre))
        {
            Debug.LogError("CargadorEscenas: destino sin escena asignada en MapaEscenas.");
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(nombre))
        {
            Debug.LogError($"CargadorEscenas: la escena '{nombre}' no está en Build Settings.");
            return;
        }

        cargando = true;
        var op = SceneManager.LoadSceneAsync(nombre);
        op.completed += _ => cargando = false;
    }
}