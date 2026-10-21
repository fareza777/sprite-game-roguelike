"""UNDERSUN launcher icon, 64x64: a cross-section of the world — dusk sky, a lone lantern-bearer on the surface,
dark strata, and the buried sun glowing up through cracks. Prints JSON {legend, frames:[["icon", rows]]}."""
import json
import math
import random

N = 64
L = {"k": "#090a14", "K": "#10141f", "a": "#151d28", "A": "#202e37", "m": "#394a50", "M": "#577277",
     "v": "#1e1d39", "b": "#253a5e", "B": "#3c5e8b", "s": "#c7cfcc", "w": "#ebede9",
     "o": "#be772b", "G": "#de9e41", "y": "#e8c170", "e": "#da863e", "E": "#cf573c", "r": "#a53030",
     "q": "#4d2b32", "Q": "#341c27"}
rnd = random.Random(11)
g = [["k"] * N for _ in range(N)]


def put(x, y, ch):
    if 0 <= x < N and 0 <= y < N:
        g[y][x] = ch


SURFACE = 19
# sky: banded dusk gradient with a few stars
for y in range(SURFACE):
    band = "v" if y < 6 else "b" if y < 12 else "B" if y < 16 else "e"
    for x in range(N):
        put(x, y, band)
for x in range(N):  # warm horizon haze just above the ground
    put(x, SURFACE - 1, "E" if abs(x - 32) < 10 else "q")
    put(x, SURFACE - 2, "e" if abs(x - 32) < 6 else "B")
for x, y in ((6, 3), (14, 7), (49, 2), (57, 9), (23, 4), (41, 6), (60, 4)):
    put(x, y, "s")
put(49, 2, "w")

# ruined towers of Emberwick on the horizon
for x0, w, h in ((3, 5, 9), (9, 3, 6), (52, 4, 11), (57, 5, 7)):
    for x in range(x0, x0 + w):
        for y in range(SURFACE - h, SURFACE):
            put(x, y, "a")
    put(x0 + w // 2, SURFACE - h + 2, "G")  # one lit window

# ground surface + strata
surface_y = [SURFACE + (1 if (x * 7) % 11 == 0 else 0) for x in range(N)]
for x in range(N):
    for y in range(surface_y[x], N):
        d = y - surface_y[x]
        put(x, y, "M" if d == 0 else "m" if d == 1 else "A" if d < 9 else "a" if d < 18 else "K")
    # wavy strata lines
for band_y, ch in ((SURFACE + 9, "m"), (SURFACE + 18, "A"), (SURFACE + 27, "a")):
    for x in range(N):
        put(x, band_y + int(1.5 * math.sin(x * 0.35 + band_y)), ch)

# the buried sun: glow halo, rim, body, hot core
SX, SY, R = 32, 70, 25
for y in range(SURFACE + 2, N):
    for x in range(N):
        d = math.hypot(x - SX, y - SY)
        if d <= R:
            t = d / R
            ch = "w" if t < 0.45 else "y" if t < 0.72 else "G" if t < 0.9 else "o"
            put(x, y, ch)
        elif d <= R + 2.5:
            put(x, y, "e" if (x + y) % 2 == 0 else "E")
        elif d <= R + 5 and (x + y) % 2 == 0:
            put(x, y, "q")

# light cracks fanning up from the sun to the surface (the light reaching the world)
for ang in (-62, -38, -14, 12, 36, 60):
    a = math.radians(ang - 90)
    x, y = SX + math.cos(a) * (R + 1), SY + math.sin(a) * (R + 1)
    while y > SURFACE + 1:
        x += math.cos(a) * 0.7 + rnd.uniform(-0.55, 0.55)
        y += math.sin(a) * 0.7
        put(int(x), int(y), "G")
        put(int(x) + 1, int(y), "o")
        if rnd.random() < 0.25: put(int(x) - 1, int(y), "e")
    put(int(x), SURFACE, "y")

# light shaft breaking the surface under the hero
for y in range(SURFACE - 6, SURFACE + 2):
    for x in range(30, 35):
        if g[y][x] not in "ky":
            put(x, y, "y" if abs(x - 32) <= 0 else "G" if abs(x - 32) == 1 else g[y][x])

# the lone Unsung: cloak, hood, sword, lantern held high
hero = [
    "..kk...",
    ".kkkk..",
    ".kkkk.y",
    "..kk.kG",
    ".kkkkk.",
    "kkkkkk.",
    "kkkkk..",
    ".kkkk..",
    ".kkkk..",
    ".k..k..",
    ".k..k..",
]
hx, hy = 29, SURFACE - len(hero)
for j, row in enumerate(hero):
    for i, ch in enumerate(row):
        if ch != ".":
            put(hx + i, hy + j, ch)

# 1px dark frame softens the edge on round launcher masks
for i in range(N):
    put(i, 0, "k"); put(i, N - 1, "k"); put(0, i, "k"); put(N - 1, i, "k")

if __name__ == "__main__":
    print(json.dumps({"legend": L, "frames": [["icon", ["".join(r) for r in g]]]}, separators=(",", ":")))
