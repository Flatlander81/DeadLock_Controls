using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime controller for Phase 2.2 manual testing.
/// Handles input, UI panels, and test controls.
/// </summary>
public class Phase22TestController : MonoBehaviour
{
    [Header("References (Auto-assigned by Editor Script)")]
    public Ship playerShip;
    public Ship[] enemyShips;

    [Header("Runtime State")]
    public Ship currentTarget;
    public int currentTargetIndex = 0;

    // UI Panels
    private WeaponConfigPanel configPanel;
    private WeaponGroupPanel groupPanel;

    void Start()
    {
        // Wait a frame for WeaponManager to initialize
        Invoke(nameof(Initialize), 0.1f);
    }

    void Initialize()
    {
        Debug.Log("=== Phase 2.2 Test Controller Initializing ===");

        // Set initial target
        if (enemyShips != null && enemyShips.Length > 0)
        {
            currentTarget = enemyShips[0];
            currentTargetIndex = 0;
        }

        // Create UI panels
        CreateUIPanels();

        // Assign weapons to groups
        AssignWeaponGroups();

        // Set target for all weapons
        SetAllWeaponsTarget(currentTarget);

        // Debug weapon status
        DebugWeaponStatus();

        Debug.Log("");
        Debug.Log("CONTROLS:");
        Debug.Log("  1,2,3,4  - Fire weapon groups");
        Debug.Log("  A        - Alpha Strike (all weapons)");
        Debug.Log("  Tab      - Cycle targets");
        Debug.Log("  Space    - Toggle config panel visibility");
        Debug.Log("  R        - Reset all cooldowns");
        Debug.Log("  L        - Reload all ammo");
        Debug.Log("");
        Debug.Log("WEAPON GROUPS:");
        Debug.Log("  Group 1 (Blue):   RailGuns - instant hit, infinite ammo");
        Debug.Log("  Group 2 (Red):    Newtonian Cannon - slow projectile, infinite ammo");
        Debug.Log("  Group 3 (Green):  Torpedo - slow homing, 6 ammo, 3 turn cooldown");
        Debug.Log("  Group 4 (Yellow): Missiles - fast homing, 20 ammo each, 1 turn cooldown");
        Debug.Log("");
    }

    void CreateUIPanels()
    {
        // Config Panel (left side)
        GameObject configObj = new GameObject("WeaponConfigPanel");
        configObj.transform.SetParent(transform);
        configPanel = configObj.AddComponent<WeaponConfigPanel>();
        configPanel.Initialize(playerShip);
        configPanel.Show();

        // Group Panel (right side)
        GameObject groupObj = new GameObject("WeaponGroupPanel");
        groupObj.transform.SetParent(transform);
        groupPanel = groupObj.AddComponent<WeaponGroupPanel>();
        groupPanel.Initialize(playerShip, null);
        groupPanel.SetTarget(currentTarget);
        groupPanel.Show();

        Debug.Log("✓ Created UI Panels");
    }

    void AssignWeaponGroups()
    {
        WeaponManager wm = playerShip?.WeaponManager;
        if (wm == null)
        {
            Debug.LogError("No WeaponManager found!");
            return;
        }

        foreach (WeaponSystem weapon in wm.Weapons)
        {
            if (weapon is RailGun)
                wm.AssignWeaponToGroup(weapon, 1);
            else if (weapon is NewtonianCannon)
                wm.AssignWeaponToGroup(weapon, 2);
            else if (weapon is TorpedoLauncher)
                wm.AssignWeaponToGroup(weapon, 3);
            else if (weapon is MissileBattery)
                wm.AssignWeaponToGroup(weapon, 4);
        }

        Debug.Log($"✓ Assigned {wm.Weapons.Count} weapons to groups");
    }

    void SetAllWeaponsTarget(Ship target)
    {
        if (playerShip?.WeaponManager == null) return;

        foreach (WeaponSystem weapon in playerShip.WeaponManager.Weapons)
        {
            weapon.SetTarget(target);
        }

        if (groupPanel != null)
        {
            groupPanel.SetTarget(target);
        }

        Debug.Log($"Target set: {target?.gameObject.name ?? "None"}");
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        WeaponManager wm = playerShip?.WeaponManager;
        if (wm == null) return;

        // Fire weapon groups 1-4
        if (Input.GetKeyDown(KeyCode.Alpha1)) FireGroup(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) FireGroup(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) FireGroup(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) FireGroup(4);

        // Alpha Strike - fire all weapons
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log(">>> ALPHA STRIKE <<<");
            int fired = 0;
            foreach (WeaponSystem weapon in wm.Weapons)
            {
                if (weapon.CanFire())
                {
                    StartCoroutine(weapon.FireWithSpinUp());
                    fired++;
                }
            }
            Debug.Log($"Fired {fired}/{wm.Weapons.Count} weapons");
        }

        // Toggle config panel
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (configPanel != null)
            {
                configPanel.IsVisible = !configPanel.IsVisible;
            }
        }

        // Cycle targets
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (enemyShips != null && enemyShips.Length > 0)
            {
                currentTargetIndex = (currentTargetIndex + 1) % enemyShips.Length;
                currentTarget = enemyShips[currentTargetIndex];
                SetAllWeaponsTarget(currentTarget);
            }
        }

        // Cheat: Reset cooldowns
        if (Input.GetKeyDown(KeyCode.R))
        {
            var field = typeof(WeaponSystem).GetField("currentCooldown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            foreach (WeaponSystem weapon in wm.Weapons)
            {
                field.SetValue(weapon, 0);
            }
            Debug.Log("CHEAT: All cooldowns reset to 0");
        }

        // Cheat: Reload ammo
        if (Input.GetKeyDown(KeyCode.L))
        {
            var field = typeof(WeaponSystem).GetField("currentAmmo",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            foreach (WeaponSystem weapon in wm.Weapons)
            {
                if (weapon.AmmoCapacity > 0)
                {
                    field.SetValue(weapon, weapon.AmmoCapacity);
                }
            }
            Debug.Log("CHEAT: All ammo reloaded");
        }
    }

    void FireGroup(int groupNum)
    {
        WeaponManager wm = playerShip?.WeaponManager;
        if (wm == null) return;

        List<WeaponSystem> weapons = wm.GetWeaponsInGroup(groupNum);

        if (weapons.Count == 0)
        {
            Debug.Log($"Group {groupNum}: Empty");
            return;
        }

        Debug.Log($">>> Firing Group {groupNum} <<<");
        int fired = 0;
        int blocked = 0;

        foreach (WeaponSystem weapon in weapons)
        {
            if (weapon.CanFire())
            {
                StartCoroutine(weapon.FireWithSpinUp());
                fired++;
            }
            else
            {
                blocked++;
                // Log why it can't fire
                if (weapon.CurrentCooldown > 0)
                    Debug.Log($"  {weapon.WeaponName}: On cooldown ({weapon.CurrentCooldown} turns)");
                else if (weapon.AmmoCapacity > 0 && weapon.CurrentAmmo <= 0)
                    Debug.Log($"  {weapon.WeaponName}: Out of ammo!");
            }
        }

        Debug.Log($"  Fired: {fired}, Blocked: {blocked}");
    }

    void OnGUI()
    {
        // Status overlay - top left
        GUILayout.BeginArea(new Rect(10, 10, 280, 300));

        GUI.Box(new Rect(0, 0, 280, 280), "");

        GUILayout.Label("<b>PHASE 2.2 TEST SCENE</b>");
        GUILayout.Space(5);

        // Target info
        if (currentTarget != null)
        {
            GUI.color = Color.red;
            GUILayout.Label($"Target: {currentTarget.gameObject.name}");
            GUI.color = Color.white;

            float dist = Vector3.Distance(playerShip.transform.position, currentTarget.transform.position);
            GUILayout.Label($"Distance: {dist:F1} units");
        }

        GUILayout.Space(10);

        // Heat
        if (playerShip?.HeatManager != null)
        {
            float heat = playerShip.HeatManager.CurrentHeat;
            float maxHeat = playerShip.HeatManager.MaxHeat;
            GUI.color = heat > maxHeat * 0.7f ? Color.red : Color.white;
            GUILayout.Label($"Heat: {heat:F0}/{maxHeat}");
            GUI.color = Color.white;
        }

        GUILayout.Space(10);

        // Ammo status
        GUILayout.Label("<b>AMMO STATUS:</b>");
        if (playerShip?.WeaponManager != null)
        {
            foreach (WeaponSystem weapon in playerShip.WeaponManager.Weapons)
            {
                if (weapon.AmmoCapacity > 0)
                {
                    float pct = (float)weapon.CurrentAmmo / weapon.AmmoCapacity;
                    if (pct <= 0)
                        GUI.color = Color.red;
                    else if (pct < 0.25f)
                        GUI.color = Color.yellow;
                    else
                        GUI.color = Color.green;

                    string status = weapon.CurrentAmmo > 0
                        ? $"{weapon.CurrentAmmo}/{weapon.AmmoCapacity}"
                        : "EMPTY!";
                    GUILayout.Label($"  {weapon.WeaponName}: {status}");
                }
            }
        }
        GUI.color = Color.white;

        GUILayout.Space(10);
        GUILayout.Label("Press Tab to switch targets");
        GUILayout.Label("Press R to reset cooldowns");
        GUILayout.Label("Press L to reload ammo");

        GUILayout.EndArea();
    }

    void OnDrawGizmos()
    {
        // Draw line to current target
        if (currentTarget != null && playerShip != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(playerShip.transform.position, currentTarget.transform.position);
        }
    }

    void DebugWeaponStatus()
    {
        if (playerShip?.WeaponManager == null || currentTarget == null) return;

        Debug.Log($"Player at: {playerShip.transform.position}, facing: {playerShip.transform.forward}");
        Debug.Log($"Target at: {currentTarget.transform.position}");

        foreach (WeaponSystem weapon in playerShip.WeaponManager.Weapons)
        {
            Transform hp = weapon.transform;
            Vector3 toTarget = (currentTarget.transform.position - hp.position).normalized;
            float angle = Vector3.Angle(hp.forward, toTarget);
            float distance = Vector3.Distance(hp.position, currentTarget.transform.position);

            bool inArc = weapon.IsInArc(currentTarget.transform.position);
            bool inRange = weapon.IsInRange(currentTarget.transform.position);

            Debug.Log($"  {weapon.WeaponName}: pos={hp.position}, fwd={hp.forward}");
            Debug.Log($"    -> angle={angle:F1} (arc={weapon.FiringArc}/2={weapon.FiringArc/2}), inArc={inArc}");
            Debug.Log($"    -> dist={distance:F1} (range={weapon.MaxRange}), inRange={inRange}");
            Debug.Log($"    -> CanFire={weapon.CanFireSilent()}");
        }
    }
}
