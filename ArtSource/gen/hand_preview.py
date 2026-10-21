"""Preview every frame of hand-drawn creatures as one PNG (a row per creature).
usage: python hand_preview.py out.png [scale] id[@f,f,...] [id...]   (@ picks frame indices)"""
import sys

sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
import pngio  # noqa: E402
from bestiary import L  # noqa: E402
from hand_bestiary import CREATURES  # noqa: E402

out, scale, ids = sys.argv[1], int(sys.argv[2]), sys.argv[3:]
def pick(arg):
    name, _, sel = arg.partition("@")
    frames = CREATURES[name].frames()
    if sel:
        frames = [frames[int(k)] for k in sel.split(",")]
    return frames, CREATURES[name].size


strips = [pick(i) for i in ids]
cell = max(size for _, size in strips)
cols = max(len(f) for f, _ in strips)
W, H = cols * cell * scale, len(strips) * cell * scale
bg = (60, 62, 72, 255)
px = [[bg if ((x // (cell * scale)) % 2 == 0) else (54, 56, 66, 255) for x in range(W)] for _ in range(H)]
rgb = {k: tuple(int(v[i:i + 2], 16) for i in (1, 3, 5)) + (255,) for k, v in L.items()}
for r, (frames, size) in enumerate(strips):
    for n, (_, rows) in enumerate(frames):
        ox, oy = n * cell * scale, r * cell * scale + (cell - size) * scale
        for y, row in enumerate(rows):
            for x, ch in enumerate(row):
                if ch == ".":
                    continue
                for b in range(scale):
                    line = px[oy + y * scale + b]
                    for a in range(scale):
                        line[ox + x * scale + a] = rgb[ch]
pngio.write(out, W, H, px)
