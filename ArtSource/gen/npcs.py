"""NPC + room-feature sheet, 32x32 cells, 8 per row (256x64), Apollo palette, feet on row 29.
Cells: tallow0 tallow1 pilgrim0 pilgrim1 sunstone0 sunstone1 fire0 fire1 / fire2 fire3 altar fragment0 fragment1 bubble.
Prints {legend, frames:[["sheet", rows]]} for strip.py / preview.py."""
import json
import math
import sys

sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
from compose import Canvas  # noqa: E402
from rig import ellipse, poly, thick, rect, put, fill, shade  # noqa: E402

L = {"k": "#090a14", "1": "#394a50", "2": "#577277", "3": "#819796", "4": "#c7cfcc", "H": "#ebede9",
     "5": "#4d2b32", "6": "#7a4841", "7": "#ad7757", "8": "#c09473",
     "a": "#884b2b", "b": "#be772b", "c": "#de9e41", "d": "#e8c170",
     "e": "#602c2c", "f": "#a53030", "g": "#cf573c", "h": "#da863e",
     "i": "#253a5e", "j": "#3c5e8b", "l": "#4f8fba", "m": "#73bed3", "n": "#a4dddb",
     "o": "#402751", "p": "#7a367b", "q": "#a23e8c",
     "s": "#25562e", "t": "#468232", "u": "#75a743",
     "x": "#d7b594", "y": "#e7d5b3", "z": "#151d28", "Z": "#202e37", "w": "#10141f"}

STONE = ("w", "Z", "1", "2")
STONE_LIGHT = ("1", "2", "3", "4")
WOOD = ("5", "6", "7", "8")
GOLD = ("a", "b", "c", "d")
CLOTH = ("o", "p", "q", "q")
ROBE = ("5", "6", "7", "x")
SKIN = ("6", "7", "8", "x")
WAX = ("8", "x", "y", "H")
FEET = 29


def tallow(c, f):
    """Old Tallow: a hunched candle-maker under a towering pack of candles, lantern in hand."""
    bob = f
    # pack frame with candles of different heights
    shade(c, rect(14, 10 + bob, 13, 11), WOOD)
    fill(c, rect(14, 14 + bob, 13, 1), "5")
    for i, h in enumerate((8, 11, 7, 10, 9)):
        x = 15 + i * 2.4
        top = 11 + bob - h
        fill(c, rect(x, top, 2, h), "y")
        fill(c, rect(x, top, 1, h), "x")
        put(c, x, top - 1, "c" if (i + f) % 2 else "d")
        put(c, x + 1, top - 1, "k")
    # body: long patched coat
    shade(c, poly([(8, 13 + bob), (17, 13 + bob), (19, FEET), (6, FEET)]), ROBE)
    fill(c, rect(7, 22, 11, 1), "a")
    # head: bald, big nose, white whiskers
    shade(c, ellipse(10, 10 + bob, 4.2, 4), SKIN)
    put(c, 7, 11 + bob, "e")
    put(c, 9, 9 + bob, "k")
    fill(c, rect(7, 13 + bob, 5, 2), "4")
    put(c, 6, 10 + bob, "7")
    # arm holding a lantern on a pole
    shade(c, thick(9, 16 + bob, 4, 19, 2), ROBE, sep=False)
    c.line(4, 19, 4, 24, "1")
    shade(c, rect(2, 24, 5, 4), GOLD, sep=False)
    put(c, 4, 25, "d" if f == 0 else "H"); put(c, 4, 26, "c")
    # boots
    fill(c, rect(7, 28, 4, 2), "5"); fill(c, rect(14, 28, 4, 2), "5")


def pilgrim(c, f):
    """A hooded pilgrim of the Sun: grey robe, sun pendant, walking staff with a little bell."""
    sway = f
    shade(c, poly([(10, 11), (21, 11), (23, FEET), (8, FEET)]), STONE_LIGHT)
    c.line(15, 13, 15, 27, "2")
    shade(c, ellipse(15.5, 9, 5, 5.2), STONE_LIGHT)                 # hood
    shade(c, ellipse(16.5, 10, 2.8, 3), SKIN, sep=False)             # face in shadow
    fill(c, rect(15, 9, 4, 1), "6")
    put(c, 17, 10, "k")
    shade(c, ellipse(15.5, 16, 1.6, 1.6), GOLD, sep=False)          # sun pendant
    c.line(25, 4 + sway, 25, FEET, "6")                              # staff
    c.line(26, 4 + sway, 26, FEET, "7")
    shade(c, ellipse(25.5, 5 + sway, 1.6, 1.6), GOLD, sep=False)
    shade(c, thick(21, 15, 25, 15, 2), STONE_LIGHT, sep=False)      # hand on staff
    fill(c, rect(10, 28, 4, 2), "5"); fill(c, rect(17, 28, 4, 2), "5")


def sunstone(c, f):
    """Sunstone: a rough rock outcrop with a cluster of amber crystals holding buried sunlight. No altar, no idol."""
    rock = poly([(3, FEET), (5, 25), (8, 21), (11, 22), (14, 19), (18, 21), (21, 19), (25, 23), (28, 26), (29, FEET)])
    shade(c, rock, STONE)
    for x0, y0, x1, y1 in ((9, 26, 12, 23), (17, 25, 21, 22), (13, 28, 16, 25)):
        c.line(x0, y0, x1, y1, "w")
    glow = 0.6 * f
    # crystals: tall centre spire with smaller shards around it
    for cx, top, w in ((16, 4 - f, 3.2), (11, 10, 2.4), (21, 9 - f, 2.6), (8, 15, 1.8), (24, 14, 1.8)):
        spire = poly([(cx - w, 20), (cx - w * 0.6, top + 3), (cx, top), (cx + w * 0.6, top + 3), (cx + w, 20)])
        shade(c, spire, GOLD)
        c.line(cx - 1, top + 3, cx - 1, 18, "H" if cx == 16 else "d")
    for i in range(6):   # motes of light
        a = i * math.pi / 3 + f * 0.5
        put(c, 16 + (9 + glow) * math.cos(a), 12 + (7 + glow) * math.sin(a), "d" if i % 2 else "c")


def fire(c, f):
    """Campfire: ring of stones, crossed logs, a four-frame flame."""
    for i in range(10):
        a = i * math.pi / 5
        shade(c, ellipse(16 + 8 * math.cos(a), 25 + 3 * math.sin(a), 1.8, 1.4), STONE_LIGHT)
    shade(c, thick(9, 27, 23, 22, 2.2), WOOD, sep=False)
    shade(c, thick(9, 22, 23, 27, 2.2), WOOD, sep=False)
    h = (9, 11, 8, 10)[f]
    sway = (0, 1, 0, -1)[f]
    flame = poly([(10, 25), (22, 25), (19 + sway, 25 - h + 3), (16 + sway, 24 - h - 2), (13 + sway, 25 - h + 4)])
    shade(c, flame, ("f", "g", "h", "d"), sep=False)
    inner = poly([(13, 25), (19, 25), (16 + sway, 25 - h + 2)])
    fill(c, inner, "d")
    put(c, 16 + sway, 25 - h + 4, "H")
    for i in range(3):                                               # sparks
        put(c, 12 + ((f * 5 + i * 7) % 9), 10 - ((f * 3 + i * 4) % 6), "c")


def altar(c, f):
    """Gambler's altar: a cracked plinth with a skull and two bone dice."""
    shade(c, rect(7, 18, 18, 11), STONE)
    fill(c, rect(7, 28, 18, 1), "w")
    c.line(12, 19, 15, 27, "w")
    shade(c, ellipse(16, 13, 4.5, 4), ("8", "x", "y", "H"))
    fill(c, rect(14, 12, 2, 2), "k"); fill(c, rect(18, 12, 2, 2), "k")
    put(c, 17, 15, "k")
    for x0 in (8, 21):
        shade(c, rect(x0, 15, 4, 4), ("8", "x", "y", "H"), sep=False)
        put(c, x0 + 1, 16, "k"); put(c, x0 + 2, 17, "k")
    fill(c, rect(9, 21, 14, 1), "f")


def fragment(c, f):
    """A shard of the buried Sun, hovering and pulsing."""
    hover = f
    pts = poly([(16, 4 + hover), (21, 12 + hover), (16, 22 + hover), (11, 12 + hover)])
    shade(c, pts, ("b", "c", "d", "H"))
    c.line(16, 5 + hover, 16, 21 + hover, "H")
    for i in range(8):
        a = i * math.pi / 4 + f * 0.4
        put(c, 16 + 9 * math.cos(a), 13 + hover + 9 * math.sin(a), "c" if i % 2 else "d")
    shade(c, ellipse(16, 27, 5, 1.4), ("w", "Z", "Z", "Z"), sep=False)


def bubble(c, f):
    """Interaction bubble shown over NPCs and things you can use."""
    shade(c, ellipse(16, 12, 8, 7), ("2", "3", "4", "H"))
    c.line(13, 18, 11, 23, "4"); put(c, 12, 20, "3")
    fill(c, rect(15, 7, 2, 7), "b")
    fill(c, rect(15, 15, 2, 2), "b")


CELLS = [(tallow, 0), (tallow, 1), (pilgrim, 0), (pilgrim, 1), (sunstone, 0), (sunstone, 1), (fire, 0), (fire, 1),
         (fire, 2), (fire, 3), (altar, 0), (fragment, 0), (fragment, 1), (bubble, 0)]
NAMES = ["tallow0", "tallow1", "pilgrim0", "pilgrim1", "sunstone0", "sunstone1", "fire0", "fire1",
         "fire2", "fire3", "altar", "fragment0", "fragment1", "bubble"]

if __name__ == "__main__":
    sheet = Canvas(256, 64)
    for i, (fn, frame) in enumerate(CELLS):
        c = Canvas(32, 32)
        fn(c, frame)
        c.outline("k")
        sheet.paste(c.rows(), (i % 8) * 32, (i // 8) * 32)
    print(json.dumps({"legend": L, "frames": [["sheet", sheet.rows()]], "names": NAMES}))
