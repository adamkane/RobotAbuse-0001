using UnityEngine;

/// <summary>
/// Displays the robot arm attachment status using OnGUI.
/// Using OnGUI instead of UnityEngine.UI for reliable batchmode WebGL builds.
/// </summary>
public class RobotStatus : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("Font size for the status text")]
    public int fontSize = 32;

    [Tooltip("Color of the status text")]
    public Color textColor = Color.white;

    [Tooltip("Background color for the status box")]
    public Color backgroundColor = new Color(0, 0, 0, 0.5f);

    private string currentStatus = "Attached";
    private GUIStyle labelStyle;
    private GUIStyle boxStyle;
    private Texture2D backgroundTexture;

    void Start()
    {
        // Create background texture
        backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(0, 0, backgroundColor);
        backgroundTexture.Apply();
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

        // Calculate position (top center)
        float boxWidth = 250;
        float boxHeight = 50;
        float x = (Screen.width - boxWidth) / 2;
        float y = 20;

        Rect boxRect = new Rect(x, y, boxWidth, boxHeight);

        // Draw background box
        GUI.Box(boxRect, "", boxStyle);

        // Draw status text
        GUI.Label(boxRect, "Status: " + currentStatus, labelStyle);
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
