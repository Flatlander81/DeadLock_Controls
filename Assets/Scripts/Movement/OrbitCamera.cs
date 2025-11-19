using UnityEngine;

/// <summary>
/// Homeworld-style orbit camera that can focus on ships and orbit around them.
/// Supports mouse-based rotation, zooming, and smooth focus transitions.
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float orbitSpeed = 5f;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoomDistance = 5f;
    [SerializeField] private float maxZoomDistance = 50f;
    [SerializeField] private float focusTransitionSpeed = 5f;

    [Header("Input Settings")]
    [SerializeField] private float panSpeed = 0.5f;
    [SerializeField] private float keyboardPanSpeed = 10f;
    [SerializeField] private float keyboardOrbitSpeed = 90f;
    [SerializeField] private float keyboardZoomSpeed = 15f;

    [Header("Rotation Constraints")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    // Private state
    private Transform focusTarget;
    private Vector3 focusPoint;
    private float currentDistance = 20f;
    private float targetDistance = 20f;
    private float horizontalAngle = 0f;
    private float verticalAngle = 30f;

    // Smooth damping
    private Vector3 smoothFocusPoint;
    private Vector3 focusVelocity;

    private void Start()
    {
        // Initialize focus point
        if (focusTarget != null)
        {
            focusPoint = focusTarget.position;
        }
        else
        {
            focusPoint = transform.position + transform.forward * currentDistance;
        }
        smoothFocusPoint = focusPoint;

        // Initialize angles from current camera rotation
        Vector3 angles = transform.eulerAngles;
        horizontalAngle = angles.y;
        verticalAngle = angles.x;
        if (verticalAngle > 180f) verticalAngle -= 360f;
    }

    private void LateUpdate()
    {
        HandleInput();
        UpdateCameraPosition();
    }

    /// <summary>
    /// Handles all camera input including orbit, pan, and zoom.
    /// </summary>
    private void HandleInput()
    {
        // Zoom with mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetDistance -= scroll * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minZoomDistance, maxZoomDistance);
        }

        // Keyboard zoom (R to zoom in, F to zoom out)
        if (Input.GetKey(KeyCode.R))
        {
            targetDistance -= keyboardZoomSpeed * Time.deltaTime;
            targetDistance = Mathf.Clamp(targetDistance, minZoomDistance, maxZoomDistance);
        }
        if (Input.GetKey(KeyCode.F))
        {
            targetDistance += keyboardZoomSpeed * Time.deltaTime;
            targetDistance = Mathf.Clamp(targetDistance, minZoomDistance, maxZoomDistance);
        }

        // Orbit around focus point (Shift + Left Mouse drag)
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            horizontalAngle += mouseX * orbitSpeed;
            verticalAngle -= mouseY * orbitSpeed;

            // Clamp vertical angle to prevent flipping
            verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

            // Hide cursor during orbit
            Cursor.visible = false;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetMouseButtonUp(0))
        {
            Cursor.visible = true;
        }

        // Pan camera (Ctrl + Left Mouse drag) - moves focus point
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Calculate pan direction in camera space
            Vector3 right = transform.right;
            Vector3 up = Vector3.up;

            Vector3 panOffset = (-right * mouseX + -up * mouseY) * panSpeed * (currentDistance / 10f);
            focusPoint += panOffset;

            // When panning, detach from focus target
            focusTarget = null;

            Cursor.visible = false;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            Cursor.visible = true;
        }

        // Keyboard orbit (Q and E)
        float qeInput = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            qeInput = -1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            qeInput = 1f;
        }
        if (Mathf.Abs(qeInput) > 0.01f)
        {
            horizontalAngle += qeInput * keyboardOrbitSpeed * Time.deltaTime;
        }

        // Keyboard pan (WASD)
        Vector3 keyboardPan = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            keyboardPan += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            keyboardPan += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            keyboardPan += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            keyboardPan += Vector3.right;
        }

        if (keyboardPan.sqrMagnitude > 0.01f)
        {
            // Transform keyboard input to camera space
            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();

            Vector3 panOffset = (forward * keyboardPan.z + right * keyboardPan.x) * keyboardPanSpeed * Time.deltaTime;
            focusPoint += panOffset;

            // When panning, detach from focus target
            focusTarget = null;
        }
    }

    /// <summary>
    /// Updates the camera position based on focus point, angles, and distance.
    /// </summary>
    private void UpdateCameraPosition()
    {
        // Update focus point if we have a target
        if (focusTarget != null)
        {
            focusPoint = focusTarget.position;
        }

        // Smooth the focus point for fluid camera movement
        smoothFocusPoint = Vector3.SmoothDamp(smoothFocusPoint, focusPoint, ref focusVelocity, 1f / focusTransitionSpeed);

        // Smooth zoom
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 10f);

        // Calculate camera position using spherical coordinates
        Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0f);
        Vector3 offset = rotation * (Vector3.back * currentDistance);

        transform.position = smoothFocusPoint + offset;
        transform.LookAt(smoothFocusPoint);
    }

    /// <summary>
    /// Sets the camera to focus on a specific target.
    /// </summary>
    /// <param name="target">The transform to focus on</param>
    /// <param name="immediate">If true, snaps immediately; otherwise smoothly transitions</param>
    public void FocusOn(Transform target, bool immediate = false)
    {
        focusTarget = target;
        focusPoint = target.position;

        if (immediate)
        {
            smoothFocusPoint = focusPoint;
            focusVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Sets the camera to focus on a specific world position.
    /// </summary>
    /// <param name="position">The world position to focus on</param>
    /// <param name="immediate">If true, snaps immediately; otherwise smoothly transitions</param>
    public void FocusOn(Vector3 position, bool immediate = false)
    {
        focusTarget = null;
        focusPoint = position;

        if (immediate)
        {
            smoothFocusPoint = focusPoint;
            focusVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Clears the current focus target, allowing free camera movement.
    /// </summary>
    public void ClearFocus()
    {
        focusTarget = null;
    }

    /// <summary>
    /// Gets the current focus target (null if focusing on a world position).
    /// </summary>
    public Transform GetFocusTarget()
    {
        return focusTarget;
    }

    /// <summary>
    /// Sets the camera distance from the focus point.
    /// </summary>
    /// <param name="distance">The desired distance</param>
    public void SetDistance(float distance)
    {
        targetDistance = Mathf.Clamp(distance, minZoomDistance, maxZoomDistance);
    }
}
