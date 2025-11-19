using UnityEngine;

/// <summary>
/// Manages individual ship units with turn-based movement planning and execution.
/// Implements BSG Deadlock-style projection-based movement system.
/// </summary>
public class Ship : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool verboseLogging = false;

    [Header("Movement Statistics")]
    [SerializeField] private float minMoveDistance = 5f;
    [SerializeField] private float maxMoveDistance = 20f;
    [SerializeField] private float maxTurnAngle = 45f;
    [SerializeField] private float executionRotationSpeed = 270f; // Degrees per second during movement execution

    [Header("Path Settings")]
    [SerializeField] private float arcHeight = 0.3f; // How much to arc the path (0.3 = 30% of distance)
    [SerializeField] private float straightStartLength = 3f; // How far ahead to place the start control point
    [SerializeField] private Color pathColor = Color.cyan;

    [Header("Visual Components")]
    [SerializeField] private GameObject projectionPrefab;
    [SerializeField] private Material normalProjectionMaterial;
    [SerializeField] private Material collisionProjectionMaterial;

    // Private state
    private GameObject projectionObject;
    private Renderer projectionRenderer;
    private bool isSelected = false;

    // Movement execution state
    private bool isExecutingMove = false;
    private Vector3 moveStartPosition;
    private Quaternion moveStartRotation;
    private float moveProgress = 0f;
    private float moveDuration = 3f; // Should match TurnManager.simulationDuration

    // Cubic Bezier control points
    private Vector3 startControlPoint; // First control point (ahead of ship for flat start)
    private Vector3 arcControlPoint;   // Second control point (offset for curve)
    private LineRenderer pathLineRenderer;

    // Public state properties
    public Vector3 PlannedPosition { get; private set; }
    public Quaternion PlannedRotation { get; private set; }
    public bool HasPlannedMove { get; private set; }
    public float MaxTurnAngle => maxTurnAngle;

    /// <summary>
    /// Initialize the ship and create its projection.
    /// </summary>
    private void Start()
    {
        CreateProjection();
        ResetPlannedMove();
    }

    /// <summary>
    /// Update handles smooth movement execution following a cubic Bezier arc.
    /// Using Update instead of FixedUpdate ensures smooth visual rendering.
    /// </summary>
    private void Update()
    {
        if (isExecutingMove)
        {
            moveProgress += Time.deltaTime;
            float t = Mathf.Clamp01(moveProgress / moveDuration);

            // Calculate position along cubic Bezier curve
            Vector3 newPosition = CubicBezier(moveStartPosition, startControlPoint, arcControlPoint, PlannedPosition, t);

            // Calculate tangent direction by looking ahead on the curve
            Vector3 tangent;
            if (t < 0.99f)
            {
                Vector3 futurePosition = CubicBezier(moveStartPosition, startControlPoint, arcControlPoint, PlannedPosition, Mathf.Min(t + 0.01f, 1f));
                tangent = (futurePosition - newPosition).normalized;
            }
            else
            {
                tangent = (PlannedPosition - newPosition).normalized;
            }

            // Apply position
            transform.position = newPosition;

            // Apply rotation with smooth interpolation
            if (tangent.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(tangent);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    executionRotationSpeed * Time.deltaTime
                );
            }

            // End execution when complete
            if (t >= 1f)
            {
                transform.position = PlannedPosition;
                transform.rotation = PlannedRotation;
                isExecutingMove = false;
                Debug.Log($"{gameObject.name} move complete at t={t:F2}");
            }
        }
    }

    /// <summary>
    /// Calculates a point on a cubic Bezier curve.
    /// </summary>
    private Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;

        return (uuu * p0) + (3 * uu * t * p1) + (3 * u * tt * p2) + (ttt * p3);
    }


    /// <summary>
    /// Creates the semi-transparent projection GameObject for movement planning.
    /// </summary>
    private void CreateProjection()
    {
        if (projectionPrefab != null)
        {
            projectionObject = Instantiate(projectionPrefab, transform.position, transform.rotation);

            // Get the renderer from the prefab
            projectionRenderer = projectionObject.GetComponent<Renderer>();

            // If materials aren't assigned in inspector, use the prefab's material
            if (normalProjectionMaterial == null && projectionRenderer != null)
            {
                normalProjectionMaterial = projectionRenderer.material;
            }

            // Create collision material if not assigned
            if (collisionProjectionMaterial == null && normalProjectionMaterial != null)
            {
                collisionProjectionMaterial = new Material(normalProjectionMaterial);
                collisionProjectionMaterial.color = new Color(1f, 0f, 0f, 0.5f); // Red with alpha
            }
        }
        else
        {
            // Create a default projection from the ship's mesh
            projectionObject = new GameObject("Projection_" + gameObject.name);

            // Copy mesh from this ship
            MeshFilter shipMesh = GetComponent<MeshFilter>();
            if (shipMesh != null)
            {
                MeshFilter projMesh = projectionObject.AddComponent<MeshFilter>();
                projMesh.mesh = shipMesh.mesh;

                MeshRenderer projRenderer = projectionObject.AddComponent<MeshRenderer>();

                // Create semi-transparent green material
                Material projMat = new Material(Shader.Find("Standard"));
                projMat.SetFloat("_Mode", 3); // Transparent mode
                projMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                projMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                projMat.SetInt("_ZWrite", 0);
                projMat.DisableKeyword("_ALPHATEST_ON");
                projMat.EnableKeyword("_ALPHABLEND_ON");
                projMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                projMat.renderQueue = 3000;
                projMat.color = new Color(0f, 1f, 0f, 0.5f); // Green with alpha

                projRenderer.material = projMat;
                normalProjectionMaterial = projMat;

                // Create red collision material
                Material collisionMat = new Material(projMat);
                collisionMat.color = new Color(1f, 0f, 0f, 0.5f); // Red with alpha
                collisionProjectionMaterial = collisionMat;
            }

            projectionObject.transform.localScale = transform.localScale * 0.9f;
            projectionRenderer = projectionObject.GetComponent<Renderer>();
        }

        // Don't set to Ignore Raycast - we need to be able to click on it!
        // projectionObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        projectionObject.SetActive(false);
    }

    /// <summary>
    /// Selects this ship, making its projection visible and highlighted.
    /// Positions projection in front of ship at half max distance if no move is planned.
    /// </summary>
    public void Select()
    {
        isSelected = true;

        // If no move has been planned yet, position projection in front of ship
        if (!HasPlannedMove)
        {
            // Calculate position: half max distance forward from ship
            float initialDistance = maxMoveDistance * 0.5f;
            Vector3 forwardOffset = transform.forward * initialDistance;
            Vector3 initialPosition = transform.position + forwardOffset;

            // Plan initial move at this position with current rotation
            PlanMove(initialPosition, transform.rotation);
        }

        if (projectionObject != null)
        {
            projectionObject.SetActive(true);
        }

        Debug.Log($"{gameObject.name} selected");
    }

    /// <summary>
    /// Deselects this ship. Projection remains visible if a move has been planned.
    /// </summary>
    public void Deselect()
    {
        isSelected = false;
        // Keep projection visible if there's a planned move
        // Only hide it if no move was planned
        if (projectionObject != null && !HasPlannedMove)
        {
            projectionObject.SetActive(false);
        }
        Debug.Log($"{gameObject.name} deselected");
    }

    /// <summary>
    /// Plans a movement for this ship, applying distance and rotation constraints.
    /// Calculates a Bezier arc control point for smooth curved movement.
    /// </summary>
    /// <param name="targetPos">Desired target position</param>
    /// <param name="targetRot">Desired target rotation</param>
    public void PlanMove(Vector3 targetPos, Quaternion targetRot)
    {
        // Calculate movement vector
        Vector3 moveVector = targetPos - transform.position;
        float distance = moveVector.magnitude;

        // Clamp distance between min and max
        if (distance > maxMoveDistance)
        {
            moveVector = moveVector.normalized * maxMoveDistance;
            targetPos = transform.position + moveVector;
            distance = maxMoveDistance;
        }
        else if (distance < minMoveDistance && distance > 0.01f)
        {
            moveVector = moveVector.normalized * minMoveDistance;
            targetPos = transform.position + moveVector;
            distance = minMoveDistance;
        }

        PlannedPosition = targetPos;
        HasPlannedMove = true;

        // Calculate Bezier arc control point first
        CalculateArcControlPoint();

        // Calculate the final rotation based on the Bezier curve's end tangent
        // This ensures the ship ends up facing the direction it's actually moving
        Vector3 nearEnd = CubicBezier(transform.position, startControlPoint, arcControlPoint, PlannedPosition, 0.99f);
        Vector3 endTangent = (PlannedPosition - nearEnd).normalized;

        if (endTangent.sqrMagnitude > 0.001f)
        {
            PlannedRotation = Quaternion.LookRotation(endTangent);
        }
        else
        {
            // Fallback to target rotation if we can't calculate tangent
            PlannedRotation = targetRot;
        }

        // Create path visualization
        CreatePathLineRenderer();

        UpdateProjectionPosition();
    }

    /// <summary>
    /// Calculates the control points for the cubic Bezier arc.
    /// First control point is ahead of ship for flat start.
    /// Second control point is offset for the curve.
    /// </summary>
    private void CalculateArcControlPoint()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = PlannedPosition;
        Vector3 startForward = transform.forward;

        // First control point: ahead of ship in its current forward direction
        // This creates a flat start tangent
        startControlPoint = startPos + startForward * straightStartLength;

        // Calculate midpoint between start and end for the arc control point
        Vector3 midpoint = (startPos + endPos) * 0.5f;

        // Calculate turn direction
        Vector3 movementDirection = (endPos - startPos).normalized;
        float signedAngle = Vector3.SignedAngle(startForward, movementDirection, Vector3.up);

        // Calculate perpendicular offset direction
        Vector3 perpendicular = Vector3.Cross(movementDirection, Vector3.up).normalized;

        // Arc height scales with turn angle and distance
        float moveDistance = Vector3.Distance(startPos, endPos);
        float arcAmount = Mathf.Abs(signedAngle) / 90f; // Normalize to 0-1 for 90 degree turn
        float offsetAmount = moveDistance * arcHeight * arcAmount;

        // Second control point: offset perpendicular to create the arc
        arcControlPoint = midpoint + perpendicular * Mathf.Sign(signedAngle) * offsetAmount;

        // Optional verbose logging (disabled by default for performance)
        if (verboseLogging)
        {
            Debug.Log($"{gameObject.name}: Cubic Bezier with straightStart={straightStartLength:F1}u, arc offset={offsetAmount:F2}u, turn={signedAngle:F1}°");
        }
    }

    /// <summary>
    /// Creates or updates the LineRenderer that visualizes the cubic Bezier arc path.
    /// Optimized to reuse existing LineRenderer instead of recreating every frame.
    /// </summary>
    private void CreatePathLineRenderer()
    {
        // Create LineRenderer if it doesn't exist
        if (pathLineRenderer == null)
        {
            GameObject lineObj = new GameObject($"Path_{gameObject.name}");
            pathLineRenderer = lineObj.AddComponent<LineRenderer>();

            // Try multiple shaders for compatibility across render pipelines
            Material lineMaterial = null;
            string[] shaderNames = { "Universal Render Pipeline/Unlit", "Unlit/Color", "Sprites/Default", "Standard" };

            foreach (string shaderName in shaderNames)
            {
                Shader shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    lineMaterial = new Material(shader);
                    break;
                }
            }

            if (lineMaterial == null)
            {
                lineMaterial = new Material(Shader.Find("Standard"));
            }

            // Configure LineRenderer (only once during creation)
            pathLineRenderer.material = lineMaterial;
            pathLineRenderer.startColor = pathColor;
            pathLineRenderer.endColor = pathColor;
            pathLineRenderer.startWidth = 0.3f;
            pathLineRenderer.endWidth = 0.3f;
            pathLineRenderer.useWorldSpace = true;
        }

        // Update the path curve (this happens every frame during drag, must be fast)
        // Reduced from 50 to 20 points for better performance - still looks smooth
        int curveResolution = 20;
        pathLineRenderer.positionCount = curveResolution + 1;

        for (int i = 0; i <= curveResolution; i++)
        {
            float t = i / (float)curveResolution;
            Vector3 point = CubicBezier(transform.position, startControlPoint, arcControlPoint, PlannedPosition, t);
            pathLineRenderer.SetPosition(i, point);
        }
    }

    /// <summary>
    /// Updates the projection GameObject to match the planned position and rotation.
    /// Sets rotation based on the cubic Bezier arc's end tangent direction.
    /// </summary>
    public void UpdateProjectionPosition()
    {
        if (projectionObject != null)
        {
            projectionObject.transform.position = PlannedPosition;

            // Calculate the tangent at the end of the cubic Bezier curve (t=0.99)
            Vector3 nearEnd = CubicBezier(transform.position, startControlPoint, arcControlPoint, PlannedPosition, 0.99f);
            Vector3 endTangent = (PlannedPosition - nearEnd).normalized;

            if (endTangent.sqrMagnitude > 0.001f)
            {
                projectionObject.transform.rotation = Quaternion.LookRotation(endTangent);
            }
            else
            {
                projectionObject.transform.rotation = PlannedRotation;
            }

            projectionObject.SetActive(true);
        }
    }

    /// <summary>
    /// Marks the projection with collision color based on collision detection.
    /// </summary>
    /// <param name="willCollide">True if collision is detected</param>
    public void MarkCollision(bool willCollide)
    {
        if (projectionRenderer != null)
        {
            if (willCollide)
            {
                projectionRenderer.material = collisionProjectionMaterial;
            }
            else
            {
                projectionRenderer.material = normalProjectionMaterial;
            }
        }
    }

    /// <summary>
    /// Executes the planned movement by starting smooth animation following Bezier arc to planned position.
    /// </summary>
    public void ExecuteMove()
    {
        if (HasPlannedMove)
        {
            // Store starting position and rotation for the arc
            moveStartPosition = transform.position;
            moveStartRotation = transform.rotation;
            moveProgress = 0f;
            isExecutingMove = true;

            float distance = Vector3.Distance(moveStartPosition, PlannedPosition);
            float angleDiff = Quaternion.Angle(moveStartRotation, PlannedRotation);
            Debug.Log($"{gameObject.name} executing: {distance:F1}u, {angleDiff:F1}° via Bezier arc");
        }
    }

    /// <summary>
    /// Resets the planned movement for a new turn.
    /// </summary>
    public void ResetPlannedMove()
    {
        PlannedPosition = transform.position;
        PlannedRotation = transform.rotation;
        HasPlannedMove = false;
        isExecutingMove = false;

        // Clean up line renderer
        if (pathLineRenderer != null)
        {
            Destroy(pathLineRenderer.gameObject);
            pathLineRenderer = null;
        }

        if (projectionObject != null && !isSelected)
        {
            projectionObject.SetActive(false);
        }

        // Reset collision marking
        MarkCollision(false);
    }

    /// <summary>
    /// Draws gizmos showing the cubic Bezier arc path and movement ranges in the Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (HasPlannedMove)
        {
            // Draw smooth cubic Bezier arc
            Gizmos.color = pathColor;
            int curveResolution = 50;
            Vector3 previousPoint = transform.position;

            for (int i = 1; i <= curveResolution; i++)
            {
                float t = i / (float)curveResolution;
                Vector3 currentPoint = CubicBezier(transform.position, startControlPoint, arcControlPoint, PlannedPosition, t);
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }

            // Draw control points
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(startControlPoint, 0.25f);
            Gizmos.DrawLine(transform.position, startControlPoint);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(arcControlPoint, 0.3f);

            // Draw destination sphere
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(PlannedPosition, 0.7f);

            // Wire spheres showing min/max movement range
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, minMoveDistance);

            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, maxMoveDistance);
        }
    }

    /// <summary>
    /// Gets the projection GameObject for raycasting purposes.
    /// </summary>
    public GameObject GetProjection()
    {
        return projectionObject;
    }
}
