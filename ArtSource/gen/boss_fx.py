"""Boss arena art (16 px per world unit): breakable anchors, ground hazards and effects for the boss mechanics.
Writes boss_fx.png (256x160 sheet) and sand_fog.png (640x640 veil). usage: python boss_fx.py out_dir"""
import math
import os
import sys

sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
import pngio  # noqa: E402
from bestiary import L  # noqa: E402
from compose import Canvas  # noqa: E402
from rig import ellipse, poly, put, rect, shade, thick  # noqa: E402

RAMP = {"bone": ("7", "8", "x", "y"), "bark": ("F", "5", "6", "7"), "crimson": ("A", "B", "C", "f"), "fire": ("f", "g", "h", "d"),
        "pink": ("p", "q", "r", "R"), "night": ("D", "o", "p", "q"), "dusk": ("w", "D", "o", "p"), "moss": ("s", "t", "u", "v"),
        "brass": ("5", "a", "b", "c"), "sand": ("6", "7", "8", "x"), "ice": ("j", "l", "m", "n")}
BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]


def cell(w, h, draw, outline=True):
    c = Canvas(w, h)
    draw(c)
    if outline:
        c.outline("k")
    return c.rows()


# ---------------------------------------------------------------- anchors (32x32, stand on the bottom row)
def bone_totem(c):
    shade(c, rect(8, 22, 16, 9), RAMP["bone"])                       # base slab of piled bones
    for x in (9, 13, 18, 22):
        c.line(x, 23, x + (1 if x < 16 else -1), 30, "7")
    c.line(9, 26, 23, 26, "8")
    for y, w in ((15, 6.2), (8, 5.6)):                               # two skulls stacked
        shade(c, ellipse(16, y + 3, w, 4.2), RAMP["bone"], sep=True)
        c.paste(["kkg.gkk", "kk...kk"], 13 if w > 6 else 13, y + 2)
        c.paste(["8.8.8"], 14, y + 6)
    shade(c, poly([(12, 6), (16, 0), (20, 6)]), RAMP["bone"], sep=True)   # a spur of bone on top
    put(c, 16, 2, "y")


def root_core(c):
    for x0, x1 in ((7, 3), (11, 9), (21, 23), (25, 29)):              # roots splaying into the floor
        shade(c, thick(x0 + (16 - x0) * 0.4, 18, x1, 30, 2.6), RAMP["bark"], sep=False)
    shade(c, ellipse(16, 17, 8, 9), RAMP["bark"])
    shade(c, ellipse(16, 17, 4.2, 4.6), RAMP["crimson"], sep=True)    # the red heart
    shade(c, ellipse(16, 17, 2, 2), RAMP["fire"], sep=False)
    put(c, 15, 15, "H")
    for a in range(0, 360, 60):
        r = math.radians(a)
        c.line(16 + int(5.5 * math.cos(r)), 17 + int(6 * math.sin(r)), 16 + int(8 * math.cos(r)), 17 + int(9 * math.sin(r)), "5")


def spore_pod(c):
    shade(c, rect(14, 24, 4, 7), RAMP["moss"])
    shade(c, ellipse(16, 18, 8, 7.5), RAMP["pink"])
    for x, y in ((12, 14), (18, 13), (15, 19), (20, 18), (11, 20)):
        c.paste(["yy", "yx"], x, y)
    shade(c, ellipse(16, 27.5, 6, 2), RAMP["moss"], sep=False)


def dusk_anchor(c):
    shade(c, poly([(16, 1), (22, 14), (19, 30), (13, 30), (10, 14)]), RAMP["dusk"])
    shade(c, poly([(16, 9), (19, 16), (16, 24), (13, 16)]), RAMP["crimson"], sep=True)
    shade(c, ellipse(16, 16, 1.8, 2.4), RAMP["fire"], sep=False)
    c.line(16, 3, 16, 8, "q")
    shade(c, rect(9, 28, 14, 3), ("w", "z", "Z", "1"), sep=True)


def nest(c):
    shade(c, ellipse(16, 24, 13, 6.5), ("z", "Z", "1", "2"))           # rock rim on the floor
    shade(c, ellipse(16, 22, 10, 9.5), ("w", "D", "o", "p"), sep=True)   # the rift
    shade(c, poly([(16, 12), (20, 21), (16, 30), (12, 21)]), RAMP["crimson"], sep=False)
    shade(c, ellipse(16, 21, 2, 3.5), RAMP["fire"], sep=False)
    put(c, 15, 18, "H")
    for x, y in ((7, 26), (24, 27), (10, 30), (22, 31)):
        c.paste(["2", "1"], x, y)


# ---------------------------------------------------------------- 16x16 tiles and puffs
def tile(base, dark, light, motif):
    def draw(c):
        for y in range(16):
            for x in range(16):
                c.g[y][x] = base
        motif(c, dark, light)
    return draw


def waves(c, dark, light):
    for y0 in (3, 11):
        for x in range(16):
            yy = y0 + (1 if (x // 4) % 2 else 0)
            put(c, x, yy, light if x % 4 != 3 else dark)
    put(c, 5, 7, "n"); put(c, 13, 15, "n")


def ice_cracks(c, dark, light):
    c.line(0, 4, 6, 7, dark); c.line(6, 7, 9, 14, dark); c.line(9, 3, 15, 1, dark)
    for x, y in ((2, 1), (11, 9), (4, 12), (14, 13)):
        put(c, x, y, "H")
    c.line(10, 8, 14, 8, light)


def lava_cracks(c, dark, light):
    for y in range(16):
        for x in range(16):
            if BAYER[y % 4][x % 4] < 4:
                c.g[y][x] = dark
    c.line(0, 8, 5, 6, light); c.line(5, 6, 9, 9, light); c.line(9, 9, 15, 7, light)
    c.line(7, 0, 6, 6, "h"); c.line(9, 9, 11, 15, "h")
    put(c, 6, 6, "d"); put(c, 9, 9, "d")


def clock_band(c, dark, light):
    for x in range(16):
        c.g[0][x] = c.g[15][x] = "5"
        c.g[1][x] = c.g[14][x] = "a"
        c.g[2][x] = light
    for y in (4, 8, 12):
        put(c, 7, y, "5"); put(c, 8, y, "5")


def steam(c):
    for (x, y, r) in ((6, 9, 4), (10, 7, 4.5), (8, 4, 3.2)):
        shade(c, ellipse(x, y, r, r * 0.85), ("W", "4", "H", "H"), sep=False)


# ---------------------------------------------------------------- circles drawn at their exact world size
def blob_circle(size, ramp, speckle, rings=False, ticks=False):
    r = size / 2 - 1

    def draw(c):
        cx = cy = size / 2 - 0.5
        for y in range(size):
            for x in range(size):
                d = math.hypot(x - cx, y - cy) / r
                if d > 1:
                    continue
                edge = d > 0.82
                if edge and BAYER[y % 4][x % 4] < 7:
                    continue                                          # dithered soft rim
                if rings:
                    band = int(d * 7) % 2
                    ch = ramp[1] if band else ramp[2]
                    if d < 0.18:
                        ch = ramp[0]
                else:
                    ch = ramp[2] if d < 0.55 else ramp[1]
                    if (x - y) % 7 == 0 and d < 0.8:
                        ch = ramp[3]
                c.g[y][x] = ch
        for k in range(size // 6):
            a = k * 2.399
            rr = r * (0.2 + 0.6 * ((k * 37) % 10) / 10)
            put(c, cx + math.cos(a) * rr, cy + math.sin(a) * rr, speckle)
        if ticks:
            for k in range(12):
                a = k * math.pi / 6
                for t in range(3):
                    put(c, cx + math.cos(a) * (r - 2 - t), cy + math.sin(a) * (r - 2 - t), "H")
    return draw


def spore_cloud(c):
    size = 72
    cx = cy = size / 2 - 0.5
    puffs = [(0, 0, 22), (-12, -6, 14), (11, -9, 13), (-9, 11, 13), (13, 9, 14), (0, -15, 11), (-17, 3, 10), (17, -1, 10)]
    for dx, dy, r in puffs:                                           # overlapping puffs, darker underneath
        for y in range(size):
            for x in range(size):
                d = math.hypot(x - (cx + dx), y - (cy + dy)) / r
                if d > 1 or (d > 0.8 and BAYER[y % 4][x % 4] < 8):
                    continue
                lit = (x - (cx + dx)) + (y - (cy + dy)) < -r * 0.3
                c.g[y][x] = "v" if lit else ("u" if d < 0.7 else "t")
    for k in range(18):                                               # drifting pink spores
        a = k * 2.399
        rr = 6 + (k * 13) % 26
        put(c, cx + math.cos(a) * rr, cy + math.sin(a) * rr, "R" if k % 3 else "V")


def root_snare(c):
    cx = cy = 19.5
    for t in range(0, 360, 3):                                        # a thick ring of twisted roots
        a = math.radians(t)
        for w, ch in ((17.5, "5"), (16.5, "6"), (15.5, "7" if t % 24 < 12 else "6")):
            put(c, cx + math.cos(a) * w, cy + math.sin(a) * w, ch)
    for k in range(10):                                               # thorns pointing inward
        a = k * math.pi / 5
        for t in range(6):
            rr = 15 - t
            put(c, cx + math.cos(a + t * 0.05) * rr, cy + math.sin(a + t * 0.05) * rr, "6" if t < 3 else "5")
        put(c, cx + math.cos(a) * 9, cy + math.sin(a) * 9, "f")
    for t in range(0, 360, 30):
        a = math.radians(t)
        put(c, cx + math.cos(a) * 18.5, cy + math.sin(a) * 18.5, "t")


# ---------------------------------------------------------------- sheet
def paste(sheet, rows, x0, y0):
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch != ".":
                sheet[y0 + y][x0 + x] = ch


def to_rgba(rows, alpha=255):
    rgb = {k: tuple(int(v[i:i + 2], 16) for i in (1, 3, 5)) for k, v in L.items()}
    return [[(0, 0, 0, 0) if ch == "." else rgb[ch] + (alpha,) for ch in row] for row in rows]


def sand_fog(size=640, hole=150, soft=110):
    c = size / 2 - 0.5
    rows = []
    for y in range(size):
        row = []
        for x in range(size):
            d = math.hypot(x - c, y - c)
            if d < hole:
                row.append(".")
                continue
            t = (d - hole) / soft
            if t < 1 and BAYER[y % 4][x % 4] >= t * 16:
                row.append(".")
                continue
            n = ((x * 73856093) ^ (y * 19349663) ^ ((x // 3) * 83492791)) & 0xffff   # hashed, so no bands form
            row.append("8" if n % 211 == 0 else ("7" if n % 13 == 0 else "6"))
        rows.append("".join(row))
    return rows


def main(out):
    sheet = [["."] * 256 for _ in range(160)]
    paste(sheet, cell(32, 32, bone_totem), 0, 0)
    paste(sheet, cell(32, 32, root_core), 32, 0)
    paste(sheet, cell(32, 32, spore_pod), 64, 0)
    paste(sheet, cell(32, 32, dusk_anchor), 96, 0)
    paste(sheet, cell(16, 16, steam), 128, 0)
    paste(sheet, cell(16, 16, tile("l", "j", "m", waves), False), 144, 0)
    paste(sheet, cell(16, 16, tile("n", "m", "H", ice_cracks), False), 160, 0)
    paste(sheet, cell(16, 16, tile("e", "F", "g", lava_cracks), False), 176, 0)
    paste(sheet, cell(16, 16, tile("b", "a", "c", clock_band), False), 192, 0)
    paste(sheet, cell(32, 32, nest), 208, 0)
    paste(sheet, cell(72, 72, spore_cloud, False), 0, 32)
    paste(sheet, cell(80, 80, blob_circle(80, ("6", "7", "8", "x"), "5", rings=True), False), 72, 32)
    paste(sheet, cell(80, 80, blob_circle(80, ("j", "l", "m", "n"), "H", rings=True, ticks=True), False), 152, 32)
    paste(sheet, cell(40, 40, root_snare, False), 0, 112)
    rows = ["".join(r) for r in sheet]
    pngio.write(os.path.join(out, "boss_fx.png"), 256, 160, to_rgba(rows))
    fog = sand_fog()
    pngio.write(os.path.join(out, "sand_fog.png"), 640, 640, to_rgba(fog))
    print("boss_fx.png 256x160, sand_fog.png 640x640")


if __name__ == "__main__":
    main(sys.argv[1])
