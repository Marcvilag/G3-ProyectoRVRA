using UnityEngine;

public class DebugFPS : MonoBehaviour
{
    float dt;
    void Update() => dt += (Time.unscaledDeltaTime - dt) * 0.1f;
    void OnGUI()
    {
        GUI.skin.label.fontSize = 40;
        GUI.Label(new Rect(40, 40, 800, 100), $"{1f / dt:0} fps  |  t={Time.time:0.0}  |  frame={Time.frameCount}");
    }
}