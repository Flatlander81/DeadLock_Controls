using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor automation for setting up ship sections.
/// Provides menu items for adding section infrastructure to ships.
/// </summary>
public static class SectionSetupEditor
{
    /// <summary>
    /// Adds a complete section setup to the selected ship GameObject.
    /// Creates SectionManager and all 7 sections with colliders.
    /// </summary>
    [MenuItem("Hephaestus/Setup/Setup Sections on Selected Ship")]
    public static void SetupSectionsOnSelectedShip()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select a ship GameObject in the hierarchy.", "OK");
            return;
        }

        Ship ship = selected.GetComponent<Ship>();
        if (ship == null)
        {
            EditorUtility.DisplayDialog("Not a Ship", "Selected GameObject does not have a Ship component.", "OK");
            return;
        }

        Undo.SetCurrentGroupName("Setup Ship Sections");
        int undoGroup = Undo.GetCurrentGroup();

        // Add SectionManager if not present
        SectionManager sectionManager = selected.GetComponent<SectionManager>();
        if (sectionManager == null)
        {
            sectionManager = Undo.AddComponent<SectionManager>(selected);
        }

        // Create sections container
        GameObject sectionsContainer = new GameObject("Sections");
        Undo.RegisterCreatedObjectUndo(sectionsContainer, "Create Sections Container");
        sectionsContainer.transform.SetParent(selected.transform);
        sectionsContainer.transform.localPosition = Vector3.zero;
        sectionsContainer.transform.localRotation = Quaternion.identity;

        // Create each section type
        foreach (SectionType sectionType in SectionDefinitions.GetAllSectionTypes())
        {
            CreateSection(sectionsContainer.transform, sectionType, ship);
        }

        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log($"[SectionSetupEditor] Setup complete for {selected.name} - 7 sections created");
        EditorUtility.DisplayDialog("Setup Complete", $"Created 7 sections on {selected.name}.\n\nSections: Fore, Aft, Port, Starboard, Dorsal, Ventral, Core", "OK");
    }

    /// <summary>
    /// Creates a single section with collider based on SectionDefinitions config.
    /// </summary>
    private static void CreateSection(Transform parent, SectionType sectionType, Ship ship)
    {
        var config = SectionDefinitions.GetConfig(sectionType);

        // Create section GameObject
        GameObject sectionObj = new GameObject($"Section_{sectionType}");
        Undo.RegisterCreatedObjectUndo(sectionObj, $"Create {sectionType} Section");
        sectionObj.transform.SetParent(parent);
        sectionObj.transform.localPosition = config.ColliderPosition;
        sectionObj.transform.localRotation = Quaternion.identity;

        // Add ShipSection component
        ShipSection section = sectionObj.AddComponent<ShipSection>();
        section.Initialize(sectionType, ship);

        // Add collider based on config
        if (config.UseSphereCollider)
        {
            SphereCollider sphereCol = sectionObj.AddComponent<SphereCollider>();
            sphereCol.radius = config.SphereRadius;
            sphereCol.isTrigger = true;
        }
        else
        {
            BoxCollider boxCol = sectionObj.AddComponent<BoxCollider>();
            boxCol.size = config.ColliderSize;
            boxCol.isTrigger = true;
        }

        // Add hit detector
        SectionHitDetector hitDetector = sectionObj.AddComponent<SectionHitDetector>();
        hitDetector.SetParentSection(section);

        Debug.Log($"[SectionSetupEditor] Created section: {sectionType} at {config.ColliderPosition}");
    }

    /// <summary>
    /// Removes all section components from the selected ship.
    /// </summary>
    [MenuItem("Hephaestus/Setup/Remove Sections from Selected Ship")]
    public static void RemoveSectionsFromSelectedShip()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select a ship GameObject in the hierarchy.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Confirm Removal", $"Remove all sections from {selected.name}?", "Remove", "Cancel"))
        {
            return;
        }

        Undo.SetCurrentGroupName("Remove Ship Sections");
        int undoGroup = Undo.GetCurrentGroup();

        // Find and destroy Sections container
        Transform sectionsContainer = selected.transform.Find("Sections");
        if (sectionsContainer != null)
        {
            Undo.DestroyObjectImmediate(sectionsContainer.gameObject);
        }

        // Remove SectionManager
        SectionManager sectionManager = selected.GetComponent<SectionManager>();
        if (sectionManager != null)
        {
            Undo.DestroyObjectImmediate(sectionManager);
        }

        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log($"[SectionSetupEditor] Removed sections from {selected.name}");
    }

    /// <summary>
    /// Validates that a ship is selected for setup.
    /// </summary>
    [MenuItem("Hephaestus/Setup/Setup Sections on Selected Ship", true)]
    public static bool ValidateSetupSections()
    {
        return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Ship>() != null;
    }

    /// <summary>
    /// Validates that a ship is selected for removal.
    /// </summary>
    [MenuItem("Hephaestus/Setup/Remove Sections from Selected Ship", true)]
    public static bool ValidateRemoveSections()
    {
        return Selection.activeGameObject != null;
    }
}
