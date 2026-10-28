using UnityEngine;

[RequireComponent(typeof(Boton3D))]
public class BotonSalir : MonoBehaviour
{
    void Awake() => GetComponent<Boton3D>().onClick.AddListener(Salir);

    void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // Application.Quit no hace nada en el editor
#else
        Application.Quit();
#endif
    }
}