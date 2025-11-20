using UnityEngine;

/// <summary>
/// Visual effect for instant-hit weapons (rail guns, lasers).
/// No projectile travel time - creates tracer from start to end position.
/// Fades out and auto-destroys after short duration.
/// </summary>
public class InstantHitEffect : MonoBehaviour
{
    [Header("Effect Properties")]
    [SerializeField] private float fadeOutDuration = 0.2f; // Seconds to fade
    [SerializeField] private Color startColor = Color.cyan;
    [SerializeField] private float startWidth = 0.1f;
    [SerializeField] private float endWidth = 0.05f;

    private LineRenderer lineRenderer;
    private float currentAge = 0f;
    private bool isActive = false;

    // Public properties
    public float FadeOutDuration => fadeOutDuration;
    public bool IsActive => isActive;
    public float CurrentAge => currentAge;

    /// <summary>
    /// Initialize instant hit effect with start and end positions.
    /// </summary>
    public void Initialize(Vector3 startPosition, Vector3 endPosition)
    {
        // Get or create LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Configure line renderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);

        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;

        // Create simple material if none exists
        if (lineRenderer.material == null)
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        lineRenderer.startColor = startColor;
        lineRenderer.endColor = startColor;

        // Set to UI layer or high render order so it's visible
        lineRenderer.sortingOrder = 100;

        currentAge = 0f;
        isActive = true;

        Debug.Log($"InstantHitEffect created from {startPosition} to {endPosition}");
    }

    /// <summary>
    /// Update fade effect and destroy when complete.
    /// </summary>
    private void Update()
    {
        if (!isActive) return;

        currentAge += Time.deltaTime;

        // Calculate fade alpha (1.0 to 0.0)
        float alpha = 1f - (currentAge / fadeOutDuration);

        if (alpha <= 0f)
        {
            // Fade complete, destroy effect
            OnFadeComplete();
            return;
        }

        // Update line renderer alpha
        if (lineRenderer != null)
        {
            Color color = startColor;
            color.a = alpha;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            // Also fade width
            float widthMultiplier = alpha;
            lineRenderer.startWidth = startWidth * widthMultiplier;
            lineRenderer.endWidth = endWidth * widthMultiplier;
        }
    }

    /// <summary>
    /// Called when fade completes. Return to pool or destroy.
    /// </summary>
    private void OnFadeComplete()
    {
        isActive = false;

        // Return to pool instead of destroying
        ProjectileManager.ReturnInstantHitToPool(this);
    }

    /// <summary>
    /// Reset effect to pooled state.
    /// </summary>
    public void ResetToPool()
    {
        isActive = false;
        currentAge = 0f;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Re-enable line renderer when pulled from pool.
    /// </summary>
    private void OnEnable()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
    }
}
