// Pack the approved, generated sprites without changing their artwork or alpha.
// Usage: node ArtSource/gen/import_generated_icons.mjs [absolute path to sharp]
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';
import { mkdir, readFile, writeFile } from 'node:fs/promises';

const require = createRequire(import.meta.url);
const sharp = require(process.argv[2] || 'sharp');
const root = join(dirname(fileURLToPath(import.meta.url)), '../..');
const sourceDir = join(root, 'ArtSource/generated');
const artDir = join(root, 'Oathdeep/Assets/_Game/Art/UI');
const previewDir = join(root, 'docs/screenshots');
const size = 64;
const items = [
  'sword', 'longsword', 'greatsword', 'axe', 'mace', 'flail', 'hammer', 'dagger',
  'spear', 'crossbow', 'staff', 'scythe', 'plate', 'mail', 'leather', 'robe',
  'cloak', 'helm', 'gauntlet', 'boots', 'ring', 'ringGem', 'amulet', 'talisman',
  'brooch', 'idol', 'tome', 'skull', 'orb', 'lantern', 'hourglass', 'feather',
  'bell', 'heart', 'crown', 'candle',
];
const ui = [
  'iconGold', 'iconPotion', 'iconAttack', 'iconDodge', 'iconSkill', 'iconPause',
  'gear', 'info', 'share', 'star', 'codex', 'quest',
  'heroes', 'back', 'close', 'bag', 'iconFlare', 'iconRequiem',
  'iconToll', 'iconShadowstep', 'joystickBase', 'joystickKnob', 'actionRing', 'iconHealth',
];
// The generated circular rims slightly overhang the source sheet's nominal last-row boundary.
const sourceRects = {
  joystickBase: { left: 525, top: 740, width: 240, height: 240 },
  actionRing: { left: 1030, top: 750, width: 240, height: 240 },
};

await mkdir(previewDir, { recursive: true });

async function pack(source, names, columns, rowEdges, destination) {
  const bytes = await readFile(join(sourceDir, source));
  const meta = await sharp(bytes).metadata();
  if (!meta.hasAlpha) throw new Error(`${source} has no transparency`);
  const sprites = [];
  const placements = [];
  for (let index = 0; index < names.length; index++) {
    const column = index % 6;
    const row = Math.floor(index / 6);
    const override = sourceRects[names[index]];
    const left = override?.left ?? Math.round(column * meta.width / 6) + (names[index] === 'candle' ? 28 : 0);
    const right = override ? left + override.width : Math.round((column + 1) * meta.width / 6);
    const top = override?.top ?? Math.round(rowEdges[row] * meta.height);
    const bottom = override ? top + override.height : Math.round(rowEdges[row + 1] * meta.height);
    const cell = sharp(await sharp(bytes).extract({ left, top, width: right - left, height: bottom - top }).png().toBuffer());
    const { data, info } = await cell.clone().ensureAlpha().raw().toBuffer({ resolveWithObject: true });
    let x0 = info.width, y0 = info.height, x1 = -1, y1 = -1;
    // Find the subject's bounds, excluding fragments of neighboring sprites in a source cell.
    // This only chooses the extraction rectangle; exported artwork and alpha remain untouched.
    const visited = new Uint8Array(info.width * info.height);
    const components = [];
    for (let start = 0; start < visited.length; start++) {
      if (visited[start] || data[start * 4 + 3] < 32) continue;
      const queue = [start];
      visited[start] = 1;
      const bounds = { count: 0, x0: info.width, y0: info.height, x1: -1, y1: -1 };
      for (let cursor = 0; cursor < queue.length; cursor++) {
        const pixel = queue[cursor], x = pixel % info.width, y = Math.floor(pixel / info.width);
        bounds.count++;
        bounds.x0 = Math.min(bounds.x0, x); bounds.y0 = Math.min(bounds.y0, y);
        bounds.x1 = Math.max(bounds.x1, x); bounds.y1 = Math.max(bounds.y1, y);
        for (let dy = -1; dy <= 1; dy++) for (let dx = -1; dx <= 1; dx++) {
          const nx = x + dx, ny = y + dy;
          if (nx < 0 || ny < 0 || nx >= info.width || ny >= info.height) continue;
          const next = ny * info.width + nx;
          if (visited[next] || data[next * 4 + 3] < 32) continue;
          visited[next] = 1; queue.push(next);
        }
      }
      components.push(bounds);
    }
    const largest = Math.max(...components.map(c => c.count));
    for (const c of components.filter(c => c.count >= largest * 0.04)) {
      x0 = Math.min(x0, c.x0); y0 = Math.min(y0, c.y0);
      x1 = Math.max(x1, c.x1); y1 = Math.max(y1, c.y1);
    }
    if (x1 < x0) throw new Error(`Empty sprite: ${names[index]}`);
    x0 = Math.max(0, x0 - 2); y0 = Math.max(0, y0 - 2);
    x1 = Math.min(info.width - 1, x1 + 2); y1 = Math.min(info.height - 1, y1 + 2);
    const sprite = await cell.extract({ left: x0, top: y0, width: x1 - x0 + 1, height: y1 - y0 + 1 })
      .resize(size - 8, size - 8, { fit: 'inside', kernel: 'nearest' })
      .extend({ top: 4, bottom: 4, left: 4, right: 4, background: '#00000000' })
      .resize(size, size, { fit: 'contain', background: '#00000000', kernel: 'nearest' })
      .png().toBuffer();
    sprites.push(sprite);
    placements.push({ name: names[index], cell: index, source: { left: left + x0, top: top + y0, width: x1 - x0 + 1, height: y1 - y0 + 1 } });
  }
  const rows = Math.ceil(names.length / columns);
  const atlas = await sharp({ create: { width: columns * size, height: rows * size, channels: 4, background: '#00000000' } })
    .composite(sprites.map((input, i) => ({ input, left: i % columns * size, top: Math.floor(i / columns) * size })))
    .png().toBuffer();
  await writeFile(join(artDir, destination), atlas);
  if (source === 'interface-source.png') {
    // Independent control textures keep the large, translucent rims isolated from neighboring magic icons.
    for (const [name, file] of Object.entries({ joystickBase: 'joystick_base.png', joystickKnob: 'joystick_knob.png', actionRing: 'action_ring.png' }))
      await writeFile(join(artDir, file), sprites[names.indexOf(name)]);
  }

  const previews = await Promise.all(sprites.map(input => sharp(input).resize(96, 96, { kernel: 'nearest' }).toBuffer()));
  const width = columns * 128;
  const height = rows * 128;
  const captions = `<svg width="${width}" height="${height}">${names.map((name, i) =>
    `<text x="${i % columns * 128 + 64}" y="${Math.floor(i / columns) * 128 + 120}" text-anchor="middle" font-family="Consolas,monospace" font-size="12" fill="#ded4bb">${name}</text>`).join('')}</svg>`;
  await sharp({ create: { width, height, channels: 4, background: '#101923' } })
    .composite([...previews.map((input, i) => ({ input, left: i % columns * 128 + 16, top: Math.floor(i / columns) * 128 + 6 })), { input: Buffer.from(captions), left: 0, top: 0 }])
    .png().toFile(join(previewDir, `2026-10-23-${destination}`));
  return { source, destination, size, columns, placements };
}

const manifest = [];
manifest.push(await pack('items-source.png', items, 8, [0, 220, 436, 633, 825, 1026, 1254].map(v => v / 1254), 'item_icons.png'));
manifest.push(await pack('interface-source.png', ui, 6, [0, 0.25, 0.5, 0.75, 1], 'interface_icons.png'));
await writeFile(join(sourceDir, 'atlas-manifest.json'), JSON.stringify(manifest, null, 2) + '\n');
console.log(`Packed ${items.length} item sprites and ${ui.length} interface sprites at ${size}px; previews saved in docs/screenshots.`);
