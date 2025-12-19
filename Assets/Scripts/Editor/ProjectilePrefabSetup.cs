using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to create projectile prefabs.
/// Run this to generate BallisticProjectile, HomingProjectile, and InstantHitEffect prefabs.
/// </summary>
public class ProjectilePrefabSetup : EditorWindow
{
    [MenuItem("Hephaestus/Setup/Create Projectile Prefabs")]
    public static void CreateProjectilePrefabs()
    {
        Debug.Log("Creating projectile prefabs...");

        // Create prefabs folder if it doesn't exist
        string prefabPath = "Assets/Prefabs/Projectiles";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(prefabPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Projectiles");
        }

        // Create Ballistic Projectile Prefab
        CreateBallisticProjectilePrefab(prefabPath);

        // Create Homing Projectile Prefab
        CreateHomingProjectilePrefab(prefabPath);

        // Create Instant Hit Effect Prefab
        CreateInstantHitEffectPrefab(prefabPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("========================================");
        Debug.Log("PROJECTILE PREFABS CREATED!");
        Debug.Log("========================================");
        Debug.Log($"Location: {prefabPath}");
        Debug.Log("- BallisticProjectile.prefab");
        Debug.Log("- HomingProjectile.prefab");
        Debug.Log("- InstantHitEffect.prefab");
        Debug.Log("");
        Debug.Log("Next steps:");
        Debug.Log("1. Assign prefabs to ProjectileManager in scene");
        Debug.Log("2. Or ProjectileManager will create default projectiles");
        Debug.Log("========================================");
    }

    private static void CreateBallisticProjectilePrefab(string path)
    {
        // Create GameObject
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = "BallisticProjectile";
        obj.transform.localScale = Vector3.one * 0.3f;

        // Add BallisticProjectile component
        BallisticProjectile projectile = obj.AddComponent<BallisticProjectile>();

        // Configure projectile via reflection
        SetProjectileField(projectile, "damage", 40f);
        SetProjectileField(projectile, "speed", 5f);
        SetProjectileField(projectile, "lifetime", 10f);
        SetProjectileField(projectile, "collisionRadius", 0.5f);

        // Add Trail Renderer for visual
        TrailRenderer trail = obj.AddComponent<TrailRenderer>();
        trail.time = 0.5f;
        trail.startWidth = 0.2f;
        trail.endWidth = 0.05f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = Color.cyan;
        trail.endColor = new Color(0f, 1f, 1f, 0f); // Fade to transparent

        // Make glowing material
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetColor("_Color", Color.cyan);
            mat.SetColor("_EmissionColor", Color.cyan);
            mat.EnableKeyword("_EMISSION");
            renderer.material = mat;
        }

        // Save as prefab
        string prefabPath = $"{path}/BallisticProjectile.prefab";
        PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
        Debug.Log($"✓ Created {prefabPath}");

        // Cleanup temporary object
        Object.DestroyImmediate(obj);
    }

    private static void CreateHomingProjectilePrefab(string path)
    {
        // Create GameObject
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        obj.name = "HomingProjectile";
        obj.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);

        // Add HomingProjectile component
        HomingProjectile projectile = obj.AddComponent<HomingProjectile>();

        // Configure projectile via reflection
        SetProjectileField(projectile, "damage", 60f);
        SetProjectileField(projectile, "speed", 3f);
        SetProjectileField(projectile, "lifetime", 15f);
        SetProjectileField(projectile, "collisionRadius", 0.5f);

        // Set homing specific fields
        var turnRateField = typeof(HomingProjectile).GetField("turnRate",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (turnRateField != null)
        {
            turnRateField.SetValue(projectile, 90f); // 90 degrees/sec turn rate
        }

        // Add Trail Renderer
        TrailRenderer trail = obj.AddComponent<TrailRenderer>();
        trail.time = 0.7f;
        trail.startWidth = 0.15f;
        trail.endWidth = 0.03f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = Color.yellow;
        trail.endColor = new Color(1f, 1f, 0f, 0f); // Fade to transparent

        // Add particle system for thruster effect
        GameObject thrusterObj = new GameObject("ThrusterEffect");
        thrusterObj.transform.SetParent(obj.transform);
        thrusterObj.transform.localPosition = new Vector3(0f, -0.5f, 0f); // Back of projectile

        ParticleSystem ps = thrusterObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.1f;
        main.startColor = Color.yellow;

        var emission = ps.emission;
        emission.rateOverTime = 20f;

        // Make glowing material
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetColor("_Color", Color.yellow);
            mat.SetColor("_EmissionColor", Color.yellow * 2f);
            mat.EnableKeyword("_EMISSION");
            renderer.material = mat;
        }

        // Save as prefab
        string prefabPath = $"{path}/HomingProjectile.prefab";
        PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
        Debug.Log($"✓ Created {prefabPath}");

        // Cleanup
        Object.DestroyImmediate(obj);
    }

    private static void CreateInstantHitEffectPrefab(string path)
    {
        // Create GameObject
        GameObject obj = new GameObject("InstantHitEffect");

        // Add InstantHitEffect component
        InstantHitEffect effect = obj.AddComponent<InstantHitEffect>();

        // Configure via reflection
        var fadeField = typeof(InstantHitEffect).GetField("fadeOutDuration",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fadeField != null)
        {
            fadeField.SetValue(effect, 0.2f);
        }

        var colorField = typeof(InstantHitEffect).GetField("startColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (colorField != null)
        {
            colorField.SetValue(effect, Color.cyan);
        }

        var startWidthField = typeof(InstantHitEffect).GetField("startWidth",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (startWidthField != null)
        {
            startWidthField.SetValue(effect, 0.1f);
        }

        var endWidthField = typeof(InstantHitEffect).GetField("endWidth",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (endWidthField != null)
        {
            endWidthField.SetValue(effect, 0.05f);
        }

        // Add LineRenderer component
        LineRenderer lineRenderer = obj.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.cyan;
        lineRenderer.endColor = Color.cyan;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;

        // Save as prefab
        string prefabPath = $"{path}/InstantHitEffect.prefab";
        PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
        Debug.Log($"✓ Created {prefabPath}");

        // Cleanup
        Object.DestroyImmediate(obj);
    }

    private static void SetProjectileField(Projectile projectile, string fieldName, object value)
    {
        var field = typeof(Projectile).GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(projectile, value);
        }
    }
}
