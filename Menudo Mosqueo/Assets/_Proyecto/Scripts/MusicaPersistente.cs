using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicaPersistente : MonoBehaviour
{
    public static MusicaPersistente Instance { get; private set; }

    [Tooltip("Nombres exactos de las escenas donde la música debe seguir sonando")]
    [SerializeField] string[] escenasPermitidas = { "PruebasMenuVR", "PruebaEscenario" };
    [SerializeField] float duracionFundidoSalida = 1.5f;

    public MusicaDiaNoche Control { get; private set; }

    bool saliendo;

    void Awake()
    {
        if (Instance != null)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Control = GetComponent<MusicaDiaNoche>();
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
            Instance = null;
        }
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (saliendo) return;

        foreach (var nombre in escenasPermitidas)
            if (nombre == escena.name) return;   
        StartCoroutine(FundirYDestruir());
    }

    IEnumerator FundirYDestruir()
    {
        saliendo = true;
        float t = 0f;

        while (t < duracionFundidoSalida)
        {
            t += Time.unscaledDeltaTime;
            Control.SetMultiplicador(1f - t / duracionFundidoSalida);
            yield return null;
        }

        Destroy(gameObject);
    }
}