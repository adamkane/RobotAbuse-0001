using UnityEngine;

/// <summary>
/// Orbital camera controller that allows viewing the robot from different angles.
/// Supports WASD/Arrow keys for orbit and mouse scroll for zoom.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The transform to orbit around (usually the robot)")]
    public Transform target;

    [Header("Orbit Settings")]
    [Tooltip("Speed of horizontal/vertical orbit rotation")]
    public float orbitSpeed = 100f;

    [Tooltip("Minimum vertical angle (prevents looking from below)")]
    public float minVerticalAngle = -20f;

    [Tooltip("Maximum vertical angle (prevents looking from above)")]
    public float maxVerticalAngle = 80f;

    [Header("Zoom Settings")]
    [Tooltip("Speed of zoom via scroll wheel")]
    public float zoomSpeed = 5f;

    [Tooltip("Minimum distance from target")]
    public float minDistance = 0.5f;

    [Tooltip("Maximum distance from target")]
    public float maxDistance = 5f;

    [Header("Debug")]
    [Tooltip("Press G to toggle debug display")]
    public bool showDebug = true;  // Enabled by default for QA

    private float currentDistance;
    private float currentYaw;
    private float currentPitch;

    void Start()
    {
        if (target == null)
        {
            // Try to find the robot automatically
            var robot = GameObject.Find("Robot_Toy");
            if (robot != null)
                target = robot.transform;
        }

        // Initialize from current position
        if (target != null)
        {
            Vector3 direction = transform.position - target.position;
            currentDistance = direction.magnitude;
            currentYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            currentPitch = Mathf.Asin(direction.y / currentDistance) * Mathf.Rad2Deg;
        }
        else
        {
            currentDistance = 0.4f;  // Much closer for 80% fill
            currentYaw = 15f;        // Slight angle
            currentPitch = 5f;
        }
    }

    void Update()
    {
        // Toggle debug with 'G' key (D conflicts with movement)
        if (Input.GetKeyDown(KeyCode.G))
        {
            showDebug = !showDebug;
        }

        if (target == null) return;

        // Get input for orbit (WASD or Arrow keys)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Update yaw and pitch
        currentYaw += horizontal * orbitSpeed * Time.deltaTime;
        currentPitch -= vertical * orbitSpeed * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= scroll * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        // Calculate new position
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -currentDistance);
        transform.position = target.position + offset;

        // Look at target
        transform.LookAt(target);
    }

    void OnGUI()
    {
        if (!showDebug) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 10;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.yellow;

        float w = 220, h = 140;
        Rect rect = new Rect(Screen.width - w - 10, 10, w, h);

        // Find robot head for reference
        Transform head = null;
        if (target != null)
        {
            head = target.Find("Robot_Head");
            if (head == null)
            {
                foreach (Transform child in target.GetComponentsInChildren<Transform>())
                {
                    if (child.name.Contains("Head")) { head = child; break; }
                }
            }
        }

        string debugText = string.Format(
            "=== Camera Debug [G] ===\n" +
            "Cam Pos: {0:F2}, {1:F2}, {2:F2}\n" +
            "Distance: {3:F2}\n" +
            "Yaw: {4:F1}° Pitch: {5:F1}°\n" +
            "FOV: {6:F0}°\n" +
            "---\n" +
            "Robot Origin: {7:F2}, {8:F2}, {9:F2}\n" +
            "Head Pos: {10}",
            transform.position.x, transform.position.y, transform.position.z,
            currentDistance,
            currentYaw, currentPitch,
            Camera.main != null ? Camera.main.fieldOfView : 0,
            target != null ? target.position.x : 0,
            target != null ? target.position.y : 0,
            target != null ? target.position.z : 0,
            head != null ? string.Format("{0:F2}, {1:F2}, {2:F2}", head.position.x, head.position.y, head.position.z) : "not found"
        );

        GUI.Box(rect, debugText, style);
    }

    // Draw origin marker in scene view
    void OnDrawGizmos()
    {
        if (target != null)
        {
            // Draw robot origin
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, 0.05f);
            Gizmos.DrawLine(target.position, target.position + Vector3.up * 0.3f);
        }
    }
}
