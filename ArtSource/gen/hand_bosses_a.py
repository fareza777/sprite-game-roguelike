"""Hand-drawn bosses, B2-B6: Mother Morel, the Tidecaller, the Furnace Saint, the Pale Matriarch, the Verdant Hierophant.
Every part is drawn on a full 64x64 sheet in canvas coordinates; feet rest on row 62, facing right."""
import math

from hand_boss import RAMP, ellipse, poly, put, sheet, shade, spots, thick
from hand_core import Creature, biped, part, sway

S = 64


def arm(c, shoulder, elbow, hand, sleeve, skin, width=4):
    """Two-segment arm: sleeve to the elbow, bare forearm to the hand."""
    shade(c, thick(*shoulder, *elbow, width), RAMP[sleeve], sep=False)
    shade(c, thick(*elbow, *hand, width - 1), RAMP[skin], sep=False)


def sparkle_ring(c, cx, cy, r, chars, n=8, phase=0.0):
    import math
    for k in range(n):
        a = phase + k * 2 * math.pi / n
        put(c, cx + math.cos(a) * r, cy + math.sin(a) * r, chars[k % len(chars)])


# ================================================================ MOTHER MOREL (B2)
_CAP_SPOT = [".yy.", "yyyx", "yyxx", ".xx."]
_CAP_DOT = ["yy", "yx"]


def _morel_head(c, hurt=False):
    shade(c, poly([(23, 15), (45, 15), (46, 29), (43, 31), (25, 31), (22, 29)]), RAMP["linen"])   # mycelium hair
    for x0 in (24, 27, 41, 44):
        c.line(x0, 18, x0 - 1, 29, "x")
    shade(c, ellipse(34, 23, 8.5, 9), RAMP["flesh"])
    if hurt:
        c.line(30, 23, 32, 23, "6"); c.line(36, 23, 38, 23, "6")
    else:
        c.paste(["sV", "sv"], 30, 22); c.paste(["sV", "sv"], 36, 22)
    c.paste(["R.....R"], 29, 25)
    c.paste(["6x6", ".6."] if not hurt else ["666"], 33, 27)
    cap = {p for p in ellipse(32, 13, 24, 12) if p[1] <= 16}
    shade(c, cap, RAMP["violet"])
    gills = {p for p in ellipse(32, 16, 21, 3.2) if p[1] >= 16}
    shade(c, gills, RAMP["wood"], sep=False)
    for x in range(13, 52, 3):
        for y in range(16, 20):
            if c.g[y][x] != ".":
                c.g[y][x] = "5"
    spots(c, [(14, 8), (25, 3), (37, 5), (47, 10), (20, 12), (31, 10)], _CAP_SPOT)
    spots(c, [(12, 13), (43, 4), (52, 13), (28, 7)], _CAP_DOT)


def _morel_body(c):
    shade(c, poly([(22, 30), (42, 30), (45, 41), (47, 52), (17, 52), (19, 41)]), RAMP["green"], sep=False)
    shade(c, ellipse(32, 31, 12, 3.5), RAMP["pink"], sep=False)          # gill collar
    for x in range(22, 43, 2):
        put(c, x, 33, "p")
    c.line(27, 44, 24, 51, "E"); c.line(37, 44, 40, 51, "E")              # gown folds below the sash
    c.line(20, 42, 44, 42, "p"); c.line(20, 43, 44, 43, "q")              # violet sash
    spots(c, [(21, 45), (39, 38)], [".qq.", "qRqq", ".xx.", ".x.."])     # little mushrooms growing on the gown


def _morel_hem(c):
    shade(c, poly([(17, 50), (47, 50), (52, 61), (12, 61)]), RAMP["green"], sep=False)
    for x in range(12, 53, 4):
        c.paste(["qq", "pp"], x, 60)                                      # gill-frill scallops
    c.line(23, 52, 20, 59, "E"); c.line(41, 52, 44, 59, "E")


def _morel_arm(c, hand, item=None):
    arm(c, (43, 34), ((43 + hand[0]) // 2 + 1, (34 + hand[1]) // 2 + 2), hand, "moss", "flesh")
    hx, hy = hand
    for k, (dx, dy) in enumerate(((3, 1), (2, 3), (0, 3))):              # mycelium fingers
        c.line(hx, hy, hx + dx, hy + dy, "x" if k % 2 else "y")
    if item:
        item(c, hx, hy)


def _spore_orb(c, x, y):
    shade(c, ellipse(x + 1, y - 4, 3, 3), ("t", "u", "v", "V"), sep=False)
    put(c, x, y - 5, "H")


mother_morel = Creature(
    layers=[
        ("back", {"rest": (sheet(S, S, lambda c: arm(c, (22, 34), (19, 40), (17, 47), "green", "flesh")), 0, 0)}),
        ("legs", part(
            rest=(sheet(S, S, _morel_hem), 0, 0),
            a=(sway(sheet(S, S, _morel_hem)[50:], -2), 0, 50),
            b=(sway(sheet(S, S, _morel_hem)[50:], 2), 0, 50),
        )),
        ("body", part(rest=(sheet(S, S, _morel_body), 0, 0))),
        ("head", part(rest=(sheet(S, S, _morel_head), 0, 0), hurt=(sheet(S, S, lambda c: _morel_head(c, True)), 0, 0))),
        ("arm", part(
            rest=(sheet(S, S, lambda c: _morel_arm(c, (47, 47))), 0, 0),
            up=(sheet(S, S, lambda c: _morel_arm(c, (50, 22), _spore_orb)), 0, 0),
            fwd=(sheet(S, S, lambda c: _morel_arm(c, (58, 37))), 0, 0),
            follow=(sheet(S, S, lambda c: _morel_arm(c, (54, 47))), 0, 0),
        )),
        ("fx", part(
            smear=(sheet(S, S, lambda c: [sparkle_ring(c, 56, 37, r, "vVu", 10, r * 0.3) for r in (5, 8)]), 0, 0),
            cast0=(sheet(S, S, lambda c: sparkle_ring(c, 51, 17, 6, "vV", 8)), 0, 0),
            cast1=(sheet(S, S, lambda c: sparkle_ring(c, 51, 17, 9, "Vvu", 10, 0.4)), 0, 0),
            cast2=(sheet(S, S, lambda c: sparkle_ring(c, 51, 17, 12, "uvV", 12, 0.8)), 0, 0),
        )),
    ],
    plan=biped, size=S, boss=True,
)



def polearm(c, hand, d, back, front, head):
    """A long shaft through the hand along unit direction d; head(c, tip, d) draws the business end."""
    hx, hy = hand
    bx, by = round(hx - d[0] * back), round(hy - d[1] * back)
    tx, ty = round(hx + d[0] * front), round(hy + d[1] * front)
    c.line(bx, by, tx, ty, "6")
    ox, oy = (1, 0) if abs(d[1]) >= abs(d[0]) else (0, 1)
    c.line(bx + ox, by + oy, tx + ox, ty + oy, "7")
    head(c, (tx, ty), d)


def trident_head(c, tip, d):
    tx, ty = tip
    px, py = -d[1], d[0]                       # perpendicular
    c.line(round(tx - px * 4), round(ty - py * 4), round(tx + px * 4), round(ty + py * 4), "3")
    for k in (-4, 0, 4):
        sx, sy = tx + px * k, ty + py * k
        ln = 6 if k == 0 else 4
        c.line(round(sx), round(sy), round(sx + d[0] * ln), round(sy + d[1] * ln), "4" if k <= 0 else "3")
        put(c, sx + d[0] * (ln + 1), sy + d[1] * (ln + 1), "H")


# ================================================================ THE TIDECALLER (B3)
def _tide_head(c, hurt=False):
    for (x0, y0), (x1, y1) in (((30, 8), (27, 2)), ((34, 7), (35, 1)), ((38, 8), (42, 3))):   # coral crown
        shade(c, thick(x0, y0, x1, y1, 2.4), RAMP["ember"], sep=False)
        put(c, x1, y1, "R")
    crest = poly([(22, 13), (25, 6), (28, 10), (31, 4), (34, 9), (38, 5), (40, 10), (44, 12), (30, 14)])
    shade(c, crest, RAMP["sea"], sep=False)
    shade(c, ellipse(35, 16, 12, 9), RAMP["teal"])
    shade(c, {p for p in ellipse(38, 21, 9, 4) if p[1] >= 19}, RAMP["sea"], sep=False)       # pale throat
    for x in (27, 29, 31):
        c.line(x, 14, x, 20, "G")                                                             # gill slits
    c.paste(["4HH4", "H..H", "H..H", "4HH4"] if not hurt else ["....", "4HH4", "1111", "...."], 39, 11)
    if not hurt:
        c.paste(["kk", "kn"], 40, 12)
    c.line(36, 21, 47, 19, "k")
    for x in (38, 41, 44):
        put(c, x, 21 - (x - 36) // 5, "H")


def _tide_body(c):
    shade(c, poly([(24, 24), (43, 24), (45, 41), (22, 41)]), RAMP["teal"])
    for y in range(27, 39, 2):
        for x in range(25 + (y // 2) % 2, 43, 3):
            put(c, x, y, "l")                                                                 # scales
    for pts in ([(18, 26), (28, 22), (26, 31)], [(39, 22), (50, 25), (42, 31)]):              # shoulder fins
        shade(c, poly(pts), RAMP["sea"], sep=False)
    c.line(22, 39, 45, 39, "b"); c.line(22, 40, 45, 40, "a")
    for x in (26, 31, 36, 41):
        put(c, x, 39, "H")                                                                    # pearls


def _tide_hem(c):
    shade(c, poly([(22, 40), (45, 40), (49, 61), (17, 61)]), RAMP["blue"], sep=False)
    for x0 in (19, 25, 33, 41, 47):
        c.line(x0, 55, x0 + 1, 61, "t"); c.line(x0 + 1, 57, x0 + 2, 61, "s")                  # seaweed
    c.line(30, 43, 28, 60, "G"); c.line(37, 43, 39, 60, "G")


def _tide_arm(c, hand, d=(0, -1), back=16, front=20):
    arm(c, (42, 28), ((42 + hand[0]) // 2 + 1, (28 + hand[1]) // 2 + 1), hand, "teal", "sea")
    polearm(c, hand, d, back, front, trident_head)
    c.paste(["ll", "lm"], hand[0] - 1, hand[1] - 1)


tidecaller = Creature(
    layers=[
        ("back", {"rest": (sheet(S, S, lambda c: arm(c, (25, 28), (21, 34), (20, 41), "teal", "sea")), 0, 0)}),
        ("legs", part(
            rest=(sheet(S, S, _tide_hem), 0, 0),
            a=(sway(sheet(S, S, _tide_hem)[40:], -2), 0, 40),
            b=(sway(sheet(S, S, _tide_hem)[40:], 2), 0, 40),
        )),
        ("body", part(rest=(sheet(S, S, _tide_body), 0, 0))),
        ("head", part(rest=(sheet(S, S, _tide_head), 0, 0), hurt=(sheet(S, S, lambda c: _tide_head(c, True)), 0, 0))),
        ("arm", part(
            rest=(sheet(S, S, lambda c: _tide_arm(c, (48, 40), (0, -1), 22, 18)), 0, 0),
            up=(sheet(S, S, lambda c: _tide_arm(c, (48, 24), (0, -1), 22, 12)), 0, 0),
            fwd=(sheet(S, S, lambda c: _tide_arm(c, (48, 33), (1, 0), 7, 4)), 0, 0),
            follow=(sheet(S, S, lambda c: _tide_arm(c, (50, 40), (0.8, 0.6), 18, 5)), 0, 0),
        )),
        ("fx", part(
            smear=(sheet(S, S, lambda c: [sparkle_ring(c, 58, 32, r, "nmH", 12, r) for r in (4, 7)]), 0, 0),
            cast0=(sheet(S, S, lambda c: sparkle_ring(c, 48, 6, 6, "mn", 8)), 0, 0),
            cast1=(sheet(S, S, lambda c: sparkle_ring(c, 48, 6, 9, "nmH", 10, 0.4)), 0, 0),
            cast2=(sheet(S, S, lambda c: sparkle_ring(c, 48, 6, 12, "Hnm", 12, 0.8)), 0, 0),
        )),
    ],
    plan=biped, size=S, boss=True,
)


# ================================================================ THE FURNACE SAINT (B4)
def _furnace_body(c):
    for x0 in (13, 45):                                                                       # chimney stacks
        shade(c, {(x, y) for x in range(x0, x0 + 6) for y in range(11, 26)}, RAMP["slate"])
        c.line(x0, 11, x0 + 5, 11, "1")
    shade(c, ellipse(32, 33, 18, 15), RAMP["iron"])
    shade(c, {(x, y) for x in range(24, 41) for y in range(28, 42)}, RAMP["fire"], sep=True)  # open grate
    for x in range(25, 41, 3):
        c.line(x, 28, x, 41, "w")
    c.line(24, 34, 40, 34, "w")
    for k in range(10):                                                                       # rivets
        a = math.pi * (0.1 + 0.08 * k)
        put(c, 32 + math.cos(a + math.pi) * 16, 33 - math.sin(a) * 13, "2")


def _furnace_sparks(c, phase):
    for k, (x, y) in enumerate(((15, 8), (17, 5), (14, 2), (47, 8), (49, 4), (46, 1))):
        if (k + phase) % 2 == 0:
            put(c, x, y, "hd"[k % 2])


def _furnace_head(c, hurt=False):
    shade(c, ellipse(32, 15, 7, 6.5), RAMP["slate"])
    c.line(27, 15, 37, 15, "w")
    if not hurt:
        c.line(30, 15, 31, 15, "h"); c.line(34, 15, 35, 15, "h")
    c.paste(["..1..", ".121.", "12221"], 30, 6)                                               # helm spike
    _furnace_sparks(c, 0 if not hurt else 1)


def _furnace_legs(c, left=0, right=0, lift=0):
    for x0, dx, up in ((22, left, 0), (36, right, lift)):
        shade(c, {(x, y) for x in range(x0 + dx, x0 + dx + 7) for y in range(46, 61 - up)}, RAMP["iron"])
        shade(c, {(x, y) for x in range(x0 + dx - 1, x0 + dx + 9) for y in range(59 - up, 63 - up)}, RAMP["slate"], sep=False)


def _hammer_head(c, tip, d):
    tx, ty = tip
    px, py = -d[1], d[0]
    block = set()
    for k in range(-6, 7):
        for j in range(0, 8):
            block.add((round(tx + px * k + d[0] * j), round(ty + py * k + d[1] * j)))
    shade(c, block, RAMP["slate"], sep=False)
    put(c, tx + d[0] * 4, ty + d[1] * 4, "h"); put(c, tx + d[0] * 4 + px, ty + d[1] * 4 + py, "d")


def _furnace_arm(c, hand, d, reach=14):
    arm(c, (47, 27), ((47 + hand[0]) // 2 + 2, (27 + hand[1]) // 2 + 2), hand, "iron", "slate", 6)
    polearm(c, hand, d, 4, reach if d[0] < 0.9 else 6, _hammer_head)


furnace_saint = Creature(
    layers=[
        ("back", {"rest": (sheet(S, S, lambda c: arm(c, (17, 27), (12, 35), (12, 44), "iron", "slate", 6)), 0, 0)}),
        ("legs", part(
            rest=(sheet(S, S, _furnace_legs), 0, 0),
            a=(sheet(S, S, lambda c: _furnace_legs(c, -3, 3)), 0, 0),
            b=(sheet(S, S, lambda c: _furnace_legs(c, 0, 0, 3)), 0, 0),
        )),
        ("body", part(rest=(sheet(S, S, _furnace_body), 0, 0))),
        ("head", part(rest=(sheet(S, S, _furnace_head), 0, 0), hurt=(sheet(S, S, lambda c: _furnace_head(c, True)), 0, 0))),
        ("arm", part(
            rest=(sheet(S, S, lambda c: _furnace_arm(c, (52, 44), (0, 1))), 0, 0),
            up=(sheet(S, S, lambda c: _furnace_arm(c, (50, 18), (0, -1))), 0, 0),
            fwd=(sheet(S, S, lambda c: _furnace_arm(c, (42, 32), (1, 0))), 0, 0),
            follow=(sheet(S, S, lambda c: _furnace_arm(c, (50, 42), (0.6, 0.8))), 0, 0),
        )),
        ("fx", part(
            smear=(sheet(S, S, lambda c: [sparkle_ring(c, 56, 52, r, "hdg", 10, r) for r in (4, 7)]), 0, 0),
            cast0=(sheet(S, S, lambda c: sparkle_ring(c, 32, 34, 20, "hd", 10)), 0, 0),
            cast1=(sheet(S, S, lambda c: sparkle_ring(c, 32, 34, 23, "dhg", 12, 0.3)), 0, 0),
            cast2=(sheet(S, S, lambda c: sparkle_ring(c, 32, 34, 26, "ghd", 14, 0.6)), 0, 0),
        )),
    ],
    plan=biped, size=S, boss=True,
)


# ================================================================ THE PALE MATRIARCH (B5)
def _matriarch_hair(c):
    shade(c, poly([(24, 10), (40, 10), (44, 36), (38, 42), (22, 42), (20, 30)]), RAMP["sea"], sep=False)
    for x0 in (23, 27, 39, 42):
        c.line(x0, 14, x0 - 2, 40, "j")


def _matriarch_head(c, hurt=False):
    shade(c, ellipse(33, 17, 8, 8.5), ("3", "W", "4", "H"))
    if hurt:
        c.line(28, 17, 31, 17, "1"); c.line(35, 17, 38, 17, "1")
    else:
        c.paste(["GGG", "GmG", ".G."], 28, 15); c.paste(["GGG", "GmG", ".G."], 35, 15)
    put(c, 33, 21, "2")
    c.paste(["3W3W3W3", ".2.2.2."], 30, 23)                                                  # teeth
    for x, h in ((25, 5), (28, 8), (31, 10), (34, 11), (37, 9), (40, 6)):                     # icicle crown
        c.line(x, 9, x, 9 - h, "n"); c.line(x + 1, 9, x + 1, 9 - h + 2, "m")
        put(c, x, 9 - h, "H")
    c.line(24, 9, 42, 9, "l")


def _matriarch_body(c):
    shade(c, poly([(25, 26), (41, 26), (43, 42), (23, 42)]), RAMP["pale"])
    for y in (29, 32, 35):
        c.line(28, y, 38, y, "3")                                                             # rib bodice
    c.line(33, 27, 33, 40, "W")
    shade(c, ellipse(33, 27, 11, 2.5), ("3", "W", "4", "H"), sep=False)                     # frost fur mantle
    c.line(23, 40, 43, 40, "l"); c.line(23, 41, 43, 41, "j")


def _matriarch_hem(c):
    shade(c, poly([(23, 41), (43, 41), (50, 61), (16, 61)]), RAMP["ice"], sep=False)
    for x in range(16, 51, 3):
        c.paste(["H", "n"], x, 60)
    c.line(29, 44, 25, 60, "j"); c.line(37, 44, 41, 60, "j")


def _crystal(c, tip, d):
    tx, ty = tip
    shade(c, poly([(tx, ty - 7), (tx + 4, ty - 2), (tx, ty + 3), (tx - 4, ty - 2)]), RAMP["glass"], sep=False)
    put(c, tx - 1, ty - 4, "H")


def _matriarch_arm(c, hand, d=(0, -1), back=20, front=14):
    arm(c, (42, 30), ((42 + hand[0]) // 2 + 1, (30 + hand[1]) // 2 + 1), hand, "pale", "pale", 3)
    polearm(c, hand, d, back, front, _crystal)


pale_matriarch = Creature(
    layers=[
        ("back", {"rest": (sheet(S, S, _matriarch_hair), 0, 0)}),
        ("legs", part(
            rest=(sheet(S, S, _matriarch_hem), 0, 0),
            a=(sway(sheet(S, S, _matriarch_hem)[41:], -2), 0, 41),
            b=(sway(sheet(S, S, _matriarch_hem)[41:], 2), 0, 41),
        )),
        ("body", part(rest=(sheet(S, S, _matriarch_body), 0, 0))),
        ("head", part(rest=(sheet(S, S, _matriarch_head), 0, 0), hurt=(sheet(S, S, lambda c: _matriarch_head(c, True)), 0, 0))),
        ("arm", part(
            rest=(sheet(S, S, lambda c: _matriarch_arm(c, (48, 40))), 0, 0),
            up=(sheet(S, S, lambda c: _matriarch_arm(c, (49, 22), (0, -1), 20, 10)), 0, 0),
            fwd=(sheet(S, S, lambda c: _matriarch_arm(c, (48, 33), (1, 0), 6, 5)), 0, 0),
            follow=(sheet(S, S, lambda c: _matriarch_arm(c, (49, 40), (0.7, 0.7), 14, 6)), 0, 0),
        )),
        ("fx", part(
            smear=(sheet(S, S, lambda c: [sparkle_ring(c, 58, 33, r, "Hnm", 12, r) for r in (4, 7)]), 0, 0),
            cast0=(sheet(S, S, lambda c: sparkle_ring(c, 49, 8, 6, "Hn", 8)), 0, 0),
            cast1=(sheet(S, S, lambda c: sparkle_ring(c, 49, 8, 9, "nHm", 10, 0.4)), 0, 0),
            cast2=(sheet(S, S, lambda c: sparkle_ring(c, 49, 8, 12, "mnH", 12, 0.8)), 0, 0),
        )),
    ],
    plan=biped, size=S, boss=True,
)


# ================================================================ THE VERDANT HIEROPHANT (B6)
def _ent_head(c, hurt=False):
    shade(c, ellipse(33, 20, 9, 9), RAMP["bark"])
    for (x, y, rx, ry) in ((22, 11, 9, 7), (43, 11, 9, 7), (33, 7, 11, 7), (28, 4, 6, 4), (40, 5, 6, 4)):   # leaf canopy
        shade(c, ellipse(x, y, rx, ry), RAMP["moss"], sep=False)
    for x, y in ((19, 9), (27, 3), (36, 6), (45, 10), (31, 11), (24, 14), (42, 14)):
        c.paste(["v.", "Vv"], x, y)
    if hurt:
        c.line(29, 20, 31, 20, "5"); c.line(35, 20, 37, 20, "5")
    else:
        c.paste(["55", "5v"], 29, 19); c.paste(["55", "5v"], 35, 19)
    c.paste(["5555", ".55."], 31, 24)
    c.line(26, 16, 27, 25, "5"); c.line(40, 16, 39, 25, "5")


def _ent_body(c):
    shade(c, poly([(23, 27), (43, 27), (46, 50), (20, 50)]), RAMP["bark"])
    for x0 in (25, 30, 38, 42):
        c.line(x0, 30, x0 + (1 if x0 > 33 else -1), 48, "5")                                  # bark grain
    for dx, dy in ((0, -4), (4, 0), (0, 4), (-4, 0)):                                          # crimson flower heart
        shade(c, ellipse(33 + dx, 37 + dy, 3, 3), RAMP["crimson"], sep=False)
    shade(c, ellipse(33, 37, 2, 2), RAMP["fire"], sep=False)
    c.line(21, 44, 45, 41, "t"); c.line(21, 45, 45, 42, "s")                                   # vine
    for x in (25, 33, 41):
        put(c, x, 43 - (x - 21) // 8, "u")


def _ent_roots(c, lean=0):
    for x0, x1 in ((24, 15), (28, 24), (33, 33), (38, 42), (42, 51)):
        shade(c, thick(x0, 48, x1 + lean, 61, 3.4), RAMP["bark"], sep=False)


def _ent_arm(c, hand):
    sx, sy = 43, 30
    ex, ey = (sx + hand[0]) // 2 + 2, (sy + hand[1]) // 2
    shade(c, thick(sx, sy, ex, ey, 5), RAMP["bark"], sep=False)
    shade(c, thick(ex, ey, *hand, 4), RAMP["bark"], sep=False)
    hx, hy = hand
    for dx, dy in ((4, -3), (5, 0), (4, 3)):                                                    # twig fingers
        c.line(hx, hy, hx + dx, hy + dy, "6")
        put(c, hx + dx, hy + dy - 1, "u")


verdant_hierophant = Creature(
    layers=[
        ("back", {"rest": (sheet(S, S, lambda c: shade(c, thick(23, 30, 15, 45, 5), RAMP["bark"], sep=False)), 0, 0)}),
        ("legs", part(
            rest=(sheet(S, S, _ent_roots), 0, 0),
            a=(sheet(S, S, lambda c: _ent_roots(c, -2)), 0, 0),
            b=(sheet(S, S, lambda c: _ent_roots(c, 2)), 0, 0),
        )),
        ("body", part(rest=(sheet(S, S, _ent_body), 0, 0))),
        ("head", part(rest=(sheet(S, S, _ent_head), 0, 0), hurt=(sheet(S, S, lambda c: _ent_head(c, True)), 0, 0))),
        ("arm", part(
            rest=(sheet(S, S, lambda c: _ent_arm(c, (50, 46))), 0, 0),
            up=(sheet(S, S, lambda c: _ent_arm(c, (51, 16))), 0, 0),
            fwd=(sheet(S, S, lambda c: _ent_arm(c, (57, 32))), 0, 0),
            follow=(sheet(S, S, lambda c: _ent_arm(c, (55, 46))), 0, 0),
        )),
        ("fx", part(
            smear=(sheet(S, S, lambda c: [sparkle_ring(c, 59, 32, r, "vVu", 10, r) for r in (4, 7)]), 0, 0),
            cast0=(sheet(S, S, lambda c: sparkle_ring(c, 52, 13, 6, "gC", 8)), 0, 0),
            cast1=(sheet(S, S, lambda c: sparkle_ring(c, 52, 13, 9, "Cgv", 10, 0.4)), 0, 0),
            cast2=(sheet(S, S, lambda c: sparkle_ring(c, 52, 13, 12, "vgC", 12, 0.8)), 0, 0),
        )),
    ],
    plan=biped, size=S, boss=True,
)

CREATURES = {"mother_morel": mother_morel, "tidecaller": tidecaller, "furnace_saint": furnace_saint,
             "pale_matriarch": pale_matriarch, "verdant_hierophant": verdant_hierophant}
