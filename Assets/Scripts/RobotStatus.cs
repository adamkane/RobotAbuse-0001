using UnityEngine;

/// <summary>
/// Displays the robot arm attachment status using OnGUI.
/// Using OnGUI instead of UnityEngine.UI for reliable batchmode WebGL builds.
/// </summary>
public class RobotStatus : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("Font size for the status text")]
    public int fontSize = 24;

    [Tooltip("Color of the status text")]
    public Color textColor = new Color(0.1f, 0.1f, 0.1f);  // Dark gray for readability

    [Tooltip("Background color for the status box")]
    public Color backgroundColor = new Color(1f, 1f, 1f, 0.9f);  // White semi-transparent

    [Tooltip("Border/outline color")]
    public Color borderColor = new Color(0.3f, 0.3f, 0.3f);  // Medium gray border

    private string currentStatus = "Attached";
    private GUIStyle labelStyle;
    private GUIStyle boxStyle;
    private GUIStyle borderStyle;
    private GUIStyle fpsStyle;
    private Texture2D backgroundTexture;
    private Texture2D borderTexture;
    private Texture2D fpsBackgroundTexture;

    // FPS tracking
    private float deltaTime = 0f;

    void Start()
    {
        // Create background texture
        backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(0, 0, backgroundColor);
        backgroundTexture.Apply();

        // Create border texture
        borderTexture = new Texture2D(1, 1);
        borderTexture.SetPixel(0, 0, borderColor);
        borderTexture.Apply();

        // Create FPS background texture (semi-transparent dark)
        fpsBackgroundTexture = new Texture2D(1, 1);
        fpsBackgroundTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
        fpsBackgroundTexture.Apply();
    }

    void Update()
    {
        // Smooth FPS calculation
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        // Initialize styles if needed (can't do in Start because GUI not ready)
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = fontSize;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = textColor;
            labelStyle.alignment = TextAnchor.MiddleCenter;
        }

        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = backgroundTexture;
        }

        // Initialize border style
        if (borderStyle == null)
        {
            borderStyle = new GUIStyle(GUI.skin.box);
            borderStyle.normal.background = borderTexture;
        }

        // Calculate position (top-left corner to avoid head overlap)
        float boxWidth = 110;
        float boxHeight = 30;
        float x = 12;
        float y = 12;
        float borderWidth = 2;

        Rect borderRect = new Rect(x - borderWidth, y - borderWidth, boxWidth + borderWidth * 2, boxHeight + borderWidth * 2);
        Rect boxRect = new Rect(x, y, boxWidth, boxHeight);

        // Draw border then background
        GUI.Box(borderRect, "", borderStyle);
        GUI.Box(boxRect, "", boxStyle);

        // Draw status text (just the state, no "Status:" prefix)
        GUI.Label(boxRect, currentStatus, labelStyle);

        // Draw FPS in thin footer strip
        if (fpsStyle == null)
        {
            fpsStyle = new GUIStyle(GUI.skin.label);
            fpsStyle.fontSize = 12;
            fpsStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);  // Light gray
            fpsStyle.alignment = TextAnchor.MiddleRight;
            fpsStyle.padding = new RectOffset(0, 8, 0, 0);
        }

        float fps = 1.0f / deltaTime;
        float footerHeight = 18;
        Rect fpsRect = new Rect(0, Screen.height - footerHeight, Screen.width, footerHeight);
        GUI.Box(fpsRect, "", new GUIStyle { normal = { background = fpsBackgroundTexture } });
        GUI.Label(fpsRect, string.Format("{0:0} FPS", fps), fpsStyle);
    }

    /// <summary>
    /// Sets the current status to display.
    /// </summary>
    /// <param name="status">The status string ("Attached" or "Detached")</param>
    public void SetStatus(string status)
    {
        currentStatus = status;
    }

    /// <summary>
    /// Gets the current status.
    /// </summary>
    public string GetStatus()
    {
        return currentStatus;
    }
}
