"""Main-menu backdrop, 180x320 portrait: dusk sky with the empty ring where the sun was, the city of Emberwick on
two cliffs, and a jagged chasm falling away to the buried sun's glow far below.
Prints JSON {legend, frames:[["bg", rows]]}."""
import json
import math
import random

W, H = 180, 320
rnd = random.Random(4)
L = {"0": "#090a14", "1": "#10141f", "2": "#151d28", "3": "#202e37", "4": "#394a50", "5": "#577277",
     "v": "#1e1d39", "b": "#253a5e", "B": "#3c5e8b", "s": "#c7cfcc", "w": "#ebede9", "q": "#341c27",
     "Q": "#4d2b32", "r": "#752438", "E": "#cf573c", "e": "#da863e", "o": "#be772b", "G": "#de9e41",
     "y": "#e8c170", "p": "#402751"}
g = [["0"] * W for _ in range(H)]


def put(x, y, ch):
    if 0 <= x < W and 0 <= y < H:
        g[y][x] = ch


def bayer(x, y):
    return [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]][y % 4][x % 4] / 16


def gradient(y0, y1, colors):
    """Ordered-dither vertical gradient through `colors` between rows y0..y1."""
    n = len(colors) - 1
    for y in range(y0, y1):
        t = (y - y0) / max(1, y1 - y0 - 1) * n
        i = min(int(t), n - 1)
        f = t - i
        for x in range(W):
            put(x, y, colors[i + 1] if bayer(x, y) < f else colors[i])


# --- sky ---
HORIZON = 118
gradient(0, HORIZON, ["0", "v", "p", "b", "B", "Q"])
for _ in range(80):
    x, y = rnd.randrange(W), rnd.randrange(80)
    put(x, y, "w" if rnd.random() < 0.15 else "s" if rnd.random() < 0.4 else "5")
# the empty ring where the sun used to hang (faint, broken)
for a in range(0, 360, 2):
    if 200 < a < 230: continue
    x, y = 90 + math.cos(math.radians(a)) * 27, 46 + math.sin(math.radians(a)) * 27
    put(int(round(x)), int(round(y)), "5" if a % 10 else "4")

# --- chasm edges: jagged, slightly converging ---
noise = [rnd.uniform(-1, 1) for _ in range(H + 8)]
def smooth(i): return sum(noise[i:i + 6]) / 6
def edge_l(y): return 58 + (y - HORIZON) * 0.06 + smooth(y) * 9 + (6 if y % 37 < 4 else 0)
def edge_r(y): return 122 - (y - HORIZON) * 0.06 + smooth(y + 3) * 9 - (6 if (y + 17) % 41 < 4 else 0)

# --- buried sun glow at the bottom of the chasm (drawn first, walls cover it) ---
for y in range(HORIZON, H):
    for x in range(W):
        d = math.hypot((x - 90) * 1.15, y - 410)
        rings = [(96, "w"), (110, "y"), (124, "G"), (140, "e"), (158, "E"), (178, "r"), (200, "Q"), (226, "q"), (999, "0")]
        for k, (lim, ch) in enumerate(rings):
            if d < lim:
                nxt = rings[k + 1][1] if k + 1 < len(rings) else ch
                put(x, y, nxt if (lim - d) < 5 and bayer(x, y) < (5 - (lim - d)) / 5 else ch)
                break
# soft god rays rising through the chasm
for ang in (-14, -5, 4, 13):
    a = math.radians(ang - 90)
    for t in range(80, 330):
        x, y = 90 + math.cos(a) * t, 410 + math.sin(a) * t
        if y < HORIZON + 4 or y >= H: continue
        if g[int(y)][int(x)] in "0qQr" and bayer(int(x), int(y)) < 0.35:
            put(int(x), int(y), "Q" if g[int(y)][int(x)] in "0q" else "r")

# --- cliff walls with rock texture and warm rim light near the glow ---
for y in range(HORIZON - 2, H):
    l, r = edge_l(y), edge_r(y)
    warm = max(0.0, (y - 200) / 120)
    for x in range(W):
        inside_l, inside_r = x < l, x > r
        if not (inside_l or inside_r): continue
        depth = (l - x) if inside_l else (x - r)
        tex = (int(x * 0.7 + y * 0.31) * 7 + int(y * 0.53)) % 11
        ch = "2" if depth > 18 else "3"
        if depth > 34: ch = "1"
        if tex == 0 and depth > 6 and (x // 2) % 2 == 0: ch = "1"
        if tex == 5 and 3 < depth < 22 and (y // 3) % 2 == 0: ch = "3"
        if depth < 1.6: ch = "o" if warm > 0.55 else "E" if warm > 0.3 else "5"
        elif depth < 3.2 and warm > 0.25: ch = "Q" if warm < 0.6 else "r"
        put(x, y, ch)
# ledges and the carved stairway on the left wall
for k in range(6):
    y0 = 150 + k * 27
    for i in range(14):
        y = y0 + i
        x = int(edge_l(y) - 3 - ((i if k % 2 == 0 else 13 - i) * 1.1))
        put(x, y, "4"); put(x + 1, y, "3")
# a broken rope bridge
y_b = 168
for x in range(int(edge_l(y_b)), int(edge_r(y_b)) + 1):
    if 88 <= x <= 94: continue  # snapped in the middle
    sag = int(4 * math.sin(math.pi * (x - edge_l(y_b)) / (edge_r(y_b) - edge_l(y_b))))
    put(x, y_b + sag, "3")
    if x % 5 == 0:
        put(x, y_b + sag - 1, "2"); put(x, y_b + sag - 2, "2")
for i in range(9):
    put(87, y_b + 4 + i, "3")  # dangling end
# hanging lanterns down the chasm
for x0, y0 in ((70, 132), (108, 140), (76, 205), (104, 230)):
    for i in range(6): put(x0, y0 + i, "2")
    put(x0, y0 + 6, "G"); put(x0 - 1, y0 + 6, "o"); put(x0 + 1, y0 + 6, "o"); put(x0, y0 + 7, "y")

# --- Emberwick on the two cliff tops ---
def tower(x0, w, top, base, spire):
    for x in range(x0, x0 + w):
        for y in range(top, base):
            put(x, y, "1")
    if spire:
        for i in range(w // 2 + 1):
            for x in range(x0 + i, x0 + w - i):
                put(x, top - i - 1, "1")
    for _ in range(max(1, w * (base - top) // 30)):
        put(rnd.randrange(x0 + 1, x0 + w - 1), rnd.randrange(top + 2, base - 2), "y" if rnd.random() < 0.6 else "G")

for x0, w, top, sp in ((0, 8, 94, True), (9, 6, 104, False), (16, 11, 86, True), (28, 7, 101, False), (36, 9, 96, True), (46, 6, 106, False)):
    tower(x0, w, top, HORIZON + 2, sp)
for x0, w, top, sp in ((128, 6, 105, False), (135, 9, 95, True), (145, 11, 83, True), (157, 7, 100, False), (165, 10, 90, True), (176, 4, 102, False)):
    tower(x0, w, top, HORIZON + 2, sp)
# warm haze line along the cliff tops
for x in range(W):
    if g[HORIZON + 2][x] in "123": put(x, HORIZON + 2, "Q")

if __name__ == "__main__":
    print(json.dumps({"legend": L, "frames": [["bg", ["".join(r) for r in g]]]}, separators=(",", ":")))
