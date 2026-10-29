using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using Vuforia;
using Image = UnityEngine.UI.Image;

/// <summary>
/// Pantalla "Vídeo tráiler" (AR).
/// - Al enfocar el marcador (ImageTarget) se proyecta el tráiler y se reproduce.
/// - Botón Play/Pause, texto con el tiempo y barra de tiempo (Slider) con cursor para desplazarse.
/// - Si se pierde el marcador, el vídeo se pausa y se reanuda al volver a verlo.
///
/// Colocar en el GameObject ImageTarget (o en un objeto "TrailerScreen" y asignar el target).
/// </summary>
public class TrailerVideoController : MonoBehaviour
{
    [Header("Vuforia")]
    [SerializeField] private ObserverBehaviour imageTarget;   // ImageTarget que aloja el vídeo

    [Header("Vídeo")]
    [SerializeField] private VideoPlayer videoPlayer;          // En el Quad hijo del ImageTarget
    [SerializeField] private GameObject videoSurface;          // Quad donde se proyecta (hijo del target)
    [SerializeField] private bool autoPlayOnTargetFound = true;

    [Header("UI (Canvas Screen Space - Overlay)")]
    [SerializeField] private GameObject controlsPanel;         // Contenedor de los controles
    [SerializeField] private Button playPauseButton;
    [SerializeField] private Image playPauseIcon;              // Icono del botón
    [SerializeField] private Sprite playSprite;                // PLACEHOLDER: sprite temporal
    [SerializeField] private Sprite pauseSprite;               // PLACEHOLDER: sprite temporal
    [SerializeField] private Slider timeline;                  // Línea de tiempo (el handle es el cursor)
    [SerializeField] private TMP_Text timeLabel;               // "00:00"

    private bool isDragging;
    private bool wasPlayingBeforeLost;
    private bool targetVisible;

    private void Awake()
    {
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.prepareCompleted += OnPrepared;

        playPauseButton.onClick.AddListener(TogglePlayPause);

        timeline.minValue = 0f;
        timeline.maxValue = 1f;
        timeline.wholeNumbers = false;
        timeline.onValueChanged.AddListener(OnTimelineChanged);
        AddDragEvents(timeline.gameObject);

        SetVisible(false);
    }

    private void OnEnable()
    {
        if (imageTarget != null)
            imageTarget.OnTargetStatusChanged += OnTargetStatusChanged;
    }

    private void OnDisable()
    {
        if (imageTarget != null)
            imageTarget.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    private void Start() => videoPlayer.Prepare();

    private void Update()
    {
        if (!videoPlayer.isPrepared || videoPlayer.length <= 0) return;

        if (!isDragging)
            timeline.SetValueWithoutNotify((float)(videoPlayer.time / videoPlayer.length));

        timeLabel.text = FormatTime(videoPlayer.time);
        RefreshIcon();
    }

    // ---------- Vuforia ----------
    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        bool found = status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;
        if (found == targetVisible) return;
        targetVisible = found;

        SetVisible(found);

        if (found)
        {
            if (autoPlayOnTargetFound || wasPlayingBeforeLost) videoPlayer.Play();
        }
        else
        {
            wasPlayingBeforeLost = videoPlayer.isPlaying;
            videoPlayer.Pause();
        }
    }

    private void SetVisible(bool visible)
    {
        if (videoSurface != null) videoSurface.SetActive(visible);
        if (controlsPanel != null) controlsPanel.SetActive(visible);
    }

    // ---------- Controles ----------
    public void TogglePlayPause()
    {
        if (videoPlayer.isPlaying) videoPlayer.Pause();
        else videoPlayer.Play();
        RefreshIcon();
    }

    private void OnTimelineChanged(float value)
    {
        if (!isDragging || !videoPlayer.canSetTime) return;
        videoPlayer.time = value * videoPlayer.length;
        timeLabel.text = FormatTime(videoPlayer.time);
    }

    private void OnPrepared(VideoPlayer vp)
    {
        timeline.SetValueWithoutNotify(0f);
        timeLabel.text = FormatTime(0);
    }

    private void RefreshIcon()
    {
        if (playPauseIcon == null) return;
        playPauseIcon.sprite = videoPlayer.isPlaying ? pauseSprite : playSprite;
    }

    // Detecta cuándo el usuario arrastra el cursor para no pelearse con Update()
    private void AddDragEvents(GameObject target)
    {
        var trigger = target.GetComponent<EventTrigger>() ?? target.AddComponent<EventTrigger>();

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener(_ => isDragging = true);
        trigger.triggers.Add(down);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener(_ =>
        {
            isDragging = false;
            if (videoPlayer.canSetTime) videoPlayer.time = timeline.value * videoPlayer.length;
        });
        trigger.triggers.Add(up);
    }

    private static string FormatTime(double seconds)
    {
        int s = Mathf.Max(0, Mathf.FloorToInt((float)seconds));
        return $"{s / 60:00}:{s % 60:00}";
    }
}