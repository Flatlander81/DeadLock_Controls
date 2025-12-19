using UnityEngine;
using System;

/// <summary>
/// Coordinates all damage UI elements.
/// Subscribes to damage events and updates UI components.
/// </summary>
public class DamageUIManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private SectionStatusPanel sectionStatusPanel;
    [SerializeField] private SectionDetailPopup sectionDetailPopup;
    [SerializeField] private CombatLogPanel combatLogPanel;
    [SerializeField] private ShieldStatusBar shieldStatusBar;

    [Header("References")]
    [SerializeField] private Ship playerShip;

    [Header("Settings")]
    [SerializeField] private bool autoCreateComponents = true;
    [SerializeField] private bool showDamageUI = true;

    // Cached references
    private SectionManager sectionManager;
    private ShieldSystem shieldSystem;
    private DamageRouter damageRouter;
    private ShipDeathController deathController;
    private CoreProtectionSystem coreProtection;

    // Properties
    public SectionStatusPanel SectionPanel => sectionStatusPanel;
    public SectionDetailPopup DetailPopup => sectionDetailPopup;
    public CombatLogPanel CombatLog => combatLogPanel;
    public ShieldStatusBar ShieldBar => shieldStatusBar;
    public Ship PlayerShip => playerShip;
    public bool IsVisible => showDamageUI;

    private void Awake()
    {
        if (autoCreateComponents)
        {
            EnsureComponentsExist();
        }
    }

    private void Start()
    {
        // Find player ship if not assigned
        if (playerShip == null)
        {
            playerShip = FindFirstObjectByType<Ship>();
        }

        if (playerShip != null)
        {
            Initialize(playerShip);
        }
    }

    /// <summary>
    /// Ensure all UI components exist.
    /// </summary>
    private void EnsureComponentsExist()
    {
        if (sectionStatusPanel == null)
        {
            GameObject panelObj = new GameObject("SectionStatusPanel");
            panelObj.transform.SetParent(transform);
            sectionStatusPanel = panelObj.AddComponent<SectionStatusPanel>();
        }

        if (sectionDetailPopup == null)
        {
            GameObject popupObj = new GameObject("SectionDetailPopup");
            popupObj.transform.SetParent(transform);
            sectionDetailPopup = popupObj.AddComponent<SectionDetailPopup>();
        }

        if (combatLogPanel == null)
        {
            GameObject logObj = new GameObject("CombatLogPanel");
            logObj.transform.SetParent(transform);
            combatLogPanel = logObj.AddComponent<CombatLogPanel>();
        }

        if (shieldStatusBar == null)
        {
            GameObject shieldObj = new GameObject("ShieldStatusBar");
            shieldObj.transform.SetParent(transform);
            shieldStatusBar = shieldObj.AddComponent<ShieldStatusBar>();
        }
    }

    /// <summary>
    /// Initialize with a target ship.
    /// </summary>
    public void Initialize(Ship ship)
    {
        // Unsubscribe from previous ship
        UnsubscribeFromEvents();

        playerShip = ship;

        if (playerShip == null)
        {
            Debug.LogWarning("[DamageUIManager] No ship to initialize");
            return;
        }

        // Get references
        sectionManager = playerShip.SectionManager;
        shieldSystem = playerShip.ShieldSystem;
        damageRouter = playerShip.DamageRouter;
        deathController = playerShip.DeathController;
        coreProtection = playerShip.CoreProtection;

        // Initialize UI components
        if (sectionStatusPanel != null)
        {
            sectionStatusPanel.Initialize(playerShip);
            sectionStatusPanel.OnSectionClicked += HandleSectionClicked;
        }

        if (shieldStatusBar != null)
        {
            shieldStatusBar.Initialize(playerShip);
        }

        // Subscribe to events
        SubscribeToEvents();

        Debug.Log($"[DamageUIManager] Initialized for {playerShip.gameObject.name}");
    }

    /// <summary>
    /// Subscribe to ship damage events.
    /// </summary>
    private void SubscribeToEvents()
    {
        if (sectionManager != null)
        {
            sectionManager.OnSectionArmorDamaged += HandleArmorDamaged;
            sectionManager.OnSectionStructureDamaged += HandleStructureDamaged;
            sectionManager.OnSectionBreached += HandleSectionBreached;
        }

        if (deathController != null)
        {
            deathController.OnShipDestroyed += HandleShipDestroyed;
            deathController.OnShipDisabled += HandleShipDisabled;
        }
    }

    /// <summary>
    /// Unsubscribe from events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (sectionManager != null)
        {
            sectionManager.OnSectionArmorDamaged -= HandleArmorDamaged;
            sectionManager.OnSectionStructureDamaged -= HandleStructureDamaged;
            sectionManager.OnSectionBreached -= HandleSectionBreached;
        }

        if (deathController != null)
        {
            deathController.OnShipDestroyed -= HandleShipDestroyed;
            deathController.OnShipDisabled -= HandleShipDisabled;
        }

        if (sectionStatusPanel != null)
        {
            sectionStatusPanel.OnSectionClicked -= HandleSectionClicked;
        }
    }

    // Event handlers
    private void HandleArmorDamaged(ShipSection section, float damage, float remaining)
    {
        // Log armor damage
        if (combatLogPanel != null && playerShip != null)
        {
            // Note: Full damage report would come from DamageRouter
            // This is a simplified log for armor-only hits
        }
    }

    private void HandleStructureDamaged(ShipSection section, float damage, float remaining)
    {
        // Check for critical hit
        var critResult = section.LastCriticalResult;
        if (critResult.SystemWasDamaged || critResult.SystemWasDestroyed)
        {
            if (combatLogPanel != null && playerShip != null)
            {
                combatLogPanel.LogCritical(
                    playerShip.gameObject.name,
                    section.SectionType,
                    critResult.SystemTypeHit,
                    critResult.SystemWasDestroyed
                );
            }
        }
    }

    private void HandleSectionBreached(ShipSection section)
    {
        if (combatLogPanel != null && playerShip != null)
        {
            combatLogPanel.LogBreach(playerShip.gameObject.name, section.SectionType);
        }
    }

    private void HandleShipDestroyed(Ship ship, ShipDeathController.DeathCause cause)
    {
        if (combatLogPanel != null)
        {
            combatLogPanel.LogDeath(ship.gameObject.name, cause);
        }
    }

    private void HandleShipDisabled(Ship ship)
    {
        if (combatLogPanel != null)
        {
            combatLogPanel.LogDisabled(ship.gameObject.name);
        }
    }

    private void HandleSectionClicked(ShipSection section)
    {
        if (sectionDetailPopup != null)
        {
            sectionDetailPopup.ShowSection(section);
        }
    }

    /// <summary>
    /// Process a damage report and update UI accordingly.
    /// Call this after damage is applied via DamageRouter.
    /// </summary>
    public void ProcessDamageReport(DamageReport report, string shipName = null)
    {
        if (shipName == null && playerShip != null)
        {
            shipName = playerShip.gameObject.name;
        }

        if (combatLogPanel == null) return;

        // Log shield damage
        if (report.ShieldDamage > 0)
        {
            combatLogPanel.LogShieldEvent(shipName, report.ShieldDamage, report.ShieldsDepleted);

            // Flash shield bar
            if (shieldStatusBar != null)
            {
                shieldStatusBar.Flash();
            }
        }

        // Log the hit
        if (report.TotalDamageApplied > 0)
        {
            combatLogPanel.LogHit(
                shipName,
                report.SectionHit,
                report.TotalIncomingDamage,
                report.ShieldDamage,
                report.ArmorDamage,
                report.StructureDamage
            );
        }

        // Log critical hit
        if (report.HadCritical && report.CriticalResult.HasValue)
        {
            var crit = report.CriticalResult.Value;
            if (crit.SystemWasDamaged || crit.SystemWasDestroyed)
            {
                combatLogPanel.LogCritical(
                    shipName,
                    report.SectionHit,
                    crit.SystemTypeHit,
                    crit.SystemWasDestroyed
                );
            }
        }

        // Log breach
        if (report.SectionBreached)
        {
            combatLogPanel.LogBreach(shipName, report.SectionHit);
        }

        // Log lucky shot
        if (report.WasLuckyShot)
        {
            combatLogPanel.LogMessage(
                $"LUCKY SHOT! Damage punched through to Core!",
                CombatLogPanel.LogCategory.Critical,
                Color.magenta
            );
        }

        // Log core protection
        if (report.CoreWasProtected)
        {
            combatLogPanel.LogMessage(
                $"Core protected - damage redirected to {report.SectionHit}",
                CombatLogPanel.LogCategory.Hit,
                Color.cyan
            );
        }
    }

    /// <summary>
    /// Show all damage UI panels.
    /// </summary>
    public void Show()
    {
        showDamageUI = true;

        if (sectionStatusPanel != null) sectionStatusPanel.Show();
        if (shieldStatusBar != null) shieldStatusBar.Show();
        if (combatLogPanel != null) combatLogPanel.Show();
    }

    /// <summary>
    /// Hide all damage UI panels.
    /// </summary>
    public void Hide()
    {
        showDamageUI = false;

        if (sectionStatusPanel != null) sectionStatusPanel.Hide();
        if (sectionDetailPopup != null) sectionDetailPopup.Hide();
        if (shieldStatusBar != null) shieldStatusBar.Hide();
        if (combatLogPanel != null) combatLogPanel.Hide();
    }

    /// <summary>
    /// Toggle damage UI visibility.
    /// </summary>
    public void Toggle()
    {
        if (showDamageUI)
            Hide();
        else
            Show();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// Manually set component references (for testing).
    /// </summary>
    public void SetComponents(
        SectionStatusPanel sectionPanel,
        SectionDetailPopup detailPopup,
        CombatLogPanel logPanel,
        ShieldStatusBar shieldBar)
    {
        sectionStatusPanel = sectionPanel;
        sectionDetailPopup = detailPopup;
        combatLogPanel = logPanel;
        shieldStatusBar = shieldBar;
    }
}
