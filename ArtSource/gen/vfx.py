"""Combat VFX sheet 192x48: slash x3 + bash x3 (32x32, facing right), telegraph, ground shadow, pickup glow (16x16).
Prints JSON {legend, frames:[["sheet", rows]]}."""
import json
import math

W, H = 192, 48
L = {"H": "#ebede9", "C": "#a4dddb", "T": "#73bed3", "B": "#4f8fba", "Y": "#e8c170", "G": "#de9e41",
     "o": "#be772b", "W": "#819796", "x": "#090a14"}
g = [["."] * W for _ in range(H)]


def put(x, y, ch):
    if 0 <= x < W and 0 <= y < H:
        g[y][x] = ch


def arc(ox, oy, cx, cy, r_out, a0, a1, width, ramp, dither=False):
    """Crescent in the 32x32 cell at (ox,oy): width(t) px thick at sweep position t (0 at a0 .. 1 at a1)."""
    for y in range(32):
        for x in range(32):
            a = math.atan2(y - cy, x - cx)
            if not a0 <= a <= a1:
                continue
            t = (a - a0) / (a1 - a0)
            d = math.hypot(x - cx, y - cy)
            depth = r_out - d
            w = width(t)
            if 0 <= depth < w:
                if dither and (x + y) % 2:
                    continue
                band = min(int(depth / max(w, 1) * len(ramp)), len(ramp) - 1)
                put(ox + x, oy + y, ramp[band])


# slash: leading half, full bright crescent, thin fading trail
arc(0, 0, 4, 16, 14, -1.3, 0.25, lambda t: 1 + 3 * t, "HCT")
arc(32, 0, 4, 16, 14, -1.3, 1.3, lambda t: 1 + 4.5 * math.sin(math.pi * t), "HHCT")
arc(64, 0, 4, 16, 14, -1.3, 1.3, lambda t: 1 + 1.5 * math.sin(math.pi * t), "CT", dither=True)

# bash: shockwave rings pushing forward + impact star on the first frame
arc(96, 0, 6, 16, 9, -1.1, 1.1, lambda t: 1 + 2.5 * math.sin(math.pi * t), "YGo")
for dx, dy in ((0, -3), (0, 3), (-3, 0), (3, 0), (0, 0), (2, 2), (-2, -2), (2, -2), (-2, 2)):
    put(96 + 17 + dx, 16 + dy, "H" if abs(dx) + abs(dy) < 3 else "Y")
arc(128, 0, 6, 16, 13, -1.1, 1.1, lambda t: 1 + 3 * math.sin(math.pi * t), "HYG")
arc(160, 0, 6, 16, 15, -1.1, 1.1, lambda t: 1 + 1.5 * math.sin(math.pi * t), "Go", dither=True)

# telegraph: rounded rect, bright rim, softer fill (tinted at runtime)
for y in range(16):
    for x in range(16):
        cx, cy = min(max(x, 4), 11), min(max(y, 4), 11)
        d = math.hypot(x - cx, y - cy)
        if d <= 4.2:
            rim = x in (0, 15) or y in (0, 15) or d > 3.3
            put(x, 32 + y, "H" if rim else "W")

# ground shadow
for y in range(16):
    for x in range(16):
        if ((x - 7.5) / 7.2) ** 2 + ((y - 8) / 3.2) ** 2 <= 1:
            put(16 + x, 32 + y, "x")

# pickup glow: dithered halo ring + sparkle cross
for y in range(16):
    for x in range(16):
        d = math.hypot(x - 7.5, y - 7.5)
        if 5.5 <= d <= 7.5 and (x + y) % 2 == 0:
            put(32 + x, 32 + y, "H")
for x, y in ((7, 2), (8, 2), (2, 7), (13, 8), (7, 13)):
    put(32 + x, 32 + y, "H")

if __name__ == "__main__":
    print(json.dumps({"legend": L, "frames": [["sheet", ["".join(r) for r in g]]]}, separators=(",", ":")))
