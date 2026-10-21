# UNDERSUN

*The sun was buried. You are what it sings for.*

UNDERSUN is a pixel-art action roguelike for Android. It is played in portrait with one thumb.

Nine hundred years ago the Choir of Dawn sang the Sun down into the earth and sealed her beneath ten vaults. Each vault
holds one verse of the hymn and one Chorister to guard it. Now the Sun is singing back. You are one of the Unsung, and
you descend through ten biomes to silence every verse and raise her.

## Features
- **Five heroes**, each with their own stats, combo and skill:
  - Aldric the Oathknight: sword and shield, Shield Bash.
  - Wren the Lamplighter: crossbow, Flare Volley.
  - Morrow the Gravesinger: homing soul orbs, Requiem wisps.
  - Brakka the Bellbreaker: bell-hammer, Toll shockwave.
  - Sable the Duskblade: twin daggers, Shadowstep.
- **Ten floors and ten biomes**:
  - Sunken Crypt, Mycelial Hollow, Drowned Cistern, Ember Forge, Frostbone Caverns.
  - Thornwild Ruins, Clockwork Ossuary, Sandsunk Necropolis, Glass Abyss, Heart of Dusk.
  - Each floor has 16–20 large procedural rooms and a boss.
- **Forty creatures** with a Codex that opens as you defeat them, including ten Chorister bosses with multi-phase
  attack patterns.
- **Peaceful rooms**: Old Tallow's shop, sun shrines with run-long blessings, pilgrims with lore and gifts, campfires,
  and risky events (gambler's dice, sun shards, a weeping statue, a chained chest).
- **104 items**: 64 bases and 40 legendaries across Weapon, Armor, Charm and Relic slots. Stats are procedurally
  rolled, with 24 affixes and specials (lifesteal, thorns, skill haste, gold find). Includes an equipment screen and an
  item compendium.
- **Story**:
  - Narrated prologue and ending cinematics, plus a narrated introduction for every floor.
  - A main quest in twelve chapters.
  - Twelve side quests that grant permanent bonuses.
- **Original score and sound**: eleven synthesized music tracks and 34 sound effects, all made offline.
- **Mobile-first controls**:
  - Floating or fixed joystick, attack / dodge / skill buttons, a contextual TALK / TRADE / PRAY button.
  - Left-handed layout, auto-aim, haptics, settings, and a one-time tutorial.

## Building
- Unity **6000.3.20f1** with Android Build Support.
- Open `Oathdeep/`, then run **Oathdeep > Build Everything** to regenerate all data, prefabs and the scene.
- Build the APK from **Oathdeep > Build Android APK** (IL2CPP, ARM64, min SDK 25).

Art and audio are generated from source:
- Pixel art: Python generators in `ArtSource/gen/` → curated and exported with Aseprite (`ArtSource/*.aseprite`).
- Music, SFX and narration: `tools/audio/` (numpy / scipy synthesis, fully offline).

See `docs/PROGRESS.md` for the full pipeline and architecture.

## Credits
Created by fareza777. Pixel font based on a public-domain (CC0) 5×7 font. Palette: Apollo by AdamCYounis.
