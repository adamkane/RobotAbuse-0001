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
}
