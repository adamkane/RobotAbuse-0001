using UnityEngine;

/// <summary>
/// Controller for a detachable robot arm.
/// Handles highlighting when hovered, and detach/reattach via drag.
/// </summary>
public class ArmController : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = new Color(1f, 0.5f, 0.3f, 1f);
    public Color normalColor = Color.white;

    [Header("Detach Settings")]
    [Tooltip("Distance from original position to trigger detach")]
    public float detachDistance = 0.3f;

    [Tooltip("Distance from attach point to trigger reattach")]
    public float reattachDistance = 0.2f;

    [Header("References")]
    [Tooltip("Reference to the status display")]
    public RobotStatus statusUI;

    private Camera mainCamera;
    private Renderer[] armRenderers;
    private MaterialPropertyBlock propBlock;

    private bool isHighlighted = false;
    private bool isDragging = false;
    private bool isAttached = true;

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 attachWorldPosition;

    private float dragDepth;
    private Vector3 dragOffset;

    void Start()
    {
        mainCamera = Camera.main;
        propBlock = new MaterialPropertyBlock();
        armRenderers = GetComponentsInChildren<Renderer>();

        // Store original transform info
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        attachWorldPosition = transform.position;

        // Find status UI if not set
        if (statusUI == null)
        {
            statusUI = FindObjectOfType<RobotStatus>();
        }
    }

    void Update()
    {
        HandleHighlight();
        HandleDrag();
    }

    void HandleHighlight()
    {
        if (isDragging) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool shouldHighlight = false;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if we hit this arm or its children
            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
            {
                shouldHighlight = true;
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
        // Start drag on mouse down while highlighted
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
        Vector3 newPosition = GetMouseWorldPosition() + dragOffset;
        transform.position = newPosition;

        // Check for detach
        if (isAttached)
        {
            float distanceFromAttach = Vector3.Distance(transform.position, attachWorldPosition);
            if (distanceFromAttach > detachDistance)
            {
                Detach();
            }
        }
    }

    void EndDrag()
    {
        isDragging = false;

        // Check for reattach
        if (!isAttached)
        {
            // Update attach world position (robot may have moved)
            if (originalParent != null)
            {
                attachWorldPosition = originalParent.TransformPoint(originalLocalPosition);
            }

            float distanceFromAttach = Vector3.Distance(transform.position, attachWorldPosition);
            if (distanceFromAttach < reattachDistance)
            {
                Reattach();
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = dragDepth;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    void Detach()
    {
        isAttached = false;
        transform.SetParent(null);

        if (statusUI != null)
        {
            statusUI.SetStatus("Detached");
        }

        Debug.Log("Arm detached!");
    }

    void Reattach()
    {
        isAttached = true;
        transform.SetParent(originalParent);
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;

        if (statusUI != null)
        {
            statusUI.SetStatus("Attached");
        }

        Debug.Log("Arm reattached!");
    }

    void SetHighlight(bool highlight)
    {
        Color color = highlight ? highlightColor : normalColor;

        foreach (var renderer in armRenderers)
        {
            if (renderer == null) continue;

            renderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", color);
            renderer.SetPropertyBlock(propBlock);
        }
    }

    // Public accessors for testing
    public bool IsAttached => isAttached;
    public void SetAttached(bool attached) { isAttached = attached; }
}
