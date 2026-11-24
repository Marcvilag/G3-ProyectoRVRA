using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Al apuntarle crece un poco; al pulsar el gatillo carga el menú.
[RequireComponent(typeof(XRSimpleInteractable))]
public class BotonVRCambiarEscena : MonoBehaviour
{
    public string escena = "PruebasMenuVR";

    void Awake()
    {
        var boton = GetComponent<XRSimpleInteractable>();
        Vector3 escala = transform.localScale;

        boton.hoverEntered.AddListener(_ => transform.localScale = escala * 1.1f);
        boton.hoverExited.AddListener(_ => transform.localScale = escala);
        boton.selectEntered.AddListener(_ => SceneManager.LoadScene(escena));
    }
}