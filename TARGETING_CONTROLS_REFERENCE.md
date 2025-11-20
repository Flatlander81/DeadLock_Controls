# Targeting UI - Controls Reference Card

Quick reference for testing the Targeting UI system.

---

## 🎯 SELECTION

| Input | Action |
|-------|--------|
| **Left Click Ship** | Select ship |
| **Left Click Empty Space** | Deselect all |
| **ESC** | Deselect all |

**Visual Feedback:**
- 💠 **Cyan Ring** = Enemy Selected
- 💚 **Green Ring** = Player Selected

---

## 🔥 WEAPON FIRING

**Must select enemy first!**

| Key | Group | Color | Action |
|-----|-------|-------|--------|
| **1** | Group 1 | 🔵 Blue | Fire Group 1 |
| **2** | Group 2 | 🔴 Red | Fire Group 2 |
| **3** | Group 3 | 🟢 Green | Fire Group 3 |
| **4** | Group 4 | 🟡 Yellow | Fire Group 4 |
| **A** | All | 🔥 Multi | **ALPHA STRIKE** |

---

## ⚙️ WEAPON CONFIGURATION

**Click Hephaestus first!**

| Action | Result |
|--------|--------|
| Click group button | Cycle: Unassigned → 1 → 2 → 3 → 4 → Unassigned |
| Button color | Matches group color |

**Group Colors:**
- Unassigned = ⚪ White
- Group 1 = 🔵 Blue
- Group 2 = 🔴 Red
- Group 3 = 🟢 Green
- Group 4 = 🟡 Yellow

---

## ⚡ ABILITIES

| Key | Ability |
|-----|---------|
| **5** | Ability slot 5 |
| **6** | Ability slot 6 |

**Note:** Keys 1-4 activate abilities when NO enemy targeted

---

## 🚀 MOVEMENT

**⚠️ PLAYER SHIP (HEPHAESTUS) ONLY!**

| Key | Action |
|-----|--------|
| **M** | Toggle Movement Mode |
| **Drag Projection** | Plan movement |
| **E** | Elevation adjust mode |
| **R** | Rotation adjust mode |
| **SPACE** / **ENTER** | Confirm movement |
| **ESC** | Cancel movement |

**Note:** You cannot move enemy ships - they are for targeting only!

---

## 📊 UI PANELS

### Weapon Config Panel (Left)
**When:** Player ship selected
**Shows:**
- Weapon list
- Group assignments
- Weapon stats
- Cooldown status

### Weapon Group Panel (Right)
**When:** Enemy ship selected
**Shows:**
- 4 group buttons
- Weapons per group
- Heat costs
- Arc warnings
- Cooldown warnings
- Alpha Strike button

---

## ⚠️ WARNINGS

| Warning | Meaning |
|---------|---------|
| **[OUT OF ARC]** | Target not in weapon's firing arc |
| **[COOLDOWN]** | Weapon recharging (X turns) |
| **(CD:X)** | Specific weapon cooldown remaining |
| **(No Arc)** | Individual weapon out of arc |
| **(Ready)** | Weapon ready to fire (green) |
| **Heat Warning** | Firing will exceed heat capacity |

---

## 🎨 VISUAL INDICATORS

| Visual | Meaning |
|--------|---------|
| Rotating ring | Ship selected |
| Cyan ring | Enemy selected |
| Green ring | Player selected |
| Colored line | Weapon group targeting |
| Blue line | Group 1 targeting |
| Red line | Group 2 targeting |
| Green line | Group 3 targeting |
| Yellow line | Group 4 targeting |

---

## 🧪 QUICK TESTS

### Test 1: Basic Targeting
```
1. Click Enemy1
2. Press 1
   → See blue line
   → Weapons fire
```

### Test 2: Configure Weapons
```
1. Click Hephaestus
2. Click group button (Rail Gun)
   → Cycles through groups
   → Color changes
```

### Test 3: Multi-Target
```
1. Assign: Rail Gun → Group 1, Cannon → Group 2
2. Click Enemy1, Press 1
3. Click Enemy2, Press 2
   → Two different colored lines
```

### Test 4: Alpha Strike
```
1. Click Enemy
2. Press A
   → ALL weapons fire!
   → Multiple effects
```

### Test 5: Arc Warning
```
1. Click Enemy3 (behind player)
2. Check panel
   → Cannon shows [OUT OF ARC]
   → RailGun shows (Ready)
```

---

## 🔧 TROUBLESHOOTING

**Nothing happens when clicking?**
→ Check TargetingController exists
→ Check colliders on ships

**Weapons won't fire?**
→ Select enemy first
→ Assign weapons to groups
→ Check arc and range

**Panels don't show?**
→ Check UIManager exists
→ Look in Console for errors

**Hotkeys not working?**
→ Enemy must be selected for 1-4
→ Check TargetingController active

**Enemy ship moving when clicked?**
→ This is a bug! Should be fixed now
→ Only Hephaestus should move
→ Re-run "Tools → Setup Targeting Test Scene"

---

## 📍 TEST SCENE LAYOUT

```
              Enemy2 (Front)
                  ▲
                  │
                  │ 15 units
                  │
                  │
   Enemy1 ────────●──────── Hephaestus (Player)
   (Right)      15 units       green wireframe
                  │
                  │
                  │ 15 units
                  │
                  ▼
              Enemy3 (Behind)
```

**Purpose:**
- Enemy1 (Right): Test side targeting
- Enemy2 (Front): Test forward targeting
- Enemy3 (Behind): Test arc warnings

---

## 💡 PRO TIPS

1. **Assign Before Firing**
   - Configure weapons first
   - Then select targets and fire

2. **Use Arc Testing**
   - Enemy3 great for testing Cannon arc limits
   - RailGuns can hit anything (360°)

3. **Multi-Targeting**
   - Assign different groups to different targets
   - See multiple colored lines simultaneously

4. **Heat Management**
   - Watch heat cost totals in panel
   - Alpha Strike shows total heat impact

5. **Visual Clarity**
   - Colored lines match group colors
   - Ring colors distinguish friend/enemy
   - Warnings prevent mistakes

---

**Ready to command! Click, configure, and conquer!** ⚔️
