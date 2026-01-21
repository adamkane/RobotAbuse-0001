using UnityEngine;

/// <summary>
/// Main robot controller handling torso highlight and drag functionality.
/// When the mouse hovers over the torso, the entire robot is highlighted.
/// Dragging the torso moves the entire robot.
/// </summary>
public class RobotController : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Tooltip("Color to tint the robot when highlighted")]
    public Color highlightColor = new Color(1f, 0.8f, 0.4f, 1f);

    [Tooltip("Original color of the robot")]
    public Color normalColor = Color.white;

    [Header("References")]
    [Tooltip("All renderers in the robot (auto-populated if empty)")]
    public Renderer[] allRenderers;

    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 dragOffset;
    private float dragDepth;
    private bool isHighlighted = false;

    // Material property block for efficient color changes
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        mainCamera = Camera.main;
        propBlock = new MaterialPropertyBlock();

        // Auto-populate renderers if not set
        if (allRenderers == null || allRenderers.Length == 0)
        {
            allRenderers = GetComponentsInChildren<Renderer>();
        }
    }

    void Update()
    {
        HandleHighlight();
        HandleDrag();
    }

    void HandleHighlight()
    {
        if (isDragging) return; // Don't change highlight while dragging

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool shouldHighlight = false;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if we hit the torso (this object or its collider)
            if (hit.collider.gameObject == gameObject ||
                hit.collider.transform.IsChildOf(transform))
            {
                // Only highlight if we hit the torso specifically
                if (hit.collider.CompareTag("Torso") ||
                    hit.collider.gameObject.name.Contains("Torso"))
                {
                    shouldHighlight = true;
                }
            }
        }

        if (shouldHighlight != isHighlighted)
        {
            isHighlighted = shouldHighlight;
            SetHighlight(isHighlighted);
        }
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0) && isHighlighted)
        {
            StartDrag();
        }

        if (isDragging)
        {
            if (Input.GetMouseButton(0))
            {
                ContinueDrag();
            }
            else
            {
                EndDrag();
            }
        }
    }

    void StartDrag()
    {
        isDragging = true;
        dragDepth = mainCamera.WorldToScreenPoint(transform.position).z;
        dragOffset = transform.position - GetMouseWorldPosition();
    }

    void ContinueDrag()
    {
        // Move the robot root to follow mouse
        transform.root.position = GetMouseWorldPosition() + dragOffset;
    }

    void EndDrag()
    {
        isDragging = false;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = dragDepth;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    /// <summary>
    /// Sets the highlight state for all robot renderers.
    /// </summary>
    public void SetHighlight(bool highlight)
    {
        Color color = highlight ? highlightColor : normalColor;

        foreach (var renderer in allRenderers)
        {
            if (renderer == null) continue;

            renderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", color);
            renderer.SetPropertyBlock(propBlock);
        }
    }

    /// <summary>
    /// Public property to check if robot is currently being dragged.
    /// </summary>
    public bool IsDragging => isDragging;
}
