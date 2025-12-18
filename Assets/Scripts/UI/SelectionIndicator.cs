using UnityEngine;

/// <summary>
/// Visual indicator showing which ship is selected.
/// Rotating ring that follows the selected ship.
/// Color: Cyan for enemy, Green for friendly.
/// </summary>
public class SelectionIndicator : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Color enemyColor = Color.cyan;
    [SerializeField] private Color friendlyColor = Color.green;
    [SerializeField] private float rotationSpeed = 45f;
    [SerializeField] private float ringRadius = 2f;
    [SerializeField] private float ringHeight = 0.5f;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private int ringSegments = 32;

    [Header("Runtime State")]
    private Ship targetShip;
    private bool isFriendly;
    private LineRenderer lineRenderer;

    /// <summary>
    /// Initialize the selection indicator.
    /// </summary>
    public void Initialize(Ship ship, bool friendly)
    {
        targetShip = ship;
        isFriendly = friendly;

        // Create line renderer for ring
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;

        // Set color based on friendly/enemy
        Color ringColor = isFriendly ? friendlyColor : enemyColor;
        lineRenderer.startColor = ringColor;
        lineRenderer.endColor = ringColor;

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        // Create material
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Create ring shape
        CreateRing();

        Debug.Log($"SelectionIndicator initialized for {ship.gameObject.name} ({(friendly ? "Friendly" : "Enemy")})");
    }

    /// <summary>
    /// Create circular ring shape.
    /// </summary>
    private void CreateRing()
    {
        lineRenderer.positionCount = ringSegments;

        float angleStep = 360f / ringSegments;

        for (int i = 0; i < ringSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * ringRadius;
            float z = Mathf.Sin(angle) * ringRadius;

            lineRenderer.SetPosition(i, new Vector3(x, ringHeight, z));
        }
    }

    /// <summary>
    /// Update indicator position and rotation.
    /// </summary>
    private void Update()
    {
        if (targetShip == null)
        {
            Destroy(gameObject);
            return;
        }

        // Follow ship position
        transform.position = targetShip.transform.position;

        // Rotate around Y axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Draw gizmo in Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (targetShip != null)
        {
            Gizmos.color = isFriendly ? friendlyColor : enemyColor;
            Gizmos.DrawWireSphere(targetShip.transform.position, ringRadius);
        }
    }
}
