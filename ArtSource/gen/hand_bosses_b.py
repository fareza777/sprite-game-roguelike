"""Hand-drawn bosses, B7-B10: the Horologist, the Dune Pharaoh, the Mirrored Choir, the Cantor.
Every part is drawn on a full 64x64 sheet in canvas coordinates; feet rest on row 62, facing right."""
import math

from hand_boss import RAMP, ellipse, poly, put, sheet, shade, thick
from hand_bosses_a import arm, polearm, sparkle_ring
from hand_core import Creature, biped, part, sway

S = 64


def casts(cx, cy, chars):
    """The three cast-glow frames around a raised hand."""
    return dict(
        cast0=(sheet(S, S, lambda c: sparkle_ring(c, cx, cy, 6, chars[:2], 8)), 0, 0),
        cast1=(sheet(S, S, lambda c: sparkle_ring(c, cx, cy, 9, chars, 10, 0.4)), 0, 0),
        cast2=(sheet(S, S, lambda c: sparkle_ring(c, cx, cy, 12, chars[::-1], 12, 0.8)), 0, 0),
    )


def smear(cx, cy, chars):
    return (sheet(S, S, lambda c: [sparkle_ring(c, cx, cy, r, chars, 12, r) for r in (4, 7)]), 0, 0)


def orb_head(ramp, glint="H"):
    def draw(c, tip, d):
        tx, ty = tip
        shade(c, ellipse(tx, ty - 3, 3.2, 3.2), RAMP[ramp], sep=False)
        put(c, tx - 1, ty - 4, glint)
    return draw


# ================================================================ THE HOROLOGIST (B7)
def _gear(c, cx, cy, r, phase):
    teeth = set()
    for k in range(12):
        a = phase + k * math.pi / 6
        for j in range(3):
            teeth |= ellipse(cx + math.cos(a) * (r + 1.5), cy + math.sin(a) * (r + 1.5), 1.2, 1.2)
    shade(c, teeth | ellipse(cx, cy, r, r), RAMP["brass"], sep=False)
    shade(c, ellipse(cx, cy, r * 0.45, r * 0.45), RAMP["slate"], sep=False)


def _horo_head(c, hurt=False):
    shade(c, ellipse(33, 15, 10, 10), RAMP["gold"])
    shade(c, ellipse(33, 15, 7.5, 7.5), RAMP["linen"], sep=False)
    for k in range(12):
        a = k * math.pi / 6
        put(c, 33 + math.cos(a) * 6, 15 + math.sin(a) * 6, "6")
    if hurt:
        c.line(33, 15, 37, 19, "f"); c.line(33, 15, 29, 18, "f")
    else:
        c.line(33, 15, 33, 10, "k"); c.line(33, 15, 37, 16, "k")
    put(c, 33, 15, "f")
    c.paste(["..5..", ".5a5.", "5aba5"], 31, 3)                                                # winding crown


def _horo_body(c):
    shade(c, poly([(25, 25), (41, 25), (44, 44), (22, 44)]), RAMP["slate"])
    shade(c, {(x, y) for x in range(31, 36) for y in range(26, 44)}, RAMP["brass"], sep=False)  # brass panel
    for y in (29, 34, 39):
        put(c, 33, y, "d")
    shade(c, ellipse(33, 26, 10, 2.5), RAMP["gold"], sep=False)                                 # collar
    for gx, gy in ((26, 33), (39, 38)):
        _gear(c, gx, gy, 2, 0.2)


def _horo_hem(c):
    shade(c, poly([(22, 43), (44, 43), (49, 61), (17, 61)]), RAMP["slate"], sep=False)
    c.line(17, 60, 49, 60, "b"); c.line(18, 59, 48, 59, "c")
    shade(c, {(x, y) for x in range(31, 36) for y in range(44, 59)}, RAMP["brass"], sep=False)


def _pendulum(c, tip, d):
    tx, ty = tip
    c.line(tx, ty, tx, ty - 3, "a")
    shade(c, ellipse(tx, ty - 6, 3.5, 3.5), RAMP["gold"], sep=False)
    put(c, tx - 1, ty - 7, "H")


def _horo_arm(c, hand, d=(0, -1), back=20, front=14):
    arm(c, (42, 29), ((42 + hand[0]) // 2 + 1, (29 + hand[1]) // 2 + 1), hand, "slate", "brass", 3)
    polearm(c, hand, d, back, front, _pendulum)


horologist = Creature(
    layers=[
        ("back", part(rest=(sheet(S, S, lambda c: _gear(c, 22, 22, 10, 0.0)), 0, 0),
                      up=(sheet(S, S, lambda c: _gear(c, 22, 22, 10, 0.26)), 0, 0))),
        ("legs", part(
            rest=(sheet(S, S, _horo_hem), 0, 0),
            a=(sway(sheet(S, S, _horo_hem)[43:], -2), 0, 43),
            b=(sway(sheet(S, S, _horo_hem)[43:], 2), 0, 43),
        )),
        ("body", part(rest=(sheet(S, S, _horo_body), 0, 0))),
        ("head", part(rest=(sheet(S, S, _horo_head), 0, 0), hurt=(sheet(S, S, lambda c: _horo_head(c, True)), 0, 0))),
        ("arm", part(
            rest=(sheet(S, S, lambda c: _horo_arm(c, (48, 40))), 0, 0),
            up=(sheet(S, S, lambda c: _horo_arm(c, (49, 22), (0, -1), 20, 10)), 0, 0),
            fwd=(sheet(S, S, lambda c: _horo_arm(c, (47, 33), (1, 0), 6, 6)), 0, 0),
            follow=(sheet(S, S, lambda c: _horo_arm(c, (49, 40), (0.7, 0.7), 12, 6)), 0, 0),
        )),
        ("fx", part(smear=smear(58, 32, "dcH"), **casts(49, 8, "dcb"))),
    ],
    plan=biped, size=S, boss=True,
)


# ================================================================ THE DUNE PHARAOH (B8)
def _pharaoh_head(c, hurt=False):
    nemes = poly([(22, 8), (44, 8), (47, 30), (19, 30)])
    shade(c, nemes, RAMP["gold"])
    for y in range(11, 30, 3):
        c.line(20, y, 46, y, "j")                                                               # striped headcloth
    shade(c, ellipse(33, 9, 11, 5), RAMP["gold"], sep=False)
    shade(c, ellipse(34, 18, 7, 8), RAMP["gold"])                                               # golden mask
    if hurt:
        c.line(30, 17, 32, 17, "5"); c.line(36, 17, 38, 17, "5")
    else:
        c.paste(["iii", "ilm"], 30, 16); c.paste(["iii", "ilm"], 36, 16)
    c.line(33, 22, 35, 22, "a")
    shade(c, {(x, y) for x in range(33, 36) for y in range(25, 32)}, RAMP["blue"], sep=False)  # braided beard
    for y in (26, 28, 30):
        c.line(33, y, 35, y, "c")


def _pharaoh_body(c):
    shade(c, poly([(23, 29), (43, 29), (45, 44), (21, 44)]), RAMP["blue"])
    for r, ch in ((11, "c"), (9, "j"), (7, "f"), (5, "c")):                                     # broad collar
        shade(c, {p for p in ellipse(33, 29, r, r * 0.55) if p[1] >= 29}, RAMP["gold"] if ch == "c" else RAMP["blue" if ch == "j" else "ember"], sep=False)
    c.line(21, 42, 45, 42, "c"); c.line(21, 43, 45, 43, "a")


def _pharaoh_legs(c, left=0, right=0, lift=0):
    shade(c, poly([(21, 43), (45, 43), (47, 50), (19, 50)]), RAMP["linen"], sep=False)          # kilt
    for x in range(22, 46, 3):
        c.line(x, 44, x - 1, 50, "x")
    for x0, dx, up in ((24, left, 0), (36, right, lift)):
        shade(c, {(x, y) for x in range(x0 + dx, x0 + dx + 6) for y in range(50, 60 - up)}, RAMP["sand"])
        shade(c, {(x, y) for x in range(x0 + dx, x0 + dx + 6) for y in range(53, 58 - up)}, RAMP["gold"], sep=False)
        shade(c, {(x, y) for x in range(x0 + dx - 1, x0 + dx + 8) for y in range(60 - up, 63 - up)}, RAMP["gold"], sep=False)


def _pharaoh_arm(c, hand, d=(0, -1), back=10, front=14):
    arm(c, (43, 32), ((43 + hand[0]) // 2 + 1, (32 + hand[1]) // 2 + 1), hand, "blue", "sand", 4)
    c.paste(["cc", "aa"], hand[0] - 3, hand[1] - 4)                                             # gold armband
    polearm(c, hand, d, back, front, orb_head("gold", "H"))


dune_pharaoh = Creature(
    layers=[
        ("back", {"rest": (sheet(S, S, lambda c: arm(c, (23, 32), (19, 38), (18, 45), "blue", "sand")), 0, 0)}),
        ("legs", part(
            rest=(sheet(S, S, _pharaoh_legs), 0, 0),
            a=(sheet(S, S, lambda c: _pharaoh_legs(c, -3, 3)), 0, 0),
            b=(sheet(S, S, lambda c: _pharaoh_legs(c, 0, 0, 3)), 0, 0),
        )),
        ("body", part(rest=(sheet(S, S, _pharaoh_body), 0, 0))),
        ("head", part(rest=(sheet(S, S, _pharaoh_head), 0, 0), hurt=(sheet(S, S, lambda c: _pharaoh_head(c, True)), 0, 0))),
        ("arm", part(
            rest=(sheet(S, S, lambda c: _pharaoh_arm(c, (50, 44), (0, -1), 8, 16)), 0, 0),
            up=(sheet(S, S, lambda c: _pharaoh_arm(c, (50, 22), (0, -1), 8, 10)), 0, 0),
            fwd=(sheet(S, S, lambda c: _pharaoh_arm(c, (47, 34), (1, 0), 4, 7)), 0, 0),
            follow=(sheet(S, S, lambda c: _pharaoh_arm(c, (49, 42), (0.6, 0.8), 4, 9)), 0, 0),
        )),
        ("fx", part(smear=smear(58, 35, "dcy"), **casts(50, 8, "dcy"))),
    ],
    plan=biped, size=S, boss=True,
)


# ================================================================ THE MIRRORED CHOIR (B9)
def _singer(c, cx, top, k, ramp, mouth=1, wings=0.0, hurt=False):
    """One crystal singer. k scales it; wings 0 rest, 1 raised, -1 swept forward."""
    h = 54 * k
    feet = 62
    body = poly([(cx, top + 10 * k), (cx + 9 * k, top + 26 * k), (cx + 6 * k, feet), (cx - 6 * k, feet), (cx - 9 * k, top + 26 * k)])
    for side in (-1, 1):
        lift = 10 * k * wings if wings > 0 else 0
        fwd = 8 * k if (wings < 0 and side > 0) else 0
        tip = (cx + side * (17 * k) + fwd, top + (16 - 12 * max(wings, 0)) * k - lift * 0.2)
        shade(c, poly([(cx + side * 6 * k, top + 18 * k), tip, (cx + side * 12 * k, top + 28 * k)]), RAMP[ramp], sep=False)
    shade(c, body, RAMP[ramp])
    c.line(round(cx), round(top + 12 * k), round(cx), feet - 2, RAMP[ramp][3])
    for j in (0.35, 0.6):                                                                       # facets
        y = round(top + 10 * k + (feet - top - 10 * k) * j)
        c.line(round(cx - 6 * k), y + 3, round(cx), y, RAMP[ramp][1])
        c.line(round(cx), y, round(cx + 6 * k), y + 3, RAMP[ramp][2])
    shade(c, ellipse(cx, top + 5 * k, 5.5 * k, 5.5 * k), RAMP[ramp], sep=False)
    if hurt:
        c.line(round(cx - 3 * k), round(top + 5 * k), round(cx - k), round(top + 5 * k), "G")
        c.line(round(cx + k), round(top + 5 * k), round(cx + 3 * k), round(top + 5 * k), "G")
    else:
        put(c, cx - 2 * k, top + 4 * k, "G"); put(c, cx + 2 * k, top + 4 * k, "G")
    for j in range(mouth):
        c.line(round(cx - k), round(top + 7 * k) + j, round(cx + k), round(top + 7 * k) + j, "G")
    return h


def _choir_sheet(sway_, wings=0.0, mouth=1, hurt=False, who="all"):
    def draw(c):
        if who in ("all", "left"):
            _singer(c, 12, 22 + sway_[0], 0.7, "violet", mouth, wings)
        if who in ("all", "right"):
            _singer(c, 52, 22 + sway_[1], 0.7, "violet", mouth, wings)
        if who in ("all", "mid"):
            _singer(c, 32, 6 + sway_[2], 1.0, "glass", mouth, wings, hurt)
    return draw


def _choir_part(who):
    def st(sw, **kw):
        return (sheet(S, S, _choir_sheet(sw, who=who, **kw)), 0, 0)
    return {
        "rest": st((0, 0, 0)), "a": st((1, 0, 0)), "b": st((0, 1, 1)),
        "up": st((0, 0, 0), wings=1.0, mouth=2), "fwd": st((0, 0, 0), wings=-1.0, mouth=3),
        "hurt": st((1, 1, 1), hurt=True), "cast": st((0, 0, -1), wings=1.0, mouth=3),
    }


def choir_plan(tag, i):
    if tag == "idle":
        return {"left": ("a" if i else "rest", 0, 0), "right": ("b" if i else "rest", 0, 0), "mid": ("rest", 0, i)}
    if tag == "run":
        return {"all": (1, 0), "left": (("a", "rest")[i % 2], 0, 0), "right": (("rest", "b")[i % 2], 0, 0), "mid": ("rest", 0, i % 2)}
    if tag == "windup":
        return {"all": (-1 - i, 0), "left": ("up", 0, 0), "right": ("up", 0, 0), "mid": ("up", 0, 0)}
    if tag == "attack":
        return {"all": ((2, 2, 1)[i], 0), "left": ("fwd", 0, 0), "right": ("fwd", 0, 0), "mid": ("fwd", 0, 0),
                "fx": (("smear", "rest", "rest")[i], 0, 0)}
    if tag == "cast":
        return {"left": ("cast", 0, 0), "right": ("cast", 0, 0), "mid": ("cast", 0, 0), "fx": ("cast%d" % i, 0, 0)}
    if tag == "hurt":
        return {"all": (-2, 0), "left": ("hurt", 0, 0), "right": ("hurt", 0, 0), "mid": ("hurt", 0, 0)}
    return {}


mirrored_choir = Creature(
    layers=[
        ("left", _choir_part("left")),
        ("right", _choir_part("right")),
        ("mid", _choir_part("mid")),
        ("fx", part(smear=(sheet(S, S, lambda c: [sparkle_ring(c, 40, 12, r, "nHq", 14, r) for r in (8, 12)]), 0, 0),
                    **casts(32, 11, "Hnr"))),
    ],
    plan=choir_plan, size=S, boss=True,
)


# ================================================================ THE CANTOR (B10)
def _cantor_cape(c, flare=0):
    pts = [(22, 24), (42, 24), (48 + flare, 50), (50 + flare, 61), (45, 58), (40, 61), (34, 57), (28, 61), (22, 58), (16 - flare, 61), (17 - flare, 50)]
    shade(c, poly(pts), RAMP["crimson"], sep=False)


def _cantor_head(c, hurt=False):
    for side in (-1, 1):                                                                         # horned crown
        x0 = 33 + side * 6
        shade(c, thick(x0, 11, x0 + side * 5, 4, 4.2), RAMP["iron"], sep=False)
        shade(c, thick(x0 + side * 5, 4, x0 + side * 9, 3, 2.6), RAMP["iron"], sep=False)
        put(c, x0 + side * 10, 2, "1")
    shade(c, ellipse(33, 16, 10, 10), RAMP["crimson"])                                           # hood
    shade(c, ellipse(35, 18, 6, 6.5), ("w", "A", "A", "B"), sep=False)                           # void face
    if hurt:
        c.line(32, 18, 34, 18, "a"); c.line(37, 18, 39, 18, "a")
    else:
        c.paste(["dH", "cd"], 32, 17); c.paste(["dH", "cd"], 37, 17)
    shade(c, ellipse(33, 7, 2, 2), RAMP["fire"], sep=False)                                       # ember gem
    put(c, 32, 6, "H")


def _cantor_body(c):
    shade(c, poly([(25, 26), (41, 26), (44, 46), (22, 46)]), RAMP["night"])
    c.line(33, 28, 33, 45, "C")
    for y in (30, 36, 42):
        c.paste(["cdc"], 32, y)                                                                  # gold clasps
    shade(c, ellipse(33, 27, 11, 2.6), RAMP["crimson"], sep=False)


def _cantor_hem(c):
    shade(c, poly([(22, 45), (44, 45), (49, 61), (17, 61)]), RAMP["dusk"], sep=False)
    for x in range(17, 50, 4):
        c.paste(["C", "f"], x, 60)
    c.line(29, 47, 26, 60, "w"); c.line(37, 47, 40, 60, "w")


def _cantor_arm(c, hand, d=(0, -1), back=20, front=14):
    arm(c, (42, 30), ((42 + hand[0]) // 2 + 1, (30 + hand[1]) // 2 + 1), hand, "night", "crimson", 4)
    polearm(c, hand, d, back, front, orb_head("fire", "H"))


the_cantor = Creature(
    layers=[
        ("back", part(rest=(sheet(S, S, _cantor_cape), 0, 0), up=(sheet(S, S, lambda c: _cantor_cape(c, 3)), 0, 0))),
        ("legs", part(
            rest=(sheet(S, S, _cantor_hem), 0, 0),
            a=(sway(sheet(S, S, _cantor_hem)[45:], -2), 0, 45),
            b=(sway(sheet(S, S, _cantor_hem)[45:], 2), 0, 45),
        )),
        ("body", part(rest=(sheet(S, S, _cantor_body), 0, 0))),
        ("head", part(rest=(sheet(S, S, _cantor_head), 0, 0), hurt=(sheet(S, S, lambda c: _cantor_head(c, True)), 0, 0))),
        ("arm", part(
            rest=(sheet(S, S, lambda c: _cantor_arm(c, (49, 41))), 0, 0),
            up=(sheet(S, S, lambda c: _cantor_arm(c, (50, 22), (0, -1), 20, 10)), 0, 0),
            fwd=(sheet(S, S, lambda c: _cantor_arm(c, (47, 33), (1, 0), 6, 6)), 0, 0),
            follow=(sheet(S, S, lambda c: _cantor_arm(c, (49, 40), (0.7, 0.7), 12, 6)), 0, 0),
        )),
        ("fx", part(smear=smear(58, 31, "gdh"), **casts(50, 8, "gdC"))),
    ],
    plan=biped, size=S, boss=True,
)

CREATURES = {"horologist": horologist, "dune_pharaoh": dune_pharaoh, "mirrored_choir": mirrored_choir, "the_cantor": the_cantor}
