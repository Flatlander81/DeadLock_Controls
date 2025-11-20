using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to add weapon hardpoints to the player ship prefab.
/// Run this once to set up hardpoint structure.
/// </summary>
public class WeaponHardpointSetup : EditorWindow
{
    [MenuItem("Tools/Setup Weapon Hardpoints")]
    public static void SetupHardpoints()
    {
        // Find the ship prefab in the scene
        GameObject ship = GameObject.Find("Scifi_Ship_Cruiser-FBX");

        if (ship == null)
        {
            Debug.LogError("Ship not found in scene! Please add 'Scifi_Ship_Cruiser-FBX' prefab to the scene first.");
            return;
        }

        // Create hardpoint parent object
        GameObject hardpointsParent = new GameObject("WeaponHardpoints");
        hardpointsParent.transform.SetParent(ship.transform);
        hardpointsParent.transform.localPosition = Vector3.zero;
        hardpointsParent.transform.localRotation = Quaternion.identity;

        // Ship scale is 0.04, so we need to work in that coordinate space
        // Approximate positions based on a cruiser ship model
        // Forward = +Z, Up = +Y, Right = +X (in ship local space)

        // RailGun Hardpoints (port and starboard, mid-ship)
        CreateHardpoint(hardpointsParent.transform, "RailGun_Port_Hardpoint",
            new Vector3(-15f, 0f, 10f));  // Left side, forward

        CreateHardpoint(hardpointsParent.transform, "RailGun_Starboard_Hardpoint",
            new Vector3(15f, 0f, 10f));   // Right side, forward

        // Cannon Hardpoint (forward, center)
        CreateHardpoint(hardpointsParent.transform, "Cannon_Forward_Hardpoint",
            new Vector3(0f, 0f, 20f));    // Center front

        // Torpedo Hardpoint (forward, slightly below)
        CreateHardpoint(hardpointsParent.transform, "Torpedo_Forward_Hardpoint",
            new Vector3(0f, -5f, 15f));   // Center front, below

        // Missile Hardpoints (dorsal and ventral, mid-ship)
        CreateHardpoint(hardpointsParent.transform, "Missile_Dorsal_Hardpoint",
            new Vector3(0f, 10f, 0f));    // Top, center

        CreateHardpoint(hardpointsParent.transform, "Missile_Ventral_Hardpoint",
            new Vector3(0f, -10f, 0f));   // Bottom, center

        Debug.Log($"✓ Created 6 weapon hardpoints on {ship.name}");
        Debug.Log("Positions are approximate - adjust in Scene view as needed for your ship model.");
        Debug.Log("Remember to save the prefab after positioning hardpoints!");

        // Select the hardpoints parent for easy editing
        Selection.activeGameObject = hardpointsParent;
    }

    private static void CreateHardpoint(Transform parent, string name, Vector3 localPosition)
    {
        GameObject hardpoint = new GameObject(name);
        hardpoint.transform.SetParent(parent);
        hardpoint.transform.localPosition = localPosition;
        hardpoint.transform.localRotation = Quaternion.identity;

        // Add a small gizmo to visualize the hardpoint in the editor
        var gizmoScript = hardpoint.AddComponent<HardpointGizmo>();

        Debug.Log($"  Created hardpoint: {name} at local position {localPosition}");
    }
}
