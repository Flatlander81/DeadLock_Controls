using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime test controller for Heat and Cooldown integration testing.
/// Provides OnGUI controls for testing heat dissipation and cooldown ticking.
/// </summary>
public class HeatCooldownTestController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnEndProcessor turnEndProcessor;
    [SerializeField] private Ship testShip;

    [Header("Test Settings")]
    [SerializeField] private float heatToAdd = 50f;
    [SerializeField] private int cooldownToSet = 3;

    [Header("Event Log")]
    [SerializeField] private List<string> eventLog = new List<string>();
    private const int MAX_LOG_ENTRIES = 8;

    // Cached references
    private HeatManager heatManager;
    private WeaponManager weaponManager;
    private AbilitySystem abilitySystem;
    private SystemDegradationManager degradationManager;

    // State tracking
    private float heatBeforeTurn;
    private Dictionary<string, int> cooldownsBeforeTurn = new Dictionary<string, int>();

    // GUI state
    private Vector2 scrollPosition;
    private bool showEventLog = true;

    private void Start()
    {
        // Auto-find references if not set
        if (turnEndProcessor == null)
        {
            turnEndProcessor = FindAnyObjectByType<TurnEndProcessor>();
        }

        if (testShip == null)
        {
            testShip = FindAnyObjectByType<Ship>();
        }

        if (testShip != null)
        {
            CacheShipReferences();
            SubscribeToEvents();
        }

        LogEvent("Initialized");
    }

    private void CacheShipReferences()
    {
        heatManager = testShip.GetComponent<HeatManager>();
        weaponManager = testShip.GetComponent<WeaponManager>();
        abilitySystem = testShip.GetComponent<AbilitySystem>();
        degradationManager = testShip.GetComponent<SystemDegradationManager>();
    }

    private void SubscribeToEvents()
    {
        if (turnEndProcessor != null)
        {
            turnEndProcessor.OnHeatDissipated += HandleHeatDissipated;
            turnEndProcessor.OnWeaponReady += HandleWeaponReady;
            turnEndProcessor.OnAbilityReady += HandleAbilityReady;
        }

        if (heatManager != null)
        {
            heatManager.OnHeatDissipated += (amount) => LogEvent($"Dissipated {amount:F1}");
        }
    }

    private void OnDestroy()
    {
        if (turnEndProcessor != null)
        {
            turnEndProcessor.OnHeatDissipated -= HandleHeatDissipated;
            turnEndProcessor.OnWeaponReady -= HandleWeaponReady;
            turnEndProcessor.OnAbilityReady -= HandleAbilityReady;
        }
    }

    private void HandleHeatDissipated(Ship ship, float amount)
    {
        LogEvent($"{ship.gameObject.name}: -{amount:F1} heat");
    }

    private void HandleWeaponReady(WeaponSystem weapon)
    {
        LogEvent($"{weapon.WeaponName} ready!");
    }

    private void HandleAbilityReady(string abilityName)
    {
        LogEvent($"Ability '{abilityName}' ready!");
    }

    private void LogEvent(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        eventLog.Insert(0, $"[{timestamp}] {message}");

        while (eventLog.Count > MAX_LOG_ENTRIES)
        {
            eventLog.RemoveAt(eventLog.Count - 1);
        }

        Debug.Log($"[HeatCooldownTest] {message}");
    }

    private void OnGUI()
    {
        // Position on right side with compact layout
        float panelWidth = 280f;
        float panelHeight = Mathf.Min(Screen.height - 20, 500f);
        Rect panelRect = new Rect(Screen.width - panelWidth - 10, 10, panelWidth, panelHeight);

        GUILayout.BeginArea(panelRect, GUI.skin.box);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("HEAT/COOLDOWN TEST", GetHeaderStyle());

        // Ship status - compact
        DrawShipStatusCompact();

        // Heat controls
        DrawHeatControlsCompact();

        // Cooldown controls
        DrawCooldownControlsCompact();

        // Turn controls
        DrawTurnControlsCompact();

        // Radiator controls
        DrawRadiatorControlsCompact();

        // Event log (collapsible)
        DrawEventLogCompact();

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawShipStatusCompact()
    {
        GUILayout.Label("Status", GetSubHeaderStyle());

        if (testShip == null)
        {
            GUILayout.Label("No ship!");
            return;
        }

        if (heatManager != null)
        {
            float heatPercent = heatManager.CurrentHeat / heatManager.MaxHeat * 100f;
            GUILayout.Label($"Heat: {heatManager.CurrentHeat:F0}/{heatManager.MaxHeat:F0} ({heatPercent:F0}%) - {heatManager.GetCurrentTier()}");
        }

        if (weaponManager != null)
        {
            GUILayout.BeginHorizontal();
            var weapons = weaponManager.Weapons;
            foreach (var weapon in weapons)
            {
                string status = weapon.CurrentCooldown > 0 ? $"CD:{weapon.CurrentCooldown}" : "RDY";
                GUILayout.Label($"{weapon.WeaponName}:{status}", GUILayout.Width(130));
            }
            GUILayout.EndHorizontal();
        }
    }

    private void DrawHeatControlsCompact()
    {
        GUILayout.Label("Heat", GetSubHeaderStyle());

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+25", GUILayout.Width(40))) AddHeat(25f);
        if (GUILayout.Button("+50", GUILayout.Width(40))) AddHeat(50f);
        if (GUILayout.Button("+100", GUILayout.Width(45))) AddHeat(100f);
        if (GUILayout.Button("Reset", GUILayout.Width(50))) ResetHeat();
        GUILayout.EndHorizontal();
    }

    private void DrawCooldownControlsCompact()
    {
        GUILayout.Label("Cooldowns", GetSubHeaderStyle());

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set CD=3")) SetAllWeaponCooldowns(3);
        if (GUILayout.Button("Reset CD")) SetAllWeaponCooldowns(0);
        GUILayout.EndHorizontal();
    }

    private void DrawTurnControlsCompact()
    {
        GUILayout.Label("Turn", GetSubHeaderStyle());

        int currentTurn = TurnManager.Instance != null ? TurnManager.Instance.CurrentTurn : 0;

        if (turnEndProcessor != null)
        {
            float dissipation = turnEndProcessor.CalculateDissipation(testShip);
            GUILayout.Label($"Turn {currentTurn} | Dissipation: {dissipation:F1}/turn");
        }
        else
        {
            GUILayout.Label($"Turn {currentTurn}");
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Advance Turn")) AdvanceTurn();
        if (GUILayout.Button("Process End")) ProcessTurnEndOnly();
        GUILayout.EndHorizontal();
    }

    private void DrawRadiatorControlsCompact()
    {
        GUILayout.Label("Radiators", GetSubHeaderStyle());

        if (degradationManager == null) return;

        var radiators = degradationManager.GetRadiators();
        foreach (var radiator in radiators)
        {
            GUILayout.BeginHorizontal();
            string state = radiator.IsDestroyed ? "X" : radiator.IsDamaged ? "DMG" : "OK";
            string shortName = radiator.gameObject.name.Replace("Radiator", "Rad");
            GUILayout.Label($"{shortName}:{state}", GUILayout.Width(120));

            if (!radiator.IsDestroyed)
            {
                if (GUILayout.Button("Hit", GUILayout.Width(40)))
                {
                    DamageRadiator(radiator);
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    private void DrawEventLogCompact()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Log", GetSubHeaderStyle());
        if (GUILayout.Button(showEventLog ? "[-]" : "[+]", GUILayout.Width(30)))
        {
            showEventLog = !showEventLog;
        }
        GUILayout.EndHorizontal();

        if (showEventLog)
        {
            foreach (string entry in eventLog)
            {
                GUILayout.Label(entry, GetLogStyle());
            }
        }
    }

    // Helper methods

    private void AddHeat(float amount)
    {
        if (heatManager != null)
        {
            heatManager.AddPlannedHeat(amount);
            heatManager.CommitPlannedHeat();
            LogEvent($"+{amount:F0} heat -> {heatManager.CurrentHeat:F0}");
        }
    }

    private void ResetHeat()
    {
        if (heatManager != null)
        {
            heatManager.Reset();
            LogEvent("Heat reset");
        }
    }

    private void SetAllWeaponCooldowns(int cooldown)
    {
        if (weaponManager == null) return;

        var weapons = weaponManager.Weapons;
        foreach (var weapon in weapons)
        {
            // Use reflection to set cooldown directly for testing
            var field = typeof(WeaponSystem).GetField("currentCooldown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(weapon, cooldown);
        }
        LogEvent($"All CD={cooldown}");
    }

    private void AdvanceTurn()
    {
        RecordStateBeforeTurn();

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.ForceEndTurn();
            LogEvent($"Turn {TurnManager.Instance.CurrentTurn}");
        }

        ShowStateChanges();
    }

    private void ProcessTurnEndOnly()
    {
        if (turnEndProcessor == null) return;

        RecordStateBeforeTurn();
        turnEndProcessor.ProcessTurnEnd(TurnManager.Instance?.CurrentTurn ?? 0);
        LogEvent("Processed end");
        ShowStateChanges();
    }

    private void RecordStateBeforeTurn()
    {
        if (heatManager != null)
        {
            heatBeforeTurn = heatManager.CurrentHeat;
        }

        cooldownsBeforeTurn.Clear();
        if (weaponManager != null)
        {
            foreach (var weapon in weaponManager.Weapons)
            {
                cooldownsBeforeTurn[weapon.WeaponName] = weapon.CurrentCooldown;
            }
        }
    }

    private void ShowStateChanges()
    {
        if (heatManager != null)
        {
            float heatChange = heatManager.CurrentHeat - heatBeforeTurn;
            if (Mathf.Abs(heatChange) > 0.01f)
            {
                LogEvent($"{heatBeforeTurn:F0}->{heatManager.CurrentHeat:F0} ({heatChange:+0;-0})");
            }
        }
    }

    private void DamageRadiator(MountedRadiator radiator)
    {
        radiator.TakeCriticalHit();
        string state = radiator.IsDestroyed ? "destroyed" : "damaged";
        LogEvent($"{radiator.gameObject.name} {state}");
        degradationManager?.RecalculateAllMultipliers();
    }

    // GUI Styles

    private GUIStyle GetHeaderStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 14;
        style.alignment = TextAnchor.MiddleCenter;
        return style;
    }

    private GUIStyle GetSubHeaderStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 11;
        return style;
    }

    private GUIStyle GetLogStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 9;
        style.wordWrap = true;
        return style;
    }
}
