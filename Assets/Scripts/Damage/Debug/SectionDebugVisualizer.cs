using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Debug visualization for ship sections.
/// Shows section colliders and health status in Scene view.
/// </summary>
public class SectionDebugVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    [SerializeField] private bool showColliders = true;
    [SerializeField] private bool showHealthBars = true;
    [SerializeField] private bool showLabels = true;

    [Header("Colors")]
    [SerializeField] private Color healthyColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Color damagedColor = new Color(1f, 1f, 0f, 0.3f);
    [SerializeField] private Color criticalColor = new Color(1f, 0.5f, 0f, 0.3f);
    [SerializeField] private Color breachedColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private Color coreColor = new Color(0f, 0.5f, 1f, 0.3f);

    [Header("Thresholds")]
    [SerializeField] private float damagedThreshold = 0.75f;
    [SerializeField] private float criticalThreshold = 0.25f;

    private SectionManager sectionManager;

    private void Awake()
    {
        sectionManager = GetComponent<SectionManager>();
        if (sectionManager == null)
        {
            sectionManager = GetComponentInChildren<SectionManager>();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showColliders) return;

        // Try to find sections at runtime or in editor
        List<ShipSection> sections = GetSections();

        foreach (ShipSection section in sections)
        {
            if (section == null) continue;

            DrawSectionGizmo(section);
        }
    }

    private List<ShipSection> GetSections()
    {
        List<ShipSection> sections = new List<ShipSection>();

        if (sectionManager != null)
        {
            sections = sectionManager.GetAllSections();
        }

        // Fallback: find sections in children
        if (sections.Count == 0)
        {
            ShipSection[] childSections = GetComponentsInChildren<ShipSection>();
            sections.AddRange(childSections);
        }

        return sections;
    }

    private void DrawSectionGizmo(ShipSection section)
    {
        Color sectionColor = GetSectionColor(section);
        Gizmos.color = sectionColor;

        // Get collider bounds
        Collider col = section.GetComponent<Collider>();
        if (col != null)
        {
            if (col is BoxCollider boxCol)
            {
                // Draw box
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = section.transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.center, boxCol.size);
                Gizmos.color = new Color(sectionColor.r, sectionColor.g, sectionColor.b, 1f);
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (col is SphereCollider sphereCol)
            {
                // Draw sphere
                Vector3 worldCenter = section.transform.TransformPoint(sphereCol.center);
                float worldRadius = sphereCol.radius * Mathf.Max(section.transform.lossyScale.x, section.transform.lossyScale.y, section.transform.lossyScale.z);
                Gizmos.DrawSphere(worldCenter, worldRadius);
                Gizmos.color = new Color(sectionColor.r, sectionColor.g, sectionColor.b, 1f);
                Gizmos.DrawWireSphere(worldCenter, worldRadius);
            }
        }

        // Draw labels
        if (showLabels)
        {
#if UNITY_EDITOR
            string label = $"{section.SectionType}";
            if (showHealthBars)
            {
                float armorPct = section.GetArmorPercentage() * 100f;
                float structPct = section.GetStructurePercentage() * 100f;
                label += $"\nA:{armorPct:F0}% S:{structPct:F0}%";
            }
            if (section.IsBreached)
            {
                label += "\nBREACHED";
            }
            UnityEditor.Handles.Label(section.transform.position + Vector3.up * 0.5f, label);
#endif
        }
    }

    private Color GetSectionColor(ShipSection section)
    {
        // Core has special color
        if (section.SectionType == SectionType.Core)
        {
            return section.IsBreached ? breachedColor : coreColor;
        }

        // Breached sections are red
        if (section.IsBreached)
        {
            return breachedColor;
        }

        // Color based on total health percentage
        float totalMax = section.MaxArmor + section.MaxStructure;
        float totalCurrent = section.CurrentArmor + section.CurrentStructure;
        float healthPct = totalMax > 0 ? totalCurrent / totalMax : 0f;

        if (healthPct > damagedThreshold)
        {
            return healthyColor;
        }
        else if (healthPct > criticalThreshold)
        {
            return damagedColor;
        }
        else
        {
            return criticalColor;
        }
    }
}
