"""Menu icons 16x16: gear, info, share, star, codex, quest, heroes, back, close, bag.
Prints JSON {legend, frames:[[name, rows]]}."""
import json
import math
import sys

sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
from compose import Canvas

L = {"k": "#10141f", "W": "#c7cfcc", "H": "#ebede9", "s": "#819796", "m": "#394a50", "G": "#de9e41",
     "y": "#e8c170", "o": "#be772b", "n": "#7a4841", "N": "#ad7757", "c": "#c09473", "R": "#a53030",
     "r": "#752438", "B": "#4f8fba", "b": "#3c5e8b"}


def disc(c, cx, cy, r, ch):
    for y in range(16):
        for x in range(16):
            if (x - cx) ** 2 + (y - cy) ** 2 <= r * r:
                c.g[y][x] = ch


def gear():
    c = Canvas(16, 16)
    for y in range(16):
        for x in range(16):
            d = math.hypot(x - 7.5, y - 7.5)
            a = math.atan2(y - 7.5, x - 7.5)
            tooth = math.cos(a * 8) > 0.2
            if d <= 5.2 or (tooth and d <= 7.0):
                c.g[y][x] = "W" if (x + y) < 15 else "s"
    disc(c, 7.5, 7.5, 2.2, ".")
    return c.outline("k").rows()


def info():
    c = Canvas(16, 16)
    disc(c, 7.5, 7.5, 6.8, "b")
    disc(c, 6.8, 6.8, 5.5, "B")
    for y in range(6, 12): c.g[y][7] = "H"; c.g[y][8] = "W"
    c.g[4][7] = "H"; c.g[4][8] = "H"
    return c.outline("k").rows()


def share():
    c = Canvas(16, 16)
    c.line(4, 8, 11, 3, "s"); c.line(4, 8, 11, 12, "s")
    for cx, cy in ((4, 8), (11, 3), (11, 12)):
        disc(c, cx, cy, 2.3, "G")
        c.g[cy - 1][cx - 1] = "y"
    return c.outline("k").rows()


def star():
    c = Canvas(16, 16)
    pts = []
    for i in range(10):
        r = 7.2 if i % 2 == 0 else 3.0
        a = -math.pi / 2 + i * math.pi / 5
        pts.append((7.5 + r * math.cos(a), 8 + r * math.sin(a)))
    for y in range(16):
        for x in range(16):
            px, py, inside, j = x + .5, y + .5, False, len(pts) - 1
            for i in range(len(pts)):
                (xi, yi), (xj, yj) = pts[i], pts[j]
                if (yi > py) != (yj > py) and px < (xj - xi) * (py - yi) / (yj - yi) + xi:
                    inside = not inside
                j = i
            if inside:
                c.g[y][x] = "y" if y < 8 and x < 9 else "G" if y < 11 else "o"
    return c.outline("k").rows()


CODEX = [
    "................",
    ".kkkkkkkkkkkkk..",
    "kRRRRRRRRRRRRRk.",
    "kRrkkkkkkkkkkRk.",
    "kRrkWWWWWWWWkRk.",
    "kRrkWGGGGGGWkRk.",
    "kRrkWGWWWWGWkRk.",
    "kRrkWGWkkWGWkRk.",
    "kRrkWGWWWWGWkRk.",
    "kRrkWGGGGGGWkRk.",
    "kRrkWWWWWWWWkRk.",
    "kRrkkkkkkkkkkRk.",
    "kRRRRRRRRRRRRRk.",
    "krrrrrrrrrrrrrk.",
    ".kkkkkkkkkkkkk..",
    "................",
]

QUEST = [
    "................",
    "..kkkkkkkkkkk...",
    ".kNccccccccccNk.",
    ".kNkkkkkkkkkkNk.",
    "..kccccccccck...",
    "..kcnnnnnnnck...",
    "..kccccccccck...",
    "..kcnnnnnnck....",
    "..kccccccccck...",
    "..kcnnnnnnnck...",
    "..kcccccRRcck...",
    "..kccccRrrRck...",
    ".kNkkkkkRRkkNk..",
    ".kNccccccccccNk.",
    "..kkkkkkkkkkkk..",
    "................",
]

HEROES = [
    "................",
    "......kkk.......",
    ".....kRRRk......",
    "....kRrkkkk.....",
    "...kkWWWWWWk....",
    "..kWWHHWWWWsk...",
    "..kWHWWWWWWsk...",
    "..kWWkkkkkksk...",
    "..kWkBBBBBBkk...",
    "..kWWkkkkkksk...",
    "..kWWWWkWWWsk...",
    "..kWWWkWkWWsk...",
    "...kWWWWWWsk....",
    "....kkkkkkk.....",
    "................",
    "................",
]

BACK = [
    "................",
    "................",
    "......kk........",
    ".....kHk........",
    "....kHWk........",
    "...kHWWkkkkkkk..",
    "..kHWWWWWWWWWWk.",
    ".kHWWWWWWWWWWWk.",
    "..ksWWWWWWWWWWk.",
    "...ksWWksssssk..",
    "....ksWk........",
    ".....ksk........",
    "......kk........",
    "................",
    "................",
    "................",
]


def close():
    c = Canvas(16, 16)
    for i in range(3, 13):
        for d in (0, 1):
            c.g[i][i - 1 + d] = "H" if d == 0 else "W"
            c.g[i][16 - i - d] = "H" if d == 0 else "W"
    return c.outline("k").rows()


BAG = [
    "................",
    "......kkkk......",
    ".....kNnnNk.....",
    "......kGGk......",
    ".....kkkkkk.....",
    "....kNccccNk....",
    "...kNccccccNk...",
    "..kNcccGGccccNk.",
    "..kNccGyyGcccNk.",
    "..kNcccGGccccNk.",
    "..kNccccccccnNk.",
    "..kNcccccccnnNk.",
    "...kNccccnnnNk..",
    "....kkkkkkkkk...",
    "................",
    "................",
]

frames = [("gear", gear()), ("info", info()), ("share", share()), ("star", star()), ("codex", CODEX),
          ("quest", QUEST), ("heroes", HEROES), ("back", BACK), ("close", close()), ("bag", BAG)]

if __name__ == "__main__":
    for name, rows in frames:
        assert len(rows) == 16 and all(len(r) == 16 for r in rows), (name, [len(r) for r in rows])
    print(json.dumps({"legend": L, "frames": frames}, separators=(",", ":")))
