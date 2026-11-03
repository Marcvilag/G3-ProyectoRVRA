using UnityEngine;
using TMPro;

public class PantallaPausa : MonoBehaviour
{
    [Header("Elementos (cada uno lleva un CanvasGroup)")]
    public CanvasGroup velo;
    public CanvasGroup menu;
    public TMP_Text textoCuentaAtras;

    [Header("Aspecto")]
    [Range(0f, 1f)] public float opacidadVelo = 0.35f;
    public float velocidadFade = 6f;
    public float escalaInicialMenu = 0.9f;

    GestorPausa gestor;

    void Awake()
    {
        Ocultar(velo); Ocultar(menu);
        if (textoCuentaAtras) textoCuentaAtras.gameObject.SetActive(false);
    }

    void Start()
    {
        gestor = GestorPausa.Instancia;
        if (!gestor)
        {
            Debug.LogError("PantallaPausa: no hay ningún GestorPausa en la escena.");
            enabled = false;
            return;
        }
        gestor.CuentaAtras += MostrarCuenta;
    }

    void OnDestroy()
    {
        if (gestor) gestor.CuentaAtras -= MostrarCuenta;
    }

    void Update()
    {
        bool pausado = gestor.Pausado;
        bool conMenu = pausado && !gestor.Reanudando;

        Fundir(velo, pausado ? opacidadVelo : 0f, pausado);   // el velo bloquea el HUD de debajo
        Fundir(menu, conMenu ? 1f : 0f, conMenu);

        if (menu)
            menu.transform.localScale = Vector3.one * Mathf.Lerp(escalaInicialMenu, 1f, menu.alpha);
    }

    // Para los botones de la UI
    public void Pausar()   { if (gestor) gestor.Pausar(); }
    public void Reanudar() { if (gestor) gestor.Reanudar(); }

    void MostrarCuenta(int n)
    {
        if (!textoCuentaAtras) return;
        textoCuentaAtras.gameObject.SetActive(n > 0);
        if (n > 0) textoCuentaAtras.text = n.ToString();
    }

    void Fundir(CanvasGroup g, float objetivo, bool recibeToques)
    {
        if (!g) return;
        g.alpha = Mathf.MoveTowards(g.alpha, objetivo, velocidadFade * Time.unscaledDeltaTime);
        g.blocksRaycasts = recibeToques;
        g.interactable = recibeToques;
    }

    static void Ocultar(CanvasGroup g)
    {
        if (!g) return;
        g.alpha = 0f;
        g.blocksRaycasts = false;
        g.interactable = false;
    }
}