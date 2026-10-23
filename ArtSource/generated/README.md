# UNDERSUN inventory and interface art

Generated on 23 October 2026 using the built-in image generator. The art direction is dark fantasy pixel art: cool silver, antique gold, mahogany leather, scarlet cloth and restrained teal/violet magic. The source PNGs retain their generated transparency.

- `items-source.png`: 36 inventory silhouettes, six columns and six rows. These cover the 104 procedural item identities.
- `interface-source.png`: 24 HUD/menu/hero-skill/control sprites, six columns and four rows.
- `atlas-manifest.json`: exact source extraction rectangles and production atlas order.

`ArtSource/gen/import_generated_icons.mjs` extracts each sprite, preserves its alpha, downsamples with nearest-neighbor sampling and packs transparent 64px cells. It also creates labeled review sheets in `docs/screenshots/`. Run it with Node and the `sharp` package; an absolute path to the installed `sharp` package can be passed as the first argument.

The production textures are `Oathdeep/Assets/_Game/Art/UI/item_icons.png` (8 columns) and `interface_icons.png` (6 columns). The three round control sprites are also exported as independent `joystick_base.png`, `joystick_knob.png` and `action_ring.png` textures, keeping their translucent rims isolated from neighboring icons. These replace the historical 16px item and interface drawings; use this importer when repacking the new art rather than the older `items.py` generator.

In Unity, **Oathdeep > Refresh Item and HUD Art** updates slicing, item lookup, UI skin, all five hero skill icons and scene pickup references. **Build Everything** also imports this art. Both atlases use 64 pixels per world unit, preserving the original pickup footprint, with point filtering and no mipmaps or compression.
