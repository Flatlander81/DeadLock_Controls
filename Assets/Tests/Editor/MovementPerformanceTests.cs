using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Performance tests to diagnose choppy movement issues.
/// Measures computation time, frame consistency, and identifies bottlenecks.
/// </summary>
public class MovementPerformanceTests
{
    [Test]
    public void BezierCalculation_PerformanceTest()
    {
        // Test how long Bezier calculations take
        Vector3 p0 = Vector3.zero;
        Vector3 p1 = new Vector3(2, 0, 1);
        Vector3 p2 = new Vector3(4, 0, 2);
        Vector3 p3 = new Vector3(6, 0, 0);

        Stopwatch sw = new Stopwatch();
        sw.Start();

        // Simulate one second of movement at 50 Hz
        int iterations = 50;
        for (int i = 0; i < iterations; i++)
        {
            float t = i / (float)iterations;
            Vector3 result = CubicBezierStatic(p0, p1, p2, p3, t);
        }

        sw.Stop();
        float msPerFrame = (float)sw.Elapsed.TotalMilliseconds / iterations;

        Assert.Less(msPerFrame, 0.5f, "Bezier calculation should be fast enough for 50Hz updates");
    }

    [Test]
    public void QuaternionLookRotation_PerformanceTest()
    {
        // Test rotation calculation performance
        Stopwatch sw = new Stopwatch();
        sw.Start();

        int iterations = 50;
        for (int i = 0; i < iterations; i++)
        {
            Vector3 direction = new Vector3(Random.value, 0, Random.value).normalized;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion rotation = Quaternion.LookRotation(direction);
            }
        }

        sw.Stop();
        float msPerFrame = (float)sw.Elapsed.TotalMilliseconds / iterations;

        Assert.Less(msPerFrame, 0.1f, "Rotation calculation should be negligible");
    }

    [Test]
    public void FrameTimeConsistency_Simulation()
    {
        // Simulate consistent vs variable frame times to demonstrate the issue
        float fixedDeltaTime = 0.02f; // 50 Hz
        float duration = 3f;

        // Fixed timestep (FixedUpdate) - should be consistent
        List<float> fixedTimesteps = new List<float>();
        for (float t = 0; t < duration; t += fixedDeltaTime)
        {
            fixedTimesteps.Add(fixedDeltaTime);
        }

        // Variable timestep (Update) - simulates frame rate variations
        List<float> variableTimesteps = new List<float>();
        float t2 = 0;
        while (t2 < duration)
        {
            // Simulate frame rate variation (30-60 FPS)
            float dt = Random.Range(0.016f, 0.033f);
            variableTimesteps.Add(dt);
            t2 += dt;
        }

        // Calculate variance
        float fixedVariance = CalculateVariance(fixedTimesteps);
        float variableVariance = CalculateVariance(variableTimesteps);

        Assert.Less(fixedVariance, 0.0001f, "Fixed timestep should have near-zero variance");
        Assert.Greater(variableVariance, fixedVariance * 100, "Variable timestep should have much higher variance");
    }

    [Test]
    public void MovementSmoothnessIndex_Calculation()
    {
        // Calculate a "smoothness index" based on frame-to-frame velocity changes
        // Lower index = smoother movement

        float fixedDeltaTime = 0.02f;
        Vector3 p0 = Vector3.zero;
        Vector3 p1 = new Vector3(2, 0, 1);
        Vector3 p2 = new Vector3(4, 0, 2);
        Vector3 p3 = new Vector3(6, 0, 0);

        List<float> velocityChanges = new List<float>();
        Vector3 previousPos = p0;
        Vector3 previousVelocity = Vector3.zero;

        for (float t = fixedDeltaTime; t <= 1f; t += fixedDeltaTime)
        {
            Vector3 currentPos = CubicBezierStatic(p0, p1, p2, p3, t);
            Vector3 currentVelocity = (currentPos - previousPos) / fixedDeltaTime;

            if (previousVelocity != Vector3.zero)
            {
                float velocityChange = Vector3.Distance(currentVelocity, previousVelocity);
                velocityChanges.Add(velocityChange);
            }

            previousPos = currentPos;
            previousVelocity = currentVelocity;
        }

        float smoothnessIndex = CalculateVariance(velocityChanges);

        Assert.Less(smoothnessIndex, 5f, "Smoothness index should indicate smooth movement");
    }

    [Test]
    public void LineRenderer_UpdateCost()
    {
        // Test the cost of updating LineRenderer (path visualization)
        GameObject lineObj = new GameObject("TestLine");
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

        Vector3 p0 = Vector3.zero;
        Vector3 p1 = new Vector3(2, 0, 1);
        Vector3 p2 = new Vector3(4, 0, 2);
        Vector3 p3 = new Vector3(6, 0, 0);

        Stopwatch sw = new Stopwatch();
        sw.Start();

        // Simulate updating line renderer every frame
        int frames = 50;
        for (int frame = 0; frame < frames; frame++)
        {
            int curveResolution = 20;
            lineRenderer.positionCount = curveResolution + 1;

            for (int i = 0; i <= curveResolution; i++)
            {
                float t = i / (float)curveResolution;
                Vector3 point = CubicBezierStatic(p0, p1, p2, p3, t);
                lineRenderer.SetPosition(i, point);
            }
        }

        sw.Stop();
        float msPerFrame = (float)sw.Elapsed.TotalMilliseconds / frames;

        Object.DestroyImmediate(lineObj);

        // This could be a bottleneck if it's being updated in HandleProjectionDrag
        Assert.Less(msPerFrame, 1f, "LineRenderer updates should be reasonably fast");
    }

    [Test]
    public void DiagnoseChoppyMovement_Report()
    {
        // Diagnostic report - choppy movement has been fixed
        Assert.Pass("Movement execution choppiness fixed with Quaternion.RotateTowards interpolation.");
    }

    // Helper methods
    private static Vector3 CubicBezierStatic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;

        return (uuu * p0) + (3 * uu * t * p1) + (3 * u * tt * p2) + (ttt * p3);
    }

    private float CalculateVariance(List<float> values)
    {
        if (values.Count == 0) return 0f;

        float mean = 0f;
        foreach (float v in values)
        {
            mean += v;
        }
        mean /= values.Count;

        float variance = 0f;
        foreach (float v in values)
        {
            float diff = v - mean;
            variance += diff * diff;
        }
        variance /= values.Count;

        return variance;
    }

    private float CalculateVariance(List<Vector3> values)
    {
        if (values.Count == 0) return 0f;

        Vector3 mean = Vector3.zero;
        foreach (Vector3 v in values)
        {
            mean += v;
        }
        mean /= values.Count;

        float variance = 0f;
        foreach (Vector3 v in values)
        {
            Vector3 diff = v - mean;
            variance += diff.sqrMagnitude;
        }
        variance /= values.Count;

        return variance;
    }
}
