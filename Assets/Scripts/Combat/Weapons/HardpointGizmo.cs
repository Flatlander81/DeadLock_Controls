using UnityEngine;

/// <summary>
/// Simple component to draw gizmos for weapon hardpoints in the editor.
/// Attach to hardpoint GameObjects to visualize them in the Scene view.
/// </summary>
public class HardpointGizmo : MonoBehaviour
{
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private float sphereRadius = 0.2f;
    [SerializeField] private float forwardLineLength = 0.5f;

    /// <summary>
    /// Draw gizmos when not selected.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Draw a small sphere at the hardpoint location
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);

        // Draw a line showing the forward direction (where weapons fire)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * forwardLineLength);
    }

    /// <summary>
    /// Draw gizmos when selected (larger and different color).
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw a larger sphere when selected
        Gizmos.color = selectedColor;
        Gizmos.DrawWireSphere(transform.position, sphereRadius * 1.5f);

        // Draw forward direction
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * forwardLineLength);

        // Draw coordinate axes
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 0.3f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.3f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.3f);
    }
}
