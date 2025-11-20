using UnityEngine;

/// <summary>
/// Visual line from player ship to target, color-coded by weapon group.
/// Group 1: Blue, Group 2: Red, Group 3: Green, Group 4: Yellow
/// </summary>
public class TargetingLineRenderer : MonoBehaviour
{
    [Header("Color Coding")]
    private static readonly Color[] GroupColors = new Color[]
    {
        Color.white,    // Group 0 (unassigned) - not used
        Color.blue,     // Group 1
        Color.red,      // Group 2
        Color.green,    // Group 3
        Color.yellow    // Group 4
    };

    [Header("Runtime State")]
    private Ship sourceShip;
    private Ship targetShip;
    private int groupNumber;
    private LineRenderer lineRenderer;

    /// <summary>
    /// Initialize targeting line.
    /// </summary>
    public void Initialize(Ship source, Ship target, int group)
    {
        sourceShip = source;
        targetShip = target;
        groupNumber = Mathf.Clamp(group, 1, 4);

        // Create line renderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        // Set color based on group
        Color lineColor = GroupColors[groupNumber];
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // Create material
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Set sorting order to render on top
        lineRenderer.sortingOrder = 50;

        Debug.Log($"TargetingLine created: Group {groupNumber} ({lineColor}) from {source.gameObject.name} to {target.gameObject.name}");
    }

    /// <summary>
    /// Update line positions to follow ships.
    /// </summary>
    private void Update()
    {
        if (sourceShip == null || targetShip == null)
        {
            Destroy(gameObject);
            return;
        }

        // Update line positions
        lineRenderer.SetPosition(0, sourceShip.transform.position);
        lineRenderer.SetPosition(1, targetShip.transform.position);
    }

    /// <summary>
    /// Show/hide line.
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = visible;
        }
    }

    /// <summary>
    /// Get group color for UI display.
    /// </summary>
    public static Color GetGroupColor(int group)
    {
        int index = Mathf.Clamp(group, 0, 4);
        return GroupColors[index];
    }

    /// <summary>
    /// Get group name for UI display.
    /// </summary>
    public static string GetGroupName(int group)
    {
        if (group == 0) return "Unassigned";
        return $"Group {group}";
    }
}
