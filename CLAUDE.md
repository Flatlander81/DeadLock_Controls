# Hephaestus - Claude Code Standards

This document defines coding standards and conventions for Claude Code to follow during implementation.

---

## Phase 3: Damage System Standards

### Code Standards
- **DRY**: Extract shared logic into base classes or utility methods. No copy-paste code.
- **Naming**: Follow existing project conventions. Review Ship.cs, WeaponSystem.cs, HeatManager.cs for patterns before creating new files.
- **Events**: Use System.Action for events. Follow the event patterns already established in the codebase.
- **SerializeField**: Expose configuration values in Inspector. Keep runtime state visible for debugging with [Header] attributes to organize.
- **Null Safety**: Defensive null checks in all public methods and Awake/Start. Use TryGetComponent where appropriate.
- **Documentation**: XML summary comments on all public classes and methods.

### Editor Automation Standards
- **All Unity setup must be automated via Editor scripts.** If a human would need to click through menus or drag references, write a script instead.
- **Menu items**: Use "Hephaestus/Setup/" for configuration scripts, "Hephaestus/Testing/" for test scene creation.
- **Layer/Tag creation**: Editor scripts must create required layers/tags automatically if missing.
- **Prefab modification**: Editor scripts should work on selected GameObject OR create new test objects as appropriate.

### Testing Standards
- **Test scenes**: Every test scene must be creatable via menu item. Zero manual setup.
- **Test controllers**: Runtime test UI using OnGUI for quick verification. Include buttons for common test actions.
- **Debug visualization**: Gizmos and runtime visualizers for all spatial systems. Color-coded by state.
- **Unit tests**: Every new system needs comprehensive unit tests. Follow existing test patterns in Assets/Tests/PlayModeTests/.

### Folder Structure for Phase 3
```
Assets/Scripts/Damage/              # Core damage system
Assets/Scripts/Damage/Sections/     # Section-related components
Assets/Scripts/Damage/Systems/      # Mounted systems (weapons, engines, etc.)
Assets/Scripts/Damage/Debug/        # Debug visualizers and test controllers
Assets/Editor/DamageSystem/         # Editor automation scripts
Assets/Tests/PlayModeTests/DamageSystem/  # Unit tests
Assets/Scenes/Testing/              # Test scenes (auto-generated)
```

### GDD Reference Data (Hephaestus)

**Section Stats:**
| Section    | Armor | Structure | Notes |
|------------|-------|-----------|-------|
| Fore       | 100   | 50        | Front, forward weapons |
| Aft        | 60    | 40        | Rear, engines |
| Port       | 80    | 50        | Left side |
| Starboard  | 80    | 50        | Right side |
| Dorsal     | 70    | 40        | Top |
| Ventral    | 70    | 40        | Bottom |
| Core       | 0     | 30        | Protected center, special access rules |

**Shield Stats:**
- Max Shields: 200 (single bubble)
- Regeneration: None
- Restoration: Shield Boost ability only (+100, requires shields = 0)

---
