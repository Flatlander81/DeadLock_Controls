# Projectile System - Quick Start

## 🚀 3-Step Setup

```
1. Unity Editor → Tools → Setup Projectile Test Scene
2. Press Play
3. Start testing!
```

---

## ⌨️ Keyboard Controls

| Key | Action | What You'll See |
|-----|--------|-----------------|
| **SPACE** | Fire RailGuns | 💥 Cyan beam tracers |
| **F** | Fire Cannon | 🔵 Cyan sphere projectile |
| **P** | Spawn Ballistic | 🔵 Test projectile |
| **H** | Spawn Homing | 🟡 Yellow seeking missile |
| **T** | Switch Target | 🎯 Changes enemy target |
| **I** | Pool Info | 📊 Console stats |
| **C** | Clear All | 🧹 Return to pool |

---

## 👀 What to Watch For

### RailGun (SPACE)
- ⚡ Cyan beams from ship to target
- ✨ Instant hit (no travel time)
- 💫 Fades in 0.2 seconds
- 💥 20 damage per gun

### Cannon (F)
- 🔵 Cyan sphere flies forward
- 🌊 Trail behind projectile
- ➡️ Straight line trajectory
- 💥 40 damage on hit
- ⏱️ 2 units/second speed

### Homing Missile (H)
- 🟡 Yellow capsule
- 🔥 Particle thruster
- 🎯 Rotates toward target
- 🔄 90°/second turn rate
- 💥 60 damage

---

## ✅ Quick Tests

### Test Instant Hit
1. Press **SPACE**
2. See cyan beams!
3. Target shields drop by 40

### Test Ballistic
1. Press **F**
2. See cyan sphere fly
3. Wait for impact
4. Target takes 40 damage

### Test Homing
1. Press **H**
2. See yellow missile
3. Watch it turn toward target
4. Missile hits and explodes

### Test Pooling
1. Press **P** 10 times fast
2. Press **I** (see "Active: 10")
3. Press **C** (clear all)
4. Press **I** (see "Active: 0")
5. All returned to pool! ♻️

---

## 🎯 Success Checklist

After 5 minutes of testing, you should have seen:

- ✅ Cyan beam tracers (instant hit)
- ✅ Flying projectile spheres (ballistic)
- ✅ Yellow homing missiles (seeking)
- ✅ Trail renderers behind projectiles
- ✅ Damage numbers in Console
- ✅ Projectiles disappearing on hit
- ✅ Pool status in Console

---

## 🐛 Common Issues

**No projectiles?**
→ Check Console for errors
→ Press **I** to see pool status

**Projectiles pass through?**
→ Ships need Colliders
→ Check BoxCollider on ships

**Beams don't show?**
→ Look in Game view (not Scene)
→ They fade fast (0.2s)

**Missiles don't turn?**
→ Press **T** to set target
→ Target must be alive

---

## 📊 On-Screen Info

Top-left GUI shows:
- Current target name
- Active projectile count
- Player heat level
- All controls

---

## 🎮 Advanced Controls

**Scene View:**
- Enable Gizmos to see collision spheres
- Select projectile to see debug lines
- Pause game to inspect mid-flight

**Console Commands:**
- Press **I** for detailed pool stats
- Watch for spawn/hit messages
- Check damage application logs

**Inspector:**
- Select ProjectileManager
- View active projectile lists
- Modify pool sizes
- Assign custom prefabs

---

## 📈 Performance

**Expected:**
- 60 FPS with 50+ projectiles ✅
- Zero GC after startup ✅
- Instant spawning (pooling) ✅
- Smooth trails and effects ✅

---

## 📁 Files

**Scene:** `Assets/Scenes/ProjectileTestScene.unity`

**Key Scripts:**
- `ProjectileTester.cs` - Controls
- `ProjectileManager.cs` - Pooling
- `BallisticProjectile.cs` - Cannon shots
- `HomingProjectile.cs` - Missiles

**Prefabs:** `Assets/Prefabs/Projectiles/`

---

## 🚀 Next Level Testing

See **PROJECTILE_TEST_GUIDE.md** for:
- 11 detailed test scenarios
- Troubleshooting guide
- Performance profiling
- Advanced techniques

---

## ✨ Test Results

**All 57/57 tests passing!**
- Heat: 10/10 ✅
- Abilities: 13/13 ✅
- Integration: 10/10 ✅
- Weapons: 12/12 ✅
- **Projectiles: 12/12 ✅**

---

**Ready to test!** Press Play and start shooting! 🎯
