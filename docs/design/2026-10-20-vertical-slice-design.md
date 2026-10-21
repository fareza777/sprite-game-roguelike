# OATHDEEP — Vertical Slice Design

Working title: **OATHDEEP** — a knight sworn to descend the hundred floors of the Deep.
Engine: Unity 6000.3.20f1, URP 2D, Android, portrait, one-handed.
Art: drawn in Aseprite via the `aseprite-ai-artist` MCP. Engine work via the Unity MCP.

## Intent
Prove the core moment: *a knight in a torch-lit crypt fighting enemies, and it feels great on a phone.*
Everything else (100 floors, many heroes, menus) is built on top of this slice later.

## Slice scope (goals)
- **One floor**: 5–7 procedural rooms (start, combat x3–5, treasure, miniboss) on a grid, connected by doors.
  Combat rooms lock until cleared.
- **Biome**: "Sunken Crypt" — stone floor, mossy walls, torches with 2D lights, dust motes.
- **Hero: Knight** — 3-hit sword combo, dodge roll with i-frames, active skill *Shield Bash* (stun + knockback),
  crit chance, HP.
- **Enemies**: Skeleton (melee, telegraphed lunge), Bone Archer (keeps distance, telegraphed arrow),
  Crypt Slime (hops, splits on death into 2 small slimes).
- **Miniboss**: *Bone Warden* — 3 attacks (sweep, ground slam shockwave, summon 2 skeletons), phase 2 at 50% HP.
- **Game feel**: hit-stop, camera shake, white hit-flash, knockback, damage numbers (crit in gold),
  hit particles, haptics on Android.
- **Controls**: floating virtual joystick (left), Attack / Dodge / Skill buttons (right thumb zone),
  auto-aim to nearest enemy in facing cone. Keyboard+mouse in editor for testing.
- **Loot**: gold, health potion, 3–5 equippable items (sword / armor / ring) with rarity colours,
  stat changes applied immediately.
- **Loop**: Title → Play → floor → die (Game Over, stats) or kill Warden (Victory) → restart.

## Non-goals for the slice
Multiple heroes, skill tree, crafting, meta-progression, full settings, codex, achievements, 100 floors.
The architecture must make them additive (data-driven ScriptableObjects, no hard-coded content).

## Anti-goals
- Placeholder-looking art in the slice once art pass is done.
- Controls that feel floaty or input that gets eaten.
- Frame drops on a mid-range phone (target 60 FPS, stable).

## Technical decisions (Claude's calls)
- Pixel scale: 16 px tiles, 16 PPU, characters ~32×32, Pixel Perfect Camera ref 180×320.
- Data: ScriptableObjects for heroes, enemies, items, rooms, biomes.
- Combat: Hitbox/Hurtbox components + `DamageInfo` struct; pure-C# damage calc (unit tested).
- Dungeon: pure-C# layout generator (seeded, unit tested) → Tilemap painter.
- Pooling for projectiles, damage numbers, VFX.
- New Input System; on-screen controls are custom UI (not the Input System on-screen sticks) for full layout control later.
