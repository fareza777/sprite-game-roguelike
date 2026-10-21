"""Enemy FX sheet 192x48 (Apollo). Row 0, 16x16: fireball, iceShard, thorn, glassShard, waterOrb, sandBlade, duskOrb,
spore, curseOrb, gearShot (projectiles point right). Row 1 at y=16, 32x32: warn0, warn1 (ground warning ring), boom0..boom3.
Prints {legend, frames:[["sheet", rows]]} for strip.py."""
import json
import math

W, H = 192, 48
L = {"k": "#090a14", "H": "#ebede9", "d": "#e8c170", "c": "#de9e41", "b": "#be772b", "g": "#cf573c", "f": "#a53030",
     "n": "#a4dddb", "m": "#73bed3", "l": "#4f8fba", "j": "#3c5e8b", "u": "#75a743", "t": "#468232", "s": "#25562e",
     "x": "#d7b594", "8": "#c09473", "7": "#ad7757", "C": "#752438", "B": "#411d31", "q": "#a23e8c", "p": "#7a367b",
     "o": "#402751", "R": "#df84a5", "r": "#c65197", "v": "#a8ca58", "3": "#819796", "4": "#c7cfcc", "2": "#577277"}
g = [["."] * W for _ in range(H)]


def put(x, y, ch):
    x, y = int(round(x)), int(round(y))
    if 0 <= x < W and 0 <= y < H:
        g[y][x] = ch


def disc(cx, cy, r, ch, ry=None):
    ry = ry or r
    for y in range(int(cy - ry) - 1, int(cy + ry) + 2):
        for x in range(int(cx - r) - 1, int(cx + r) + 2):
            if ((x - cx) / r) ** 2 + ((y - cy) / ry) ** 2 <= 1:
                put(x, y, ch)


def line(x0, y0, x1, y1, ch):
    n = int(max(abs(x1 - x0), abs(y1 - y0), 1))
    for i in range(n + 1):
        put(x0 + (x1 - x0) * i / n, y0 + (y1 - y0) * i / n, ch)


def orb(ox, outer, mid, core, tail):
    for i, ch in enumerate(tail):
        put(ox + 2 + i, 8, ch); put(ox + 3 + i, 7 + (i % 2) * 2, ch)
    disc(ox + 10, 8, 4, outer); disc(ox + 10, 8, 2.8, mid); disc(ox + 9.5, 7.5, 1.4, core)


def shard(ox, dark, mid, light):
    pts = [(ox + 3, 8), (ox + 9, 5), (ox + 14, 8), (ox + 9, 11)]
    for y in range(4, 13):
        for x in range(ox + 2, ox + 15):
            # inside the diamond
            if abs(x - (ox + 8.5)) / 6 + abs(y - 8) / 3.2 <= 1:
                put(x, y, mid)
    line(ox + 3, 8, ox + 14, 8, light)
    line(ox + 3, 9, ox + 9, 11, dark)


orb(0, "f", "g", "d", "fgc")                     # fireball
shard(16, "j", "m", "H")                         # ice shard
line(34, 8, 46, 8, "t"); line(35, 7, 44, 7, "u")  # thorn
for x in (37, 40, 43):
    put(x, 6, "s"); put(x, 9, "s")
put(46, 8, "v"); put(47, 8, "H")
shard(48, "l", "n", "H")                         # glass shard
orb(64, "j", "l", "n", "jl")                     # water orb
for y in range(5, 12):                           # sand blade (crescent)
    for x in range(82, 94):
        d = math.hypot(x - 84, (y - 8) * 1.6)
        if 7 <= d <= 9.5 and x > 85:
            put(x, y, "c" if d < 8.5 else "x")
orb(96, "B", "C", "g", "BC")                     # dusk orb
for k in range(7):                               # spore cloud
    a = k * 2 * math.pi / 7
    disc(120 + math.cos(a) * 3, 8 + math.sin(a) * 3, 1.6, "r" if k % 2 else "v")
disc(120, 8, 1.5, "R")
orb(128, "o", "p", "q", "op")                    # curse orb
for k in range(8):                               # gear shot
    a = k * math.pi / 4
    put(152 + math.cos(a) * 5, 8 + math.sin(a) * 5, "c")
disc(152, 8, 3.6, "b"); disc(152, 8, 1.6, "k")

# ground warning ring (two pulse frames) and explosion
for k, (r, ch) in enumerate(((13, "g"), (14, "f"))):
    ox, oy = 32 * k, 16
    for y in range(32):
        for x in range(32):
            d = math.hypot(x - 15.5, (y - 15.5) * 1.5)
            if r - 1.6 <= d <= r:
                put(ox + x, oy + y, ch)
            elif d < r - 1.6 and (x + y + k) % 4 == 0:
                put(ox + x, oy + y, "f")
for k in range(4):
    ox, oy = 64 + 32 * k, 16
    r = 4 + k * 3.5
    for y in range(32):
        for x in range(32):
            d = math.hypot(x - 15.5, y - 15.5)
            if d <= r:
                if k == 3 and (x + y) % 2:
                    continue
                ch = "H" if d < r * 0.35 and k < 2 else ("d" if d < r * 0.6 else ("c" if d < r * 0.85 else "g"))
                put(ox + x, oy + y, ch)

print(json.dumps({"legend": L, "frames": [["sheet", ["".join(r) for r in g]]]}))
