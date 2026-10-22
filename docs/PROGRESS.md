# UNDERSUN: Progress and Handoff

UNDERSUN (formerly the OATHDEEP vertical slice) is a portrait, one-handed pixel-art action roguelike for Android, built
in Unity 6. Design doc: `docs/design/2026-10-21-undersun-full-game-design.md`.

## Status (2026-10-21): full game, milestones M1-M9 done
| Milestone | Content |
|---|---|
| M1 | Brand, app icon, splash, main menu (settings / about / share / rate), joystick + controls fix |
| M2 | Original story, narrated prologue and ending cinematics with offline VO, per-floor narration |
| M3 | Five heroes (Aldric, Wren, Morrow, Brakka, Sable) with their own stats, combos and skills; Heroes page |
| M4 | 104 item identities (64 bases + 40 legendaries), 24 affixes, 5 rarities, procedural stats; equipment + compendium |
| M5 | Ten biomes, a ten-floor descent, bigger rooms (16-20 per floor), merchant / sunstone / pilgrim / rest / events / elites |
| M6 | Forty creatures (30 regulars, 10 Chorister bosses), new AIs, data-driven pattern bosses, Codex |
| M7 | Main story chapters, 12 side quests, permanent meta rewards, Quests page |
| M8 | Nine biome themes + two boss themes, 34 SFX |
| M9 | Ten-floor smoke test, Android APK, README, push to GitHub |

## Toolchain
- Unity 6000.3.20f1 project at `Oathdeep/` (URP 2D, Android, IL2CPP ARM64, min SDK 25, package `com.fareza777.undersun`).
- Unity MCP (IvanMurzak) over HTTP: `node tools/unity-mcp.mjs <tool> '<json>'`; run C# in the editor with
  `node tools/run-cs.mjs file.cs` (`class Script { public static string Main() }`).
  - The editor only ticks while an MCP call is in flight when it is unfocused. Pump it with `tools/playtest/editorstate.cs`.
  - Long jobs are queued on an idle editor tick instead of run inside a call (calls retry on timeout).
- Aseprite 1.3.18 + aseprite MCP (live mode). Every sprite sheet passes through Aseprite (palette lock to Apollo,
  validate, save the `.aseprite` source in `ArtSource/`, export the PNG into `Assets`).
- Python generators (stdlib only, no PIL) in `ArtSource/gen/`, audio synthesis in `tools/audio/` (numpy/scipy, offline).

## Rebuilding everything
1. Art: `python ArtSource/gen/<generator>.py`, then open, validate, save and export through the Aseprite MCP.
   - Heroes: `heroes.py`.
   - Creatures: `hand_bestiary.py <id>` (`--list` for all), then `strip.py` to a PNG strip.
     - Each creature is hand-drawn as pixel-grid parts (`hand_b2.py` .. `hand_b10.py`), posed per frame by the plans
       in `hand_core.py`. Bosses (`hand_bosses_a.py`, `hand_bosses_b.py`) are drawn at full 64px resolution.
     - Preview any frames with `hand_preview.py out.png 6 id@0,2,8`.
     - The old parametric `bestiary.py` only supplies the shared palette legend now.
   - Biome tiles: `biomes.py`.
   - Items, NPCs, FX: `items.py`, `npcs.py`, `hero_fx.py`, `enemy_fx.py`.
2. Audio:
   - `python tools/audio/sfx.py Oathdeep/Assets/_Game/Audio/SFX`
   - `python tools/audio/music.py Oathdeep/Assets/_Game/Audio/Music`
   - `python tools/audio/biome_music.py Oathdeep/Assets/_Game/Audio/Music`
   - VO: `tools/audio/vo.py`.
3. Unity: `bash tools/setup-wait.sh` queues **Oathdeep > Build Everything**. It runs these stages in order: importers and
   slicing, content data, audio library and story, about 97 prefabs, floors and the scene.
4. Tests: EditMode, 2142 passing (`node tools/unity-mcp.mjs tests-run '{"testMode":"EditMode"}'`; save the scene first).
5. QA: `bash tools/smoke.sh` in play mode runs every floor (teleport to the boss, clear it, take the portal).
6. APK: `node tools/run-cs.mjs tools/playtest/apk.cs`, then pump the editor; output goes to `Oathdeep/Builds/Android/Undersun.apk`.

## Architecture map (`Oathdeep/Assets/_Game/Scripts`)
- **Core**
  - `GameRunner` (+ `.Floors`, `.Screens`): app flow, run lifecycle, floor descent, pause, inventory, encounters.
  - `GameEvents` bus (cleared every run), `SaveSystem` (JSON; codex, items, quests, meta bonuses).
  - `QuestCatalog` / `QuestTracker`, `StoryAssets`.
- **Player**
  - `PlayerController` (+ `.Skills`): state machine driven by `HeroMoves` per `HeroId`.
  - `PlayerInventory`: bag, specials, run bonuses. `SoulWisps`.
- **Loot**
  - `ItemCatalog` (bases, legendaries, affixes), `ItemGenerator` (seeded rolls), `ItemInstance`, `Loadout`, `LootDropper`.
- **Dungeon**
  - Layout generator (room types), geometry (room shapes), builder, decorator (biome decals, encounter rooms).
  - `RoomController` (waves, elites, ambush, boss), `RoomEncounter`, `Interactable`, `EncounterHost` (bridge to UI).
- **Enemies**
  - Skeleton/Melee, Archer/Shooter/Turret, Slime/Hopper, `ChargerAI`, `CasterAI`, `BomberAI`, `BoneWardenAI`.
  - `PatternBossAI` + `BossPatterns` (data), `GroundBlast`.
- **Data**
  - `HeroData`, `EnemyData`, `Bestiary` (40 entries with lore and base stats), `StatBlock`.
- **UI**
  - Code-built UGUI: front end, HUD (context button, bag), pages (Heroes, Items, Codex, Quests, Inventory, Shop,
    Settings, About), encounter dialog, cinematic, onboarding.
- **Editor**: `OathdeepSetup.*` partials build all content from code and data.

## Known limits / next ideas
- Boss fights (`Enemies/Bosses/`): `BossCore` gives every Chorister three phases (at 2/3 and 1/3 health), a break
  gauge that staggers it, and damage multipliers; each boss adds its signature `BossMechanic` components (totems,
  spores, tides, overheat, frost, roots, clock hands, sandstorm, mirror decoys; the Cantor stacks them by phase).
  Hazards are `HazardZone`s the hero's movement samples (slow, ice, burn, root). Arena pillars differ per floor.
  Play-test a boss with `bash tools/bossrun.sh <floor> [hpFraction] [pumps]`.
- Rooms: combat rooms roll a `RoomChallenge` (waves, survive, nests, gauntlet), biomes lay terrain, elites carry a
  trait (swift, armored, giant, regenerating), and harder rooms end in a pick of gold, mend or gear.
- Talents (`Player/PlayerTalents`): six run-long fighting changes offered at sunstones (one per offer).
- Atmosphere: `AmbientMotes` per floor, glowing enemy shots, roll afterimages, boss-kill slow motion.
- `python tools/csc-check.py` compile-checks all C# with Unity's Roslyn, without the editor running.
- Difficulty: enemy health/attack follow `FloorConfig.StatScaleFor` (x1.0 on B1 to x7.35 on B10, accelerating so it outgrows
  the player's compounding gear). Model it with a quick per-floor hits-to-kill / hits-to-die table before retuning.
- Screenshots in the editor use a landscape Game view, so the room camera clamp centres horizontally there.
  On a portrait phone it follows the hero normally.
- Ideas for later:
  - controller/keyboard UI navigation
  - localisation (EN only today)
  - Play Store listing assets
  - a real device performance pass on low-end phones
