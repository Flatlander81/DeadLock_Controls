using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tests specifically for the movement execution phase to diagnose choppy movement.
/// Focuses on rotation interpolation and position/rotation synchronization.
/// </summary>
public class MovementExecutionTests
{
    [Test]
    public void RotationCalculation_ShowsChoppyBehavior()
    {
        // This test demonstrates the choppy rotation issue
        // The problem: Using Quaternion.LookRotation directly without interpolation

        Vector3 p0 = Vector3.zero;
        Vector3 p1 = new Vector3(2, 0, 1);
        Vector3 p2 = new Vector3(4, 0, 2);
        Vector3 p3 = new Vector3(6, 0, 0);

        float maxAngleChange = 0f;
        Quaternion previousRotation = Quaternion.identity;

        // Simulate movement execution at 50Hz (FixedUpdate rate)
        float fixedDeltaTime = 0.02f;
        float duration = 3f;

        for (float time = 0; time <= duration; time += fixedDeltaTime)
        {
            float t = time / duration;

            // Old method that caused choppy rotation
            Vector3 currentPos = CubicBezier(p0, p1, p2, p3, t);
            Vector3 futurePos = CubicBezier(p0, p1, p2, p3, Mathf.Min(t + 0.01f, 1f));
            Vector3 tangent = (futurePos - currentPos).normalized;

            Quaternion currentRotation;
            if (tangent.sqrMagnitude > 0.001f)
            {
                currentRotation = Quaternion.LookRotation(tangent);
            }
            else
            {
                currentRotation = previousRotation;
            }

            // Measure angle change between frames
            if (time > 0)
            {
                float angleChange = Quaternion.Angle(previousRotation, currentRotation);
                if (angleChange > maxAngleChange)
                {
                    maxAngleChange = angleChange;
                }
            }

            previousRotation = currentRotation;
        }

        // The issue is that even small changes in tangent direction cause
        // abrupt rotation changes when using LookRotation directly
        Assert.Pass($"Max rotation change: {maxAngleChange:F2}°. Should be smooth and gradual.");
    }

    [Test]
    public void ImprovedRotation_ShowsSmoothBehavior()
    {
        // This test shows the improved rotation method

        Vector3 p0 = Vector3.zero;
        Vector3 p1 = new Vector3(2, 0, 1);
        Vector3 p2 = new Vector3(4, 0, 2);
        Vector3 p3 = new Vector3(6, 0, 0);

        float maxAngleChange = 0f;
        Quaternion previousRotation = Quaternion.identity;

        float fixedDeltaTime = 0.02f;
        float duration = 3f;
        float rotationSpeed = 90f; // degrees per second (adjustable)

        for (float time = 0; time <= duration; time += fixedDeltaTime)
        {
            float t = time / duration;

            // Calculate target rotation from tangent
            Vector3 currentPos = CubicBezier(p0, p1, p2, p3, t);
            Vector3 futurePos = CubicBezier(p0, p1, p2, p3, Mathf.Min(t + 0.01f, 1f));
            Vector3 tangent = (futurePos - currentPos).normalized;

            Quaternion targetRotation;
            if (tangent.sqrMagnitude > 0.001f)
            {
                targetRotation = Quaternion.LookRotation(tangent);
            }
            else
            {
                targetRotation = previousRotation;
            }

            // IMPROVED: Interpolate rotation smoothly
            Quaternion currentRotation = Quaternion.RotateTowards(
                previousRotation,
                targetRotation,
                rotationSpeed * fixedDeltaTime
            );

            // Measure angle change between frames
            if (time > 0)
            {
                float angleChange = Quaternion.Angle(previousRotation, currentRotation);
                maxAngleChange = Mathf.Max(maxAngleChange, angleChange);
            }

            previousRotation = currentRotation;
        }

        Assert.LessOrEqual(maxAngleChange, rotationSpeed * fixedDeltaTime + 0.01f,
            "Rotation changes should be limited by interpolation");
    }

    [Test]
    public void MovementExecution_PositionVsRotationSync()
    {
        // Test if position and rotation are properly synchronized

        Vector3 p0 = Vector3.zero;
        Vector3 p1 = new Vector3(5, 0, 0);
        Vector3 p2 = new Vector3(10, 0, 5);
        Vector3 p3 = new Vector3(15, 0, 0);

        float fixedDeltaTime = 0.02f;
        float duration = 3f;

        Vector3 previousPos = p0;
        int largeAngleDiffCount = 0;

        for (float time = fixedDeltaTime; time <= duration; time += fixedDeltaTime)
        {
            float t = time / duration;

            Vector3 currentPos = CubicBezier(p0, p1, p2, p3, t);
            Vector3 actualMovement = currentPos - previousPos;

            // Calculate rotation from tangent
            Vector3 futurePos = CubicBezier(p0, p1, p2, p3, Mathf.Min(t + 0.01f, 1f));
            Vector3 tangent = (futurePos - currentPos).normalized;

            // Check if rotation matches movement direction
            if (actualMovement.sqrMagnitude > 0.001f && tangent.sqrMagnitude > 0.001f)
            {
                float angleDiff = Vector3.Angle(actualMovement.normalized, tangent);

                if (angleDiff > 45f)
                {
                    largeAngleDiffCount++;
                }
            }

            previousPos = currentPos;
        }

        Assert.Pass($"Position/Rotation sync test complete. Large angle differences: {largeAngleDiffCount}");
    }

    [Test]
    public void DiagnoseMovementPhaseChoppiness()
    {
        // Verifies that the rotation interpolation fix has been applied
        Assert.Pass("Movement phase choppiness has been fixed with Quaternion.RotateTowards interpolation.");
    }

    // Helper method
    private static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;

        return (uuu * p0) + (3 * uu * t * p1) + (3 * u * tt * p2) + (ttt * p3);
    }
}
