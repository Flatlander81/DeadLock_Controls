using UnityEngine;
using System.Collections;

/// <summary>
/// Temporary testing script for weapon system (until Track C - Targeting UI is complete).
/// Automatically assigns targets and provides keyboard controls for firing weapons.
/// </summary>
public class WeaponTester : MonoBehaviour
{
    [Header("Ship References")]
    [SerializeField] private Ship playerShip;
    [SerializeField] private Ship enemyShip;

    [Header("Test Controls")]
    [SerializeField] private KeyCode fireGroup1Key = KeyCode.Space;
    [SerializeField] private KeyCode fireGroup2Key = KeyCode.F;
    [SerializeField] private KeyCode fireGroup3Key = KeyCode.G;
    [SerializeField] private KeyCode alphaStrikeKey = KeyCode.A;
    [SerializeField] private KeyCode moveEnemyLeftKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode moveEnemyRightKey = KeyCode.RightArrow;
    [SerializeField] private KeyCode moveEnemyForwardKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode moveEnemyBackKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode resetEnemyPosKey = KeyCode.R;
    [SerializeField] private KeyCode killEnemyKey = KeyCode.K;

    [Header("Test Settings")]
    [SerializeField] private float enemyMoveDistance = 5f;
    [SerializeField] private Vector3 enemyStartPosition = new Vector3(0f, 0f, 30f);

    private bool initialized = false;

    void Start()
    {
        // Wait a frame for all components to initialize
        StartCoroutine(InitializeAfterDelay());
    }

    private IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        if (playerShip == null)
        {
            Debug.LogError("WeaponTester: Player ship not assigned!");
            yield break;
        }

        if (enemyShip == null)
        {
            Debug.LogError("WeaponTester: Enemy ship not assigned!");
            yield break;
        }

        // Position enemy at start position
        enemyShip.transform.position = enemyStartPosition;

        // Assign enemy as target for all player weapons
        if (playerShip.WeaponManager != null)
        {
            var weapons = playerShip.WeaponManager.Weapons;

            if (weapons.Count == 0)
            {
                Debug.LogWarning("WeaponTester: No weapons found on player ship!");
                yield break;
            }

            foreach (var weapon in weapons)
            {
                weapon.SetTarget(enemyShip);
                Debug.Log($"[WeaponTester] Assigned {enemyShip.name} as target for {weapon.WeaponName}");
            }

            // Assign weapons to groups for testing
            // Group 1: First 2 weapons (RailGuns)
            // Group 2: Third weapon (Cannon)
            if (weapons.Count > 0) playerShip.WeaponManager.AssignWeaponToGroup(weapons[0], 1);
            if (weapons.Count > 1) playerShip.WeaponManager.AssignWeaponToGroup(weapons[1], 1);
            if (weapons.Count > 2) playerShip.WeaponManager.AssignWeaponToGroup(weapons[2], 2);

            initialized = true;
            PrintInstructions();
        }
        else
        {
            Debug.LogError("WeaponTester: Player ship has no WeaponManager component!");
        }
    }

    void Update()
    {
        if (!initialized) return;

        HandleWeaponFiring();
        HandleEnemyMovement();
        HandleTestCommands();
    }

    private void HandleWeaponFiring()
    {
        if (playerShip == null || playerShip.WeaponManager == null) return;

        // Fire weapon groups
        if (Input.GetKeyDown(fireGroup1Key))
        {
            Debug.Log("[WeaponTester] Firing Group 1 (RailGuns)...");
            StartCoroutine(playerShip.WeaponManager.FireGroup(1));
        }

        if (Input.GetKeyDown(fireGroup2Key))
        {
            Debug.Log("[WeaponTester] Firing Group 2 (Cannon)...");
            StartCoroutine(playerShip.WeaponManager.FireGroup(2));
        }

        if (Input.GetKeyDown(fireGroup3Key))
        {
            Debug.Log("[WeaponTester] Firing Group 3...");
            StartCoroutine(playerShip.WeaponManager.FireGroup(3));
        }

        if (Input.GetKeyDown(alphaStrikeKey))
        {
            Debug.Log("[WeaponTester] ALPHA STRIKE - Firing all weapons!");
            StartCoroutine(playerShip.WeaponManager.FireAlphaStrike(enemyShip));
        }
    }

    private void HandleEnemyMovement()
    {
        if (enemyShip == null) return;

        Vector3 move = Vector3.zero;

        if (Input.GetKeyDown(moveEnemyLeftKey))
            move = Vector3.left * enemyMoveDistance;
        else if (Input.GetKeyDown(moveEnemyRightKey))
            move = Vector3.right * enemyMoveDistance;
        else if (Input.GetKeyDown(moveEnemyForwardKey))
            move = Vector3.forward * enemyMoveDistance;
        else if (Input.GetKeyDown(moveEnemyBackKey))
            move = Vector3.back * enemyMoveDistance;

        if (move != Vector3.zero)
        {
            enemyShip.transform.position += move;
            Debug.Log($"[WeaponTester] Enemy moved to {enemyShip.transform.position}");
            LogWeaponStatus();
        }
    }

    private void HandleTestCommands()
    {
        if (Input.GetKeyDown(resetEnemyPosKey))
        {
            enemyShip.transform.position = enemyStartPosition;
            Debug.Log($"[WeaponTester] Enemy reset to start position {enemyStartPosition}");
            LogWeaponStatus();
        }

        if (Input.GetKeyDown(killEnemyKey))
        {
            float totalDamage = enemyShip.CurrentShields + enemyShip.CurrentHull;
            Debug.Log($"[WeaponTester] Killing enemy (dealing {totalDamage} damage)...");
            enemyShip.TakeDamage(totalDamage);
        }
    }

    private void LogWeaponStatus()
    {
        if (playerShip == null || playerShip.WeaponManager == null) return;

        var weapons = playerShip.WeaponManager.Weapons;
        foreach (var weapon in weapons)
        {
            bool canFire = weapon.CanFire();
            string status = canFire ? "READY" : "NOT READY";
            Debug.Log($"  {weapon.WeaponName}: {status}");
        }
    }

    private void PrintInstructions()
    {
        Debug.Log("========================================");
        Debug.Log("WEAPON SYSTEM TEST CONTROLS");
        Debug.Log("========================================");
        Debug.Log($"SPACE   - Fire Group 1 (RailGuns)");
        Debug.Log($"F       - Fire Group 2 (Cannon)");
        Debug.Log($"G       - Fire Group 3");
        Debug.Log($"A       - Alpha Strike (all weapons)");
        Debug.Log($"");
        Debug.Log($"ARROWS  - Move enemy ship (test arc/range)");
        Debug.Log($"R       - Reset enemy position");
        Debug.Log($"K       - Kill enemy (test dead target detection)");
        Debug.Log($"");
        Debug.Log($"1-6     - Activate abilities");
        Debug.Log("========================================");
        Debug.Log($"Player Heat: {playerShip.HeatManager.CurrentHeat}/{playerShip.HeatManager.MaxHeat}");
        Debug.Log($"Enemy Hull: {enemyShip.CurrentHull}/{enemyShip.MaxHull}");
        Debug.Log($"Enemy Shields: {enemyShip.CurrentShields}/{enemyShip.MaxShields}");
        Debug.Log("========================================");
        LogWeaponStatus();
    }

    void OnGUI()
    {
        if (!initialized) return;

        // Simple on-screen display
        GUI.Box(new Rect(10, 10, 300, 240), "Weapon Test Controls");

        int y = 35;
        GUI.Label(new Rect(20, y, 280, 20), "SPACE - Fire Group 1"); y += 20;
        GUI.Label(new Rect(20, y, 280, 20), "F - Fire Group 2"); y += 20;
        GUI.Label(new Rect(20, y, 280, 20), "A - Alpha Strike"); y += 20;
        GUI.Label(new Rect(20, y, 280, 20), "ARROWS - Move Enemy"); y += 20;
        GUI.Label(new Rect(20, y, 280, 20), "R - Reset Enemy"); y += 20;
        GUI.Label(new Rect(20, y, 280, 20), "K - Kill Enemy"); y += 20;
        y += 10;
        GUI.Label(new Rect(20, y, 280, 20), $"Player Heat: {playerShip.HeatManager.CurrentHeat:F0}"); y += 20;
        GUI.Label(new Rect(20, y, 280, 20), $"Enemy Hull: {enemyShip.CurrentHull:F0}/{enemyShip.MaxHull:F0}"); y += 20;
        GUI.Label(new Rect(20, y, 280, 20), $"Enemy Shields: {enemyShip.CurrentShields:F0}/{enemyShip.MaxShields:F0}"); y += 20;
        GUI.Label(new Rect(20, y, 280, 20), $"Enemy Pos: {enemyShip.transform.position}");
    }
}
