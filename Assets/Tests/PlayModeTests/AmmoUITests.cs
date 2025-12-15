using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for ammo display in weapon configuration UI.
/// Tests that limited ammo weapons show ammo count and infinite ammo weapons don't.
/// </summary>
public class AmmoUITests
{
    private GameObject shipObject;
    private Ship ship;
    private WeaponManager weaponManager;

    private GameObject configPanelObject;
    private WeaponConfigPanel configPanel;

    private GameObject targetObject;
    private Ship targetShip;

    [SetUp]
    public void Setup()
    {
        // Reset ProjectileManager singleton
        ProjectileManager.ResetInstance();

        // Create player ship
        shipObject = new GameObject("PlayerShip");
        ship = shipObject.AddComponent<Ship>();
        shipObject.AddComponent<HeatManager>();
        shipObject.transform.position = Vector3.zero;

        // Create config panel
        configPanelObject = new GameObject("ConfigPanel");
        configPanel = configPanelObject.AddComponent<WeaponConfigPanel>();

        // Create target ship for firing tests
        targetObject = new GameObject("TargetShip");
        targetShip = targetObject.AddComponent<Ship>();
        targetObject.AddComponent<HeatManager>();
        targetObject.transform.position = new Vector3(0f, 0f, 20f);
    }

    [TearDown]
    public void Teardown()
    {
        ProjectileManager.ResetInstance();

        if (shipObject != null) Object.DestroyImmediate(shipObject);
        if (configPanelObject != null) Object.DestroyImmediate(configPanelObject);
        if (targetObject != null) Object.DestroyImmediate(targetObject);
    }

    /// <summary>
    /// Test 1: Add TorpedoLauncher (limited ammo), verify ammo properties accessible.
    /// The actual UI rendering requires OnGUI which can't be tested directly,
    /// so we verify the data that would be displayed.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ConfigPanel_ShowsAmmoForLimitedWeapon()
    {
        yield return null;

        // Create torpedo launcher (has limited ammo)
        GameObject torpedoHardpoint = new GameObject("Torpedo_Hardpoint");
        torpedoHardpoint.transform.SetParent(shipObject.transform);
        TorpedoLauncher torpedoLauncher = torpedoHardpoint.AddComponent<TorpedoLauncher>();

        // Add WeaponManager
        weaponManager = shipObject.AddComponent<WeaponManager>();
        yield return null; // Wait for initialization

        // Initialize config panel
        configPanel.Initialize(ship);

        // Verify torpedo has limited ammo that would be displayed
        Assert.Greater(torpedoLauncher.AmmoCapacity, 0, "Torpedo should have limited ammo capacity");
        Assert.AreEqual(6, torpedoLauncher.AmmoCapacity, "Torpedo should have 6 ammo capacity");
        Assert.AreEqual(6, torpedoLauncher.CurrentAmmo, "Torpedo should start with full ammo");

        // Verify the weapon is in the manager's list
        Assert.Contains(torpedoLauncher, weaponManager.Weapons, "Torpedo should be in weapon list");

        // Cleanup
        Object.DestroyImmediate(torpedoHardpoint);
    }

    /// <summary>
    /// Test 2: Add RailGun (infinite ammo), verify no ammo would be displayed.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ConfigPanel_HidesAmmoForInfiniteWeapon()
    {
        yield return null;

        // Create rail gun (has infinite ammo)
        GameObject railGunHardpoint = new GameObject("RailGun_Hardpoint");
        railGunHardpoint.transform.SetParent(shipObject.transform);
        RailGun railGun = railGunHardpoint.AddComponent<RailGun>();

        // Add WeaponManager
        weaponManager = shipObject.AddComponent<WeaponManager>();
        yield return null; // Wait for initialization

        // Initialize config panel
        configPanel.Initialize(ship);

        // Verify rail gun has infinite ammo (capacity = 0)
        Assert.AreEqual(0, railGun.AmmoCapacity, "RailGun should have 0 ammo capacity (infinite)");

        // The UI only shows ammo when AmmoCapacity > 0
        // So this weapon should NOT display ammo in the UI
        bool shouldShowAmmo = railGun.AmmoCapacity > 0;
        Assert.IsFalse(shouldShowAmmo, "RailGun should NOT show ammo display");

        // Cleanup
        Object.DestroyImmediate(railGunHardpoint);
    }

    /// <summary>
    /// Test 3: Fire weapon, verify ammo count updates.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ConfigPanel_AmmoUpdatesAfterFiring()
    {
        yield return null;

        // Create torpedo launcher
        GameObject torpedoHardpoint = new GameObject("Torpedo_Hardpoint");
        torpedoHardpoint.transform.SetParent(shipObject.transform);
        TorpedoLauncher torpedoLauncher = torpedoHardpoint.AddComponent<TorpedoLauncher>();

        // Add WeaponManager
        weaponManager = shipObject.AddComponent<WeaponManager>();
        yield return null; // Wait for initialization

        // Initialize config panel
        configPanel.Initialize(ship);

        // Verify initial ammo
        Assert.AreEqual(6, torpedoLauncher.CurrentAmmo, "Should start with 6 ammo");

        // Set target and fire
        torpedoLauncher.SetTarget(targetShip);
        yield return torpedoLauncher.FireWithSpinUp();

        // Verify ammo decreased
        Assert.AreEqual(5, torpedoLauncher.CurrentAmmo, "Should have 5 ammo after firing");

        // Fire again (reset cooldown first)
        var cooldownField = typeof(WeaponSystem).GetField("currentCooldown",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        cooldownField.SetValue(torpedoLauncher, 0);

        yield return torpedoLauncher.FireWithSpinUp();

        // Verify ammo decreased again
        Assert.AreEqual(4, torpedoLauncher.CurrentAmmo, "Should have 4 ammo after second firing");

        // The UI would reflect this change since it reads CurrentAmmo directly
        // Cleanup
        Object.DestroyImmediate(torpedoHardpoint);
    }
}
