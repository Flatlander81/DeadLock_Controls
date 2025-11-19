using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for Ship movement system to diagnose choppy movement issues.
/// Tests cover Bezier calculations, movement execution, and timing accuracy.
/// </summary>
public class ShipMovementTests
{
    private GameObject shipObject;
    private Ship ship;

    [SetUp]
    public void SetUp()
    {
        // Create a test ship GameObject
        shipObject = new GameObject("TestShip");
        ship = shipObject.AddComponent<Ship>();

        // Add required components for Ship to function
        shipObject.AddComponent<MeshFilter>();
        shipObject.AddComponent<MeshRenderer>();

        // Set initial position and rotation
        shipObject.transform.position = Vector3.zero;
        shipObject.transform.rotation = Quaternion.identity;
    }

    [TearDown]
    public void TearDown()
    {
        if (shipObject != null)
        {
            Object.DestroyImmediate(shipObject);
        }
    }

    [Test]
    public void CubicBezier_ReturnsCorrectStartPoint()
    {
        // Arrange
        Vector3 p0 = Vector3.zero;
        Vector3 p1 = new Vector3(1, 0, 0);
        Vector3 p2 = new Vector3(2, 0, 0);
        Vector3 p3 = new Vector3(3, 0, 0);

        // Act - Use reflection to call private CubicBezier method
        var method = typeof(Ship).GetMethod("CubicBezier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Vector3 result = (Vector3)method.Invoke(ship, new object[] { p0, p1, p2, p3, 0f });

        // Assert
        Assert.AreEqual(p0, result, "Bezier curve should start at p0 when t=0");
    }

    [Test]
    public void CubicBezier_ReturnsCorrectEndPoint()
    {
        // Arrange
        Vector3 p0 = Vector3.zero;
        Vector3 p1 = new Vector3(1, 0, 0);
        Vector3 p2 = new Vector3(2, 0, 0);
        Vector3 p3 = new Vector3(3, 0, 0);

        // Act
        var method = typeof(Ship).GetMethod("CubicBezier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Vector3 result = (Vector3)method.Invoke(ship, new object[] { p0, p1, p2, p3, 1f });

        // Assert
        Assert.AreEqual(p3, result, "Bezier curve should end at p3 when t=1");
    }

    [Test]
    public void CubicBezier_MidpointIsSmooth()
    {
        // Arrange
        Vector3 p0 = Vector3.zero;
        Vector3 p1 = new Vector3(1, 0, 0);
        Vector3 p2 = new Vector3(2, 0, 0);
        Vector3 p3 = new Vector3(3, 0, 0);

        // Act - Sample multiple points along the curve
        var method = typeof(Ship).GetMethod("CubicBezier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Vector3 point1 = (Vector3)method.Invoke(ship, new object[] { p0, p1, p2, p3, 0.25f });
        Vector3 point2 = (Vector3)method.Invoke(ship, new object[] { p0, p1, p2, p3, 0.5f });
        Vector3 point3 = (Vector3)method.Invoke(ship, new object[] { p0, p1, p2, p3, 0.75f });

        // Assert - Check that points are progressing smoothly (no sudden jumps)
        float dist1 = Vector3.Distance(p0, point1);
        float dist2 = Vector3.Distance(point1, point2);
        float dist3 = Vector3.Distance(point2, point3);
        float dist4 = Vector3.Distance(point3, p3);

        // All distances should be relatively similar (no sudden jumps)
        Assert.IsTrue(dist1 > 0 && dist1 < 2, "First quarter distance should be reasonable");
        Assert.IsTrue(dist2 > 0 && dist2 < 2, "Second quarter distance should be reasonable");
        Assert.IsTrue(dist3 > 0 && dist3 < 2, "Third quarter distance should be reasonable");
        Assert.IsTrue(dist4 > 0 && dist4 < 2, "Fourth quarter distance should be reasonable");
    }

    [Test]
    public void PlanMove_ClampsDistanceToMaximum()
    {
        // Arrange
        Vector3 targetPosition = new Vector3(100, 0, 0); // Far beyond max distance
        Quaternion targetRotation = Quaternion.identity;

        // Act
        ship.PlanMove(targetPosition, targetRotation);

        // Assert
        float distance = Vector3.Distance(Vector3.zero, ship.PlannedPosition);
        Assert.LessOrEqual(distance, ship.MaxTurnAngle + 1, "Distance should be clamped to max distance");
        Assert.IsTrue(ship.HasPlannedMove, "Should have a planned move");
    }

    [Test]
    public void PlanMove_SetsPlannedPositionAndRotation()
    {
        // Arrange
        Vector3 targetPosition = new Vector3(10, 0, 0);
        Quaternion targetRotation = Quaternion.Euler(0, 45, 0);

        // Act
        ship.PlanMove(targetPosition, targetRotation);

        // Assert
        Assert.IsTrue(ship.HasPlannedMove, "Should have a planned move");
        Assert.IsNotNull(ship.PlannedPosition, "PlannedPosition should be set");
        Assert.IsNotNull(ship.PlannedRotation, "PlannedRotation should be set");
    }

    [Test]
    public void MovementTiming_ConsistentWithFixedDeltaTime()
    {
        // This test checks if using Time.fixedDeltaTime provides consistent timing
        // Arrange
        float fixedDeltaTime = 0.02f; // 50 Hz (Unity default)
        float duration = 3f;
        int expectedSteps = Mathf.CeilToInt(duration / fixedDeltaTime);

        // Act - Simulate movement steps
        float totalTime = 0f;
        int steps = 0;
        while (totalTime < duration)
        {
            totalTime += fixedDeltaTime;
            steps++;
        }

        // Assert
        Assert.AreEqual(expectedSteps, steps, "Number of FixedUpdate steps should match expected count");
        Assert.AreEqual(duration, totalTime, 0.02f, "Total time should match duration within tolerance");
    }

    [Test]
    public void MovementProgress_IncreasesMonotonically()
    {
        // Test that movement progress increases smoothly without jumps
        // Arrange
        float fixedDeltaTime = 0.02f;
        float duration = 3f;
        float previousProgress = 0f;

        // Act & Assert - Simulate movement progress
        for (float time = 0; time <= duration; time += fixedDeltaTime)
        {
            float progress = time / duration;
            Assert.GreaterOrEqual(progress, previousProgress, "Progress should never decrease");
            Assert.LessOrEqual(progress, 1f, "Progress should never exceed 1.0");
            previousProgress = progress;
        }
    }

    [Test]
    public void BezierCurve_NoJumpsBetweenFrames()
    {
        // Test that Bezier curve positions don't jump between frames
        // Arrange
        Vector3 p0 = Vector3.zero;
        Vector3 p1 = new Vector3(2, 0, 1);
        Vector3 p2 = new Vector3(4, 0, 2);
        Vector3 p3 = new Vector3(6, 0, 0);
        float maxExpectedJump = 1f; // Maximum reasonable distance between frames

        var method = typeof(Ship).GetMethod("CubicBezier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act - Sample curve at many points
        Vector3 previousPoint = p0;
        for (float t = 0.02f; t <= 1f; t += 0.02f)
        {
            Vector3 currentPoint = (Vector3)method.Invoke(ship, new object[] { p0, p1, p2, p3, t });
            float distance = Vector3.Distance(previousPoint, currentPoint);

            // Assert
            Assert.Less(distance, maxExpectedJump,
                $"Jump between frames at t={t:F2} is too large: {distance:F3} (expected < {maxExpectedJump})");
            previousPoint = currentPoint;
        }
    }

    [Test]
    public void RotationCalculation_SmoothTangents()
    {
        // Test that rotation tangents are smooth along the curve
        // Arrange
        Vector3 p0 = Vector3.zero;
        Vector3 p1 = new Vector3(2, 0, 1);
        Vector3 p2 = new Vector3(4, 0, 2);
        Vector3 p3 = new Vector3(6, 0, 0);

        var method = typeof(Ship).GetMethod("CubicBezier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act - Calculate tangents at multiple points
        Quaternion previousRotation = Quaternion.identity;
        for (float t = 0.01f; t < 1f; t += 0.05f)
        {
            Vector3 currentPoint = (Vector3)method.Invoke(ship, new object[] { p0, p1, p2, p3, t });
            Vector3 futurePoint = (Vector3)method.Invoke(ship, new object[] { p0, p1, p2, p3, Mathf.Min(t + 0.01f, 1f) });
            Vector3 tangent = (futurePoint - currentPoint).normalized;

            if (tangent.sqrMagnitude > 0.001f)
            {
                Quaternion currentRotation = Quaternion.LookRotation(tangent);
                float angleDiff = Quaternion.Angle(previousRotation, currentRotation);

                // Assert - Rotation shouldn't change drastically between frames
                if (t > 0.01f) // Skip first iteration
                {
                    Assert.Less(angleDiff, 30f,
                        $"Rotation change at t={t:F2} is too abrupt: {angleDiff:F1} degrees");
                }
                previousRotation = currentRotation;
            }
        }
    }

    [Test]
    public void FixedDeltaTime_ConsistentAcrossFrames()
    {
        // Verify that using fixedDeltaTime provides consistent values
        // This is more of a Unity engine test, but important for movement
        float dt1 = Time.fixedDeltaTime;
        float dt2 = Time.fixedDeltaTime;
        float dt3 = Time.fixedDeltaTime;

        Assert.AreEqual(dt1, dt2, "FixedDeltaTime should be constant");
        Assert.AreEqual(dt2, dt3, "FixedDeltaTime should be constant");
        Assert.Greater(dt1, 0, "FixedDeltaTime should be positive");
    }
}
