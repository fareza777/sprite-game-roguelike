"""Hero projectile + skill VFX sheet 128x48 (Apollo palette).
Row 0 (16x16): bolt, flare, orb0, orb1, wisp0, wisp1, shadow0, shadow1, shadow2.
Row 1 (32x32 at y=16): ring0..ring3 - Brakka's Toll shockwave.
Row 2 (16x16 at y=48): HUD skill icons - iconFlare, iconRequiem, iconToll, iconShadowstep.
Prints {legend, frames:[["sheet", rows]]} for strip.py."""
import json
import math

W, H = 128, 64
L = {"H": "#ebede9", "C": "#a4dddb", "T": "#73bed3", "B": "#4f8fba", "Y": "#e8c170", "G": "#de9e41",
     "o": "#be772b", "r": "#a53030", "R": "#cf573c", "W": "#819796", "w": "#577277", "x": "#090a14",
     "n": "#1e1d39", "p": "#402751", "P": "#7a367b", "l": "#884b2b", "s": "#c7cfcc"}
g = [["."] * W for _ in range(H)]


def put(x, y, ch):
    if 0 <= x < W and 0 <= y < H:
        g[int(y)][int(x)] = ch


def disc(cx, cy, r, ch, skip_odd=False):
    for y in range(int(cy - r) - 1, int(cy + r) + 2):
        for x in range(int(cx - r) - 1, int(cx + r) + 2):
            if (x - cx) ** 2 + (y - cy) ** 2 <= r * r and not (skip_odd and (x + y) % 2):
                put(x, y, ch)


# bolt (0,0): iron head, wooden shaft, white fletching - points right
for x in range(3, 12):
    put(x, 8, "l")
for x in range(3, 12):
    put(x, 7, "x") if x > 3 else None
put(12, 8, "s"); put(13, 8, "H"); put(12, 7, "W"); put(12, 9, "W"); put(14, 8, "x")
for x in (2, 3, 4):
    put(x, 7, "H"); put(x, 9, "H")
for x in range(3, 12):
    put(x, 9, "x")

# flare (16,0): burning bolt with a comet tail
ox = 16
for i, ch in enumerate("ooGGYYH"):
    put(ox + 3 + i, 8, ch)
for i, ch in enumerate("oGY"):
    put(ox + 6 + i, 7, ch); put(ox + 6 + i, 9, ch)
disc(ox + 11, 8, 2.2, "Y"); disc(ox + 11, 8, 1.2, "H"); put(ox + 13, 8, "H")
put(ox + 2, 7, "o"); put(ox + 1, 9, "o")

# soul orbs (32,0) (48,0): cyan core with a violet halo, two pulse frames
for k, r in enumerate((4.2, 4.8)):
    cx, cy = 32 + 16 * k + 8, 8
    disc(cx, cy, r, "P", skip_odd=True)
    disc(cx, cy, r - 1.2, "B")
    disc(cx, cy, r - 2.2, "T")
    disc(cx - 0.6, cy - 0.6, 1.2, "C")
    put(cx - 1, cy - 1, "H")

# wisps (64,0) (80,0): pale flame with a trailing tail
for k in range(2):
    ox = 64 + 16 * k
    sway = 1 if k else -1
    for y in range(4, 13):
        half = max(0.0, 3.2 * math.sin(math.pi * (y - 3) / 10))
        for x in range(int(8 - half), int(8 + half) + 1):
            put(ox + x + (sway if y < 6 else 0), y, "T")
    disc(ox + 8, 9, 2.4, "C"); disc(ox + 8, 9, 1.2, "H")
    put(ox + 7, 9, "n"); put(ox + 9, 9, "n")   # two hollow eyes - the dead singing
    put(ox + 8 + sway, 3, "C")

# shadow puffs (96..128, 0): Sable's step - violet smoke expanding and thinning
for k in range(3):
    ox = 96 + 16 * k
    if ox + 16 > W:
        break
    r = 3 + 2 * k
    disc(ox + 8, 9, r, "n", skip_odd=k == 2)
    disc(ox + 8, 9, r - 1.5, "p", skip_odd=k >= 1)
    if k == 0:
        disc(ox + 8, 9, 1.5, "P")

# Toll rings (row 1): an expanding brass shockwave
for k in range(4):
    ox, oy = 32 * k, 16
    rr = 5 + 3.4 * k
    thick = 3.2 - 0.7 * k
    for y in range(32):
        for x in range(32):
            d = math.hypot(x - 15.5, (y - 15.5) * 1.6)   # squashed: a ring on the floor
            if rr - thick <= d <= rr:
                band = (rr - d) / thick
                ch = "H" if band < 0.3 else ("Y" if band < 0.65 else "G")
                if k == 3 and (x + y) % 2:
                    continue
                put(ox + x, oy + y, ch)
    if k == 0:
        disc(ox + 15.5, oy + 15.5, 2.5, "H")

# ---- HUD skill icons (row y=48), read at 24-32px on the action button ----
def line(x0, y0, x1, y1, ch):
    n = max(abs(x1 - x0), abs(y1 - y0), 1)
    for i in range(n + 1):
        put(round(x0 + (x1 - x0) * i / n), round(y0 + (y1 - y0) * i / n), ch)

oy = 48
# Flare Volley: three burning bolts fanning out from a crossbow nock
for dy, ch in ((-5, "G"), (0, "Y"), (5, "G")):
    line(2, oy + 8, 12, oy + 8 + dy, "o")
    disc(12.5, oy + 8 + dy, 1.6, ch); put(13, oy + 8 + dy, "H")
disc(2.5, oy + 8, 1.6, "l")

# Requiem: a singing wisp with a music-note tail
ox = 16
for y in range(2, 14):
    half = max(0.0, 4.2 * math.sin(math.pi * (y - 1) / 13))
    for x in range(int(8 - half), int(8 + half) + 1):
        put(ox + x, oy + y, "T")
disc(ox + 8, oy + 9, 3.2, "C"); disc(ox + 8, oy + 9, 1.6, "H")
put(ox + 6, oy + 9, "n"); put(ox + 10, oy + 9, "n"); put(ox + 8, oy + 11, "n")
put(ox + 8, oy + 1, "C")

# Toll: a bronze bell with a clapper and sound arcs
ox = 32
BELL = [1.5, 2.6, 3.1, 3.2, 3.2, 3.4, 3.8, 4.4, 5.2, 5.8]   # dome, waist, flared lip
for i, half in enumerate(BELL):
    for x in range(round(8 - half), round(8 + half) + 1):
        put(ox + x, oy + 3 + i, "Y" if x < 7 else ("G" if x < 10 else "o"))
for x in range(3, 14):
    put(ox + x, oy + 12, "l")
put(ox + 8, oy + 2, "o"); put(ox + 8, oy + 13, "Y"); put(ox + 7, oy + 5, "Y"); put(ox + 7, oy + 6, "Y")
for k in (0, 1):
    put(ox + 1 - k, oy + 7 + k, "H"); put(ox + 15 + k - 1, oy + 7 + k, "H")

# Shadowstep: a dagger cutting out of a violet smoke ring
ox = 48
disc(ox + 6, oy + 10, 4.6, "n"); disc(ox + 6, oy + 10, 3.2, "p", skip_odd=True)
line(ox + 5, oy + 11, ox + 13, oy + 3, "s"); line(ox + 6, oy + 11, ox + 13, oy + 4, "W")
put(ox + 14, oy + 2, "H")
line(ox + 3, oy + 10, ox + 6, oy + 13, "r")
put(ox + 3, oy + 13, "l"); put(ox + 2, oy + 14, "l")

# dark outline around the icons so they read on the button ring
icon_px = {(x, y) for y in range(48, 64) for x in range(64) if g[y][x] != "."}
for x, y in icon_px:
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        q = (x + dx, y + dy)
        if q not in icon_px and 48 <= q[1] < 64 and 0 <= q[0] < 64 and q[0] // 16 == x // 16:
            put(q[0], q[1], "x")

print(json.dumps({"legend": L, "frames": [["sheet", ["".join(r) for r in g]]]}))
