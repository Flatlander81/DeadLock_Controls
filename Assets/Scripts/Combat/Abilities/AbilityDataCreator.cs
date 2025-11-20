using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor utility to create default ability data assets.
/// Run via: Tools → Create Default Abilities
/// </summary>
public class AbilityDataCreator
{
#if UNITY_EDITOR
    [MenuItem("Tools/Create Default Abilities")]
    public static void CreateDefaultAbilities()
    {
        // Ensure directory exists
        string path = "Assets/Data/Abilities";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Abilities");
        }

        // Emergency Cooling
        CreateAbility<EmergencyCoolingData>(
            "EmergencyCooling",
            "Emergency Cooling",
            "Instantly reduces heat by 50 points",
            heatCost: 0,
            cooldown: 3,
            spinUp: 0f,
            color: new Color(0.3f, 0.7f, 1f) // Light blue
        );

        // Shield Boost
        CreateAbility<ShieldBoostData>(
            "ShieldBoost",
            "Shield Boost",
            "Instantly restores 100 shield points",
            heatCost: 15,
            cooldown: 4,
            spinUp: 0.5f,
            color: new Color(0f, 0.8f, 0.8f) // Cyan
        );

        // Evasive Maneuver
        CreateAbility<EvasiveManeuverData>(
            "EvasiveManeuver",
            "Evasive Maneuver",
            "Doubles turn rate for 2 turns",
            heatCost: 10,
            cooldown: 3,
            spinUp: 0f,
            color: new Color(0.2f, 1f, 0.2f) // Green
        );

        // Overcharge Weapons
        CreateAbility<OverchargeWeaponsData>(
            "OverchargeWeapons",
            "Overcharge Weapons",
            "Increases damage by 50% for 3 turns, generates heat per turn",
            heatCost: 20,
            cooldown: 5,
            spinUp: 1f,
            color: new Color(1f, 0.3f, 0.2f) // Red
        );

        // Sensor Burst
        CreateAbility<SensorBurstData>(
            "SensorBurst",
            "Sensor Burst",
            "Reveals all enemies within 100 units for 2 turns",
            heatCost: 5,
            cooldown: 2,
            spinUp: 0f,
            color: new Color(1f, 1f, 0.2f) // Yellow
        );

        // PD Override
        CreateAbility<PDOverrideData>(
            "PDOverride",
            "PD Override",
            "Boosts point defense intercept chance by 50% for 2 turns",
            heatCost: 12,
            cooldown: 3,
            spinUp: 0.5f,
            color: new Color(1f, 0.5f, 0f) // Orange
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Created 6 default ability data assets in Assets/Data/Abilities/");
    }

    private static void CreateAbility<T>(
        string fileName,
        string abilityName,
        string description,
        int heatCost,
        int cooldown,
        float spinUp,
        Color color) where T : AbilityData
    {
        string assetPath = $"Assets/Data/Abilities/{fileName}.asset";

        // Check if asset already exists
        T existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (existing != null)
        {
            Debug.Log($"Ability asset already exists: {assetPath}");
            return;
        }

        // Create new instance
        T data = ScriptableObject.CreateInstance<T>();
        data.abilityName = abilityName;
        data.description = description;
        data.heatCost = heatCost;
        data.maxCooldown = cooldown;
        data.spinUpTime = spinUp;
        data.abilityColor = color;

        // Create the asset
        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log($"Created ability asset: {assetPath}");
    }
#endif
}
