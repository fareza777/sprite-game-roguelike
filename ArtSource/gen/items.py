"""Item icon sheet: 36 icons, 16x16 each, 8 per row (128x80), Apollo palette, top-left light, 1px dark outline.
Order matches ICONS below; the Unity setup slices by these names.
Prints {legend, frames:[["sheet", rows]]} for strip.py / preview.py."""
import json
import math
import sys

sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
from compose import Canvas  # noqa: E402
from rig import ellipse, poly, thick, rect, put, fill, shade  # noqa: E402

L = {"k": "#090a14",
     "1": "#394a50", "2": "#577277", "3": "#819796", "4": "#c7cfcc", "H": "#ebede9",
     "5": "#4d2b32", "6": "#7a4841", "7": "#ad7757", "8": "#c09473",
     "a": "#884b2b", "b": "#be772b", "c": "#de9e41", "d": "#e8c170",
     "e": "#602c2c", "f": "#a53030", "g": "#cf573c", "h": "#da863e",
     "i": "#253a5e", "j": "#3c5e8b", "l": "#4f8fba", "m": "#73bed3", "n": "#a4dddb",
     "o": "#402751", "p": "#7a367b", "q": "#a23e8c", "r": "#c65197",
     "s": "#25562e", "t": "#468232", "u": "#75a743", "v": "#a8ca58",
     "x": "#d7b594", "y": "#e7d5b3", "z": "#151d28", "Z": "#202e37"}

STEEL = ("1", "2", "3", "4")
WOOD = ("5", "6", "7", "8")
GOLD = ("a", "b", "c", "d")
RED = ("e", "f", "g", "h")
BLUE = ("i", "j", "l", "m")
CYAN = ("j", "l", "m", "n")
PURPLE = ("o", "p", "q", "r")
GREEN = ("s", "t", "u", "v")
BONE = ("7", "8", "x", "y")
DARK = ("z", "Z", "1", "2")


def blade(c, x0, y0, x1, y1, w, ramp=STEEL, edge="H"):
    shade(c, thick(x0, y0, x1, y1, w), ramp)
    c.line(int(x0), int(y0), int(x1), int(y1), edge)


def guard(c, cx, cy, half, ramp=GOLD):
    # perpendicular to a blade that runs bottom-left -> top-right
    shade(c, thick(cx - half, cy - half, cx + half, cy + half, 1.8), ramp, sep=False)


def handle(c, x0, y0, x1, y1, ramp=WOOD, w=1.6):
    shade(c, thick(x0, y0, x1, y1, w), ramp, sep=False)


# ---------------- weapons ----------------
def sword(c):
    blade(c, 6, 9, 13, 2, 2.2)
    guard(c, 5.5, 9.5, 2.5)
    handle(c, 2, 13, 5, 10)
    put(c, 2, 13, "c")


def longsword(c):
    blade(c, 5, 10, 14, 1, 2.6)
    guard(c, 4.5, 10.5, 3, BLUE)
    handle(c, 1, 14, 4, 11, DARK)
    put(c, 1, 14, "m")


def greatsword(c):
    blade(c, 5, 10, 14, 1, 3.6)
    guard(c, 4.5, 10.5, 3.5)
    handle(c, 1, 14, 4, 11)
    put(c, 1, 14, "d")


def axe(c):
    handle(c, 3, 14, 11, 3, WOOD, 1.8)
    shade(c, poly([(8, 2), (14, 3), (15, 8), (11, 9), (9, 6)]), STEEL)
    c.line(14, 3, 15, 8, "H")


def mace(c):
    handle(c, 2, 14, 9, 6, WOOD, 1.8)
    shade(c, ellipse(10.5, 4.5, 3.6, 3.6), STEEL)
    for p in ((10, 0), (14, 4), (10, 9), (6, 4)):
        put(c, *p, "3")
    put(c, 9, 3, "H")


def flail(c):
    handle(c, 1, 14, 5, 9, WOOD, 1.8)
    for i, p in enumerate(((6, 8), (7, 7), (8, 6))):
        put(c, *p, "3" if i % 2 else "2")
    shade(c, ellipse(11, 4.5, 3.4, 3.4), STEEL)
    for p in ((11, 0), (15, 4), (7, 4), (11, 9)):
        put(c, *p, "4")
    put(c, 10, 3, "H")


def hammer(c):
    handle(c, 2, 14, 9, 6, WOOD, 1.8)
    head = poly([(6, 2), (13, 0), (15, 6), (8, 8)])
    shade(c, head, STEEL)
    c.line(13, 0, 15, 6, "H")


def dagger(c):
    blade(c, 7, 8, 13, 2, 1.8)
    guard(c, 6.5, 8.5, 2)
    handle(c, 4, 11, 6, 9, ("o", "p", "q", "r"))
    put(c, 3, 12, "c")


def spear(c):
    handle(c, 1, 15, 10, 6, WOOD, 1.6)
    shade(c, poly([(9, 5), (14, 1), (11, 7)]), STEEL)
    shade(c, poly([(10, 6), (15, 0), (12, 5)]), STEEL, sep=False)
    put(c, 14, 1, "H")
    put(c, 9, 7, "f"); put(c, 8, 8, "f")


def crossbow(c):
    handle(c, 2, 14, 11, 5, WOOD, 2.4)
    # curved steel prod across the front of the stock, string drawn back toward the grip
    arc = [(6, 2), (8, 3), (10, 4), (11, 5), (12, 6), (13, 8), (14, 10)]
    for a, b in zip(arc, arc[1:]):
        shade(c, thick(a[0], a[1], b[0], b[1], 1.6), STEEL, sep=False)
    c.line(6, 3, 8, 8, "y"); c.line(13, 10, 8, 8, "y")
    put(c, 12, 4, "H"); put(c, 6, 2, "4")


def staff(c):
    handle(c, 2, 15, 11, 5, WOOD, 1.6)
    shade(c, ellipse(12, 4, 2.8, 2.8), CYAN)
    put(c, 11, 3, "H")
    put(c, 14, 1, "n"); put(c, 9, 2, "m")


def scythe(c):
    handle(c, 3, 15, 9, 2, WOOD, 1.6)
    for i in range(9):
        a = math.pi * (0.95 + 0.55 * i / 8)
        x, y = 9 + 6 * math.cos(a) * -1, 3 + 2.5 * math.sin(a)
        put(c, x, y, "3" if i % 3 else "4")
        put(c, x, y + 1, "2")
    put(c, 15, 4, "H")


# ---------------- armor ----------------
def torso(c, ramp, trim=None, collar=None):
    body = poly([(3, 3), (13, 3), (14, 6), (12, 7), (12, 14), (4, 14), (4, 7), (2, 6)])
    shade(c, body, ramp)
    if trim:
        fill(c, rect(4, 13, 8, 1), trim)
    if collar:
        fill(c, rect(6, 3, 4, 1), collar)


def plate(c):
    torso(c, STEEL, "c", "k")
    c.line(8, 4, 8, 12, "2")
    fill(c, rect(5, 8, 6, 1), "c")
    put(c, 5, 5, "H")


def mail(c):
    torso(c, STEEL, "a", "k")
    for y in range(5, 13):
        for x in range(4, 12):
            if (x + y) % 2 == 0 and c.g[y][x] in "234":
                put(c, x, y, "1")


def leather(c):
    torso(c, WOOD, None, "5")
    fill(c, rect(4, 9, 8, 1), "5")
    put(c, 8, 9, "c")
    c.line(5, 4, 10, 12, "6")


def robe(c):
    body = poly([(4, 2), (12, 2), (14, 15), (2, 15)])
    shade(c, body, PURPLE)
    c.line(8, 3, 8, 14, "c")
    fill(c, rect(5, 2, 6, 1), "o")
    fill(c, rect(3, 14, 10, 1), "c")


def cloak(c):
    body = poly([(6, 1), (10, 1), (15, 15), (1, 15)])
    shade(c, body, BLUE)
    c.line(8, 2, 6, 14, "i"); c.line(8, 2, 11, 14, "i")
    put(c, 8, 2, "c"); put(c, 7, 2, "c")


def helm(c):
    dome = {q for q in ellipse(8, 8, 6, 6.5) if q[1] <= 12}
    shade(c, dome, STEEL)
    fill(c, rect(4, 7, 9, 2), "k")
    put(c, 8, 7, "2"); put(c, 8, 8, "2")
    for y in range(1, 4):
        put(c, 8, y, "g")
    put(c, 5, 4, "H")


def gauntlet(c):
    shade(c, poly([(4, 5), (10, 4), (11, 11), (5, 12)]), STEEL)
    for i in range(4):
        shade(c, rect(5 + i * 2, 1 + (i % 2), 2, 5), STEEL, sep=False)
    shade(c, rect(10, 7, 4, 3), STEEL, sep=False)
    fill(c, rect(5, 12, 7, 2), "6")


def boots(c):
    shade(c, poly([(3, 2), (8, 2), (8, 10), (13, 11), (13, 14), (3, 14)]), WOOD)
    fill(c, rect(3, 13, 11, 1), "5")
    fill(c, rect(3, 3, 5, 1), "a")
    put(c, 4, 4, "8")


# ---------------- charms ----------------
def ring(c):
    band = {q for q in ellipse(8, 9, 5, 5) if (q[0] - 8) ** 2 + (q[1] - 9) ** 2 > 7}
    shade(c, band, GOLD)
    put(c, 5, 6, "d")


def ringGem(c):
    ring(c)
    shade(c, ellipse(8, 4, 2.6, 2.6), RED, sep=False)
    put(c, 7, 3, "H")


def amulet(c):
    for i in range(10):
        a = math.pi * i / 9
        put(c, 8 + 6 * math.cos(a), 1 + 5 * math.sin(a), "c")
    shade(c, poly([(8, 7), (12, 10), (8, 15), (4, 10)]), BLUE)
    put(c, 7, 9, "n")
    fill(c, rect(7, 6, 3, 1), "c")


def talisman(c):
    c.line(3, 1, 8, 6, "8"); c.line(13, 1, 8, 6, "8")
    shade(c, poly([(5, 6), (11, 6), (12, 12), (8, 15), (4, 12)]), BONE)
    put(c, 7, 9, "e"); put(c, 9, 9, "e"); put(c, 8, 11, "6")


def brooch(c):
    shade(c, ellipse(8, 8, 6, 6), GOLD)
    shade(c, ellipse(8, 8, 3.4, 3.4), GREEN, sep=False)
    put(c, 7, 7, "v")
    for p in ((8, 1), (15, 8), (8, 15), (1, 8)):
        put(c, *p, "d")


def idol(c):
    shade(c, ellipse(8, 4, 3, 3), GOLD)
    shade(c, poly([(5, 7), (11, 7), (12, 15), (4, 15)]), GOLD)
    put(c, 7, 4, "k"); put(c, 9, 4, "k")
    fill(c, rect(5, 10, 7, 1), "a")
    put(c, 6, 3, "d")


# ---------------- relics ----------------
def tome(c):
    shade(c, poly([(3, 2), (13, 2), (13, 14), (3, 14)]), RED)
    fill(c, rect(4, 13, 9, 1), "y")
    c.line(3, 2, 3, 14, "e")
    shade(c, ellipse(8.5, 7.5, 2.3, 2.3), GOLD, sep=False)
    put(c, 8, 7, "d")


def skull(c):
    shade(c, ellipse(8, 7, 5.5, 5), BONE)
    shade(c, rect(5, 11, 7, 3), BONE, sep=False)
    fill(c, rect(5, 7, 2, 2), "k"); fill(c, rect(10, 7, 2, 2), "k")
    put(c, 8, 10, "k")
    for x in (6, 8, 10):
        put(c, x, 13, "6")


def orb(c):
    shade(c, ellipse(8, 7, 5.5, 5.5), PURPLE)
    shade(c, ellipse(7, 6, 2.5, 2.5), ("p", "q", "r", "H"), sep=False)
    fill(c, rect(4, 13, 9, 2), "a")
    put(c, 6, 4, "H")


def lantern(c):
    fill(c, rect(7, 0, 3, 1), "2")
    shade(c, rect(4, 2, 9, 2), STEEL)
    shade(c, rect(5, 4, 7, 8), GOLD)
    shade(c, ellipse(8.5, 8, 1.8, 2.6), ("c", "d", "H", "H"), sep=False)
    shade(c, rect(4, 12, 9, 2), STEEL, sep=False)
    c.line(5, 4, 5, 11, "1"); c.line(11, 4, 11, 11, "1")


def hourglass(c):
    shade(c, rect(3, 1, 11, 2), WOOD)
    shade(c, rect(3, 13, 11, 2), WOOD)
    shade(c, poly([(4, 3), (13, 3), (9, 8), (13, 13), (4, 13), (8, 8)]), CYAN, sep=False)
    fill(c, rect(6, 11, 5, 2), "c"); put(c, 8, 9, "c"); put(c, 8, 10, "c")


def feather(c):
    vane = poly([(12, 1), (14, 3), (8, 11), (4, 12), (6, 8)])
    shade(c, vane, DARK)
    c.line(3, 14, 13, 2, "3")
    put(c, 12, 2, "4")


def bell(c):
    rows = [1.5, 2.6, 3.2, 3.4, 3.4, 3.6, 4.0, 4.6, 5.4, 6.2]
    pts = set()
    for i, half in enumerate(rows):
        for x in range(round(8 - half), round(8 + half) + 1):
            pts.add((x, 3 + i))
    shade(c, pts, GOLD)
    fill(c, rect(2, 12, 13, 1), "a")
    put(c, 8, 13, "c"); put(c, 8, 14, "c")
    put(c, 8, 1, "a"); put(c, 8, 2, "a")


def heart(c):
    pts = ellipse(5.5, 6, 3.5, 3.5) | ellipse(10.5, 6, 3.5, 3.5) | poly([(2, 7), (14, 7), (8, 15)])
    shade(c, pts, RED)
    put(c, 4, 4, "H"); put(c, 5, 4, "h")


def crown(c):
    shade(c, poly([(2, 5), (5, 9), (8, 3), (11, 9), (14, 5), (13, 13), (3, 13)]), GOLD)
    for x, ch in ((5, "g"), (8, "l"), (11, "u")):
        put(c, x, 11, ch)
    for p in ((2, 4), (8, 2), (14, 4)):
        put(c, *p, "d")


def candle(c):
    shade(c, rect(5, 7, 6, 7), BONE)
    fill(c, rect(3, 13, 10, 2), "a")
    put(c, 8, 6, "k")
    shade(c, ellipse(8, 4, 1.6, 2.4), ("b", "c", "d", "H"), sep=False)
    put(c, 6, 8, "y")


ICONS = [sword, longsword, greatsword, axe, mace, flail, hammer, dagger,
         spear, crossbow, staff, scythe, plate, mail, leather, robe,
         cloak, helm, gauntlet, boots, ring, ringGem, amulet, talisman,
         brooch, idol, tome, skull, orb, lantern, hourglass, feather,
         bell, heart, crown, candle]

if __name__ == "__main__":
    sheet = Canvas(128, 80)
    for i, fn in enumerate(ICONS):
        c = Canvas(16, 16)
        fn(c)
        # keep a 1px margin for the outline
        for y in range(16):
            for x in range(16):
                if (x in (0, 15) or y in (0, 15)) and c.g[y][x] != ".":
                    c.g[y][x] = "."
        c.outline("k")
        sheet.paste(c.rows(), (i % 8) * 16, (i // 8) * 16)
    print(json.dumps({"legend": L, "frames": [["sheet", sheet.rows()]], "names": [f.__name__ for f in ICONS]}))
