using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class EventSystemSiHaceFalta : MonoBehaviour
{
    void Awake()
    {
        // Si la escena de debajo ya tiene uno, no hacemos nada
        if (FindFirstObjectByType<EventSystem>() != null) return;

        gameObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        gameObject.AddComponent<InputSystemUIInputModule>();
#else
        gameObject.AddComponent<StandaloneInputModule>();
#endif
    }
}