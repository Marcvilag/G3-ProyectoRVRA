using UnityEditor;
using UnityEngine;

// Herramienta de editor: quita los componentes con script perdido ("Missing Script")
// del objeto seleccionado y de todos sus hijos. No se incluye en el juego.
public static class QuitarScriptsPerdidos
{
    [MenuItem("Herramientas/Quitar scripts perdidos de la selección")]
    static void Quitar()
    {
        int total = 0;
        foreach (var raiz in Selection.gameObjects)
        {
            foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
            {
                int n = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                if (n == 0) continue;

                Undo.RegisterCompleteObjectUndo(t.gameObject, "Quitar scripts perdidos");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                Debug.Log($"Quitados {n} scripts perdidos de '{t.name}'", t.gameObject);
                total += n;
            }
        }
        Debug.Log($"Total: {total} scripts perdidos quitados.");
    }
}