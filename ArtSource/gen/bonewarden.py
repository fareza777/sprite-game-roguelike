"""Bone Warden boss, 64x64, facing right. Pose rig built from shaded shapes; prints JSON {legend, frames}."""
import json, math, sys
sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
from compose import Canvas

L = {
    "k": "#10141f",
    # bone
    "d": "#577277", "b": "#819796", "W": "#c7cfcc", "H": "#ebede9",
    # iron
    "a": "#151d28", "A": "#202e37", "m": "#394a50", "M": "#577277", "n": "#819796",
    # gold trim
    "o": "#be772b", "G": "#de9e41", "y": "#e8c170",
    # cloth
    "Q": "#411d31", "r": "#752438", "R": "#a53030", "E": "#cf573c",
    # ember eyes
    "O": "#da863e",
    # spectral cast
    "v": "#402751", "u": "#7a367b", "U": "#a23e8c", "Z": "#c65197", "z": "#df84a5",
    # blade steel
    "S": "#a8b5b2",
}
BONE = ("d", "b", "W", "H")
IRON = ("a", "A", "m", "M")
IRON_FAR = ("a", "a", "A", "m")
FACE = BONE
GOLD = ("o", "o", "G", "y")
CLOAK = ("Q", "Q", "r", "R")
TABARD = ("Q", "r", "R", "E")
BLADE = ("m", "M", "S", "H")


# ---------- shape sets ----------
def ellipse(cx, cy, rx, ry):
    return {(x, y) for y in range(int(cy - ry) - 1, int(cy + ry) + 2)
            for x in range(int(cx - rx) - 1, int(cx + rx) + 2)
            if ((x - cx) / max(rx, .5)) ** 2 + ((y - cy) / max(ry, .5)) ** 2 <= 1.0}


def poly(pts):
    xs = [p[0] for p in pts]; ys = [p[1] for p in pts]
    out = set()
    for y in range(int(min(ys)), int(max(ys)) + 1):
        for x in range(int(min(xs)), int(max(xs)) + 1):
            px, py = x + .5, y + .5
            inside = False
            j = len(pts) - 1
            for i in range(len(pts)):
                xi, yi = pts[i]; xj, yj = pts[j]
                if (yi > py) != (yj > py) and px < (xj - xi) * (py - yi) / (yj - yi) + xi:
                    inside = not inside
                j = i
            if inside:
                out.add((x, y))
    return out


def thick(x0, y0, x1, y1, w):
    """Capsule between two points with width w."""
    out = set()
    n = max(abs(x1 - x0), abs(y1 - y0), 1)
    r = w / 2
    for i in range(int(n * 2) + 1):
        t = i / (n * 2)
        cx, cy = x0 + (x1 - x0) * t, y0 + (y1 - y0) * t
        for y in range(int(cy - r) - 1, int(cy + r) + 2):
            for x in range(int(cx - r) - 1, int(cx + r) + 2):
                if (x + .5 - cx - .5) ** 2 + (y + .5 - cy - .5) ** 2 <= r * r:
                    out.add((x, y))
    return out


def rect(x, y, w, h):
    return {(i, j) for i in range(x, x + w) for j in range(y, y + h)}


# ---------- painting ----------
def put(c, x, y, ch):
    if 0 <= x < c.w and 0 <= y < c.h:
        c.g[y][x] = ch


def fill(c, pts, ch):
    for x, y in pts:
        put(c, x, y, ch)


def shade(c, pts, ramp, sep=True):
    """Rim-lit fill: top-left rim light, bottom-right rim dark, inner lower-right core shadow.
    sep draws a dark separation line where the part overlaps something drawn earlier."""
    deep, dark, base, light = ramp
    if sep:
        for x, y in pts:
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                p = (x + dx, y + dy)
                if p not in pts and 0 <= p[0] < c.w and 0 <= p[1] < c.h and c.g[p[1]][p[0]] != '.':
                    put(c, p[0], p[1], 'k')
    for x, y in pts:
        if (x + 1, y) not in pts or (x, y + 1) not in pts:
            ch = deep
        elif (x + 2, y + 1) not in pts or (x + 1, y + 2) not in pts:
            ch = dark
        elif (x - 1, y) not in pts or (x, y - 1) not in pts:
            ch = light
        else:
            ch = base
        put(c, x, y, ch)


def rotate(px, py, ox, oy, ang):
    s, co = math.sin(ang), math.cos(ang)
    dx, dy = px - ox, py - oy
    return (ox + dx * co - dy * s, oy + dx * s + dy * co)


# ---------- parts ----------
def leg(c, hip, foot, near):
    hx, hy = hip; fx, fy = foot
    kx, ky = (hx + fx) / 2 + 2, (hy + fy) / 2 - 1
    ramp_i = IRON if near else IRON_FAR
    ramp_b = BONE if near else ("d", "d", "b", "W")
    shade(c, thick(hx, hy, kx, ky, 3), ramp_b)
    shade(c, poly([(kx - 3, ky), (kx + 3, ky), (fx + 2, fy - 2), (fx - 3, fy - 2)]), ramp_i)
    shade(c, ellipse(kx, ky, 3, 2.2), ramp_i)
    shade(c, poly([(fx - 4, fy - 3), (fx + 2, fy - 3), (fx + 6, fy), (fx + 6, fy + 1), (fx - 4, fy + 1)]), ramp_i)


def kneel_leg(c, hip):
    hx, hy = hip
    kx, ky = hx + 6, 57
    shade(c, thick(hx, hy, kx, ky, 3), BONE)
    shade(c, ellipse(kx, ky, 3, 2.2), IRON)
    shade(c, poly([(kx - 1, ky - 1), (kx + 1, ky + 2), (hx - 6, 60), (hx - 7, 58)]), IRON)


def cloak(c, bx, by, sway):
    top = by + 20
    pts = [(bx + 22, top), (bx + 31, top), (bx + 30, by + 44), (bx + 26 + sway, by + 55),
           (bx + 24 + sway, by + 53), (bx + 22 + sway, by + 56), (bx + 19 + sway, by + 52),
           (bx + 16 + sway, by + 55), (bx + 15 + sway, by + 46)]
    shade(c, poly(pts), CLOAK, sep=False)


def torso(c, bx, by):
    # rib cage cavity
    fill(c, poly([(bx + 22, by + 25), (bx + 39, by + 25), (bx + 35, by + 38), (bx + 26, by + 38)]), 'k')
    for y, half in ((26, 7), (29, 7), (32, 6), (35, 4)):
        shade(c, thick(bx + 30 - half, by + y + 1, bx + 30 + half, by + y, 2), BONE, sep=False)
        put(c, bx + 30 - half - 1, by + y + 2, 'b'); put(c, bx + 30 + half + 1, by + y + 1, 'b')
    shade(c, rect(bx + 29, by + 24, 3, 14), BONE, sep=False)            # sternum / spine
    # gorget
    shade(c, poly([(bx + 23, by + 21), (bx + 38, by + 21), (bx + 36, by + 25), (bx + 25, by + 25)]), IRON)
    # belt + buckle
    shade(c, rect(bx + 22, by + 38, 17, 4), IRON)
    shade(c, rect(bx + 29, by + 38, 4, 4), GOLD)
    # tabard
    t = [(bx + 26, by + 42), (bx + 35, by + 42), (bx + 35, by + 52), (bx + 33, by + 50),
         (bx + 31, by + 53), (bx + 29, by + 50), (bx + 26, by + 52)]
    shade(c, poly(t), TABARD)
    put(c, bx + 30, by + 45, 'G'); put(c, bx + 31, by + 45, 'G'); put(c, bx + 30, by + 46, 'o')


def head(c, bx, by, hurt=False, glow=1):
    hx, hy = bx + (29 if hurt else 31), by + (13 if hurt else 12)
    # back horn
    shade(c, thick(hx - 4, hy - 3, hx - 8, hy - 8, 3) | thick(hx - 8, hy - 8, hx - 11, hy - 10, 2), ("d", "d", "b", "W"))
    # skull face below the brim
    shade(c, ellipse(hx + 1, hy + 4, 5, 4.6), FACE)
    # helm dome + cheek guard
    shade(c, {p for p in ellipse(hx, hy - 1, 6.5, 5.5) if p[1] <= hy}, IRON)
    shade(c, poly([(hx - 6, hy), (hx - 2, hy), (hx - 3, hy + 6), (hx - 6, hy + 4)]), IRON)
    fill(c, rect(hx - 6, hy, 13, 1), 'o')
    fill(c, rect(hx - 4, hy, 5, 1), 'G')
    put(c, hx - 3, hy - 3, 'n'); put(c, hx - 2, hy - 4, 'n')
    # front horn
    shade(c, thick(hx + 4, hy - 4, hx + 7, hy - 9, 3) | thick(hx + 7, hy - 9, hx + 6, hy - 13, 2), BONE)
    # crest spike
    shade(c, poly([(hx - 1, hy - 6), (hx + 2, hy - 6), (hx, hy - 10)]), GOLD)
    # eye sockets with ember glow
    for ex in (hx - 1, hx + 3):
        fill(c, rect(ex, hy + 2, 3, 2), 'k')
        if glow:
            put(c, ex + 2, hy + 2, 'O')
            put(c, ex, hy + 2, 'O'); put(c, ex + 1, hy + 2, 'y')
            put(c, ex + 1, hy + 3, 'E' if glow > 1 else 'O')
    put(c, hx + 3, hy + 5, 'k')  # nose
    for i in range(6):           # teeth
        put(c, hx - 1 + i, hy + 7, 'W' if i % 2 == 0 else 'k')
    fill(c, rect(hx, hy + 8, 5, 1), 'd')


def pauldron(c, cx, cy, near):
    ramp = IRON if near else IRON_FAR
    shade(c, ellipse(cx, cy, 6 if near else 5, 5 if near else 4), ramp)
    rim = {p for p in ellipse(cx, cy, 6 if near else 5, 5 if near else 4) if p[1] >= cy + (3 if near else 2)}
    fill(c, rim, 'o' if near else 'Q')
    if near:
        shade(c, poly([(cx - 1, cy - 4), (cx + 2, cy - 4), (cx + 1, cy - 9)]), BONE)  # bone spike
        put(c, cx - 2, cy - 1, 'n'); put(c, cx - 3, cy, 'n')


def arm(c, sh, hand, near):
    sx, sy = sh; hx, hy = hand
    ex, ey = (sx + hx) / 2 - 1, (sy + hy) / 2 + 1
    ramp_b = BONE if near else ("d", "d", "b", "W")
    ramp_i = IRON if near else IRON_FAR
    shade(c, thick(sx, sy, ex, ey, 3), ramp_b)
    shade(c, thick(ex, ey, hx, hy, 4), ramp_i)          # vambrace
    shade(c, ellipse(hx, hy, 2.5, 2.5), ramp_i)          # gauntlet


def sword(c, hilt, ang, length=30):
    """Greatsword gripped at hilt; ang radians (0 = pointing right, +y down)."""
    hx, hy = hilt
    ux, uy = math.cos(ang), math.sin(ang)
    px, py = -uy, ux
    g0 = (hx - ux * 4, hy - uy * 4)                      # pommel
    shade(c, thick(g0[0], g0[1], hx + ux * 2, hy + uy * 2, 2), ("Q", "Q", "r", "R"))
    shade(c, ellipse(g0[0], g0[1], 1.6, 1.6), GOLD)
    gx, gy = hx + ux * 3, hy + uy * 3                    # crossguard
    shade(c, thick(gx - px * 6, gy - py * 6, gx + px * 6, gy + py * 6, 2), GOLD)
    b0 = (gx + ux * 1, gy + uy * 1)
    tip = (gx + ux * length, gy + uy * length)
    w0, w1 = 3.3, 1.5
    blade = poly([(b0[0] - px * w0, b0[1] - py * w0), (b0[0] + px * w0, b0[1] + py * w0),
                  (tip[0] - ux * 4 + px * w1, tip[1] - uy * 4 + py * w1), tip,
                  (tip[0] - ux * 4 - px * w1, tip[1] - uy * 4 - py * w1)])
    shade(c, blade, BLADE)
    # fuller (dark groove) down the middle
    for i in range(3, int(length) - 7):
        put(c, int(b0[0] + ux * i), int(b0[1] + uy * i), 'm')
    return tip


def smear(c, cx, cy, rmin, r1, a0, a1, ch_out='H', ch_in='S'):
    """Crescent swing trail: hairline at a0, full width (rmin..r1) where it meets the blade at a1."""
    for y in range(c.h):
        for x in range(c.w):
            a = math.atan2(y - cy, x - cx)
            if not (a0 <= a <= a1) or c.g[y][x] != '.':
                continue
            t = (a - a0) / (a1 - a0)
            r0 = r1 - 1.5 - (r1 - rmin - 1.5) * t * t
            d = math.hypot(x - cx, y - cy)
            if r0 <= d <= r1:
                put(c, x, y, ch_out if d > r1 - 1.6 else ch_in)


def glow_orb(c, cx, cy, r):
    fill(c, ellipse(cx, cy, r + 2, r + 2) - set(), 'v')
    fill(c, ellipse(cx, cy, r + 1, r + 1), 'u')
    fill(c, ellipse(cx, cy, r, r), 'U')
    fill(c, ellipse(cx, cy, r - 1, r - 1), 'Z')
    fill(c, ellipse(cx - 1, cy - 1, max(r - 2.5, .6), max(r - 2.5, .6)), 'z')


# ---------- frame ----------
def frame(by=0, bx=0, feet=((25, 0), (35, 0)), near_hand=(41, 38), far_hand=(23, 38),
          ang=1.0, sword_behind=False, sway=0, hurt=False, glow=1, fx=None, length=30,
          lean=0, kneel=False, cast=False):
    """lean shifts head/shoulders forward (+) or back (-) against the hips; kneel drops the near knee."""
    c = Canvas(64, 64)
    sh = lean // 2
    cloak(c, bx, by, sway)
    if sword_behind:
        sword(c, (near_hand[0] + bx, near_hand[1] + by), ang, length)
    if not cast:
        arm(c, (bx + 23 + sh, by + 27), (far_hand[0] + bx, far_hand[1] + by), near=False)
    leg(c, (bx + 26, by + 41), (feet[0][0], 59 - feet[0][1]), near=False)
    pauldron(c, bx + 23 + sh, by + 23, near=False)
    if cast:
        arm(c, (bx + 23 + sh, by + 25), (far_hand[0] + bx, far_hand[1] + by), near=True)
    if not kneel:
        leg(c, (bx + 33, by + 41), (feet[1][0], 59 - feet[1][1]), near=True)
    torso(c, bx, by)
    if kneel:                       # knee comes forward over the tabard
        kneel_leg(c, (bx + 33, by + 41))
    head(c, bx + lean, by, hurt, glow)
    if not sword_behind:
        sword(c, (near_hand[0] + bx, near_hand[1] + by), ang, length)
    pauldron(c, bx + 37 + sh, by + 23, near=True)
    arm(c, (bx + 38 + sh, by + 27), (near_hand[0] + bx, near_hand[1] + by), near=True)
    if fx:
        fx(c)
    c.outline('k')
    return c.rows()


def death(stage):
    c = Canvas(64, 64)
    if stage == 0:   # drops to one knee, leaning on the planted sword
        return frame(by=7, feet=((24, 0), (35, 0)), near_hand=(41, 40), far_hand=(37, 42), ang=1.4, length=22,
                     hurt=True, glow=1, lean=3, kneel=True, sway=2)
    if stage == 1:   # slumps forward, sword falls flat, eyes dimming
        return frame(by=12, bx=1, feet=((23, 0), (35, 0)), near_hand=(42, 47), far_hand=(36, 48), ang=0.05,
                     length=20, hurt=True, glow=0, lean=5, kneel=True, sway=3)
    # collapsed bone pile
    shade(c, poly([(12, 61), (18, 55), (30, 53), (44, 55), (52, 61)]), CLOAK, sep=False)
    sword(c, (6, 61), -0.03, 44)
    shade(c, ellipse(43, 56, 4.5, 3), IRON)
    fill(c, {p for p in ellipse(43, 56, 4.5, 3) if p[1] >= 58}, 'o')
    for (x0, y0, x1, y1) in ((16, 58, 24, 56), (47, 59, 55, 57), (35, 58, 41, 59)):
        shade(c, thick(x0, y0, x1, y1, 2), BONE)
    rib_y = 53 if stage == 2 else 55                       # bones settle after the collapse
    for y, half in ((rib_y, 5), (rib_y + 2, 5), (rib_y + 4, 4)):   # broken rib cage
        shade(c, thick(31 - half, y + 1, 31 + half, y, 2), BONE)
    fill(c, rect(30, rib_y - 1, 2, 7 if stage == 2 else 5), 'W')
    hx, hy = (22, 49) if stage == 2 else (21, 51)
    shade(c, ellipse(hx + 1, hy + 4, 5, 4), BONE)
    shade(c, {p for p in ellipse(hx, hy - 1, 6.5, 5) if p[1] <= hy}, IRON)
    fill(c, rect(hx - 6, hy, 13, 1), 'o')
    shade(c, thick(hx + 4, hy - 4, hx + 8, hy - 8, 3), BONE)
    shade(c, thick(hx - 4, hy - 3, hx - 8, hy - 7, 3), ("d", "d", "b", "W"))
    for ex in (hx, hx + 4):
        fill(c, rect(ex, hy + 2, 2, 2), 'k')
    if stage == 2:
        put(c, hx, hy + 2, 'E'); put(c, hx + 4, hy + 2, 'E')
    if stage == 3:
        put(c, hx + 4, hy + 2, 'Q')
    if stage == 4:   # soul wisp escaping
        for (x, y, ch) in ((24, 44, 'u'), (25, 42, 'U'), (24, 40, 'Z'), (25, 38, 'z'), (23, 36, 'u'), (24, 34, 'v')):
            put(c, x, y, ch)
    c.outline('k')
    return c.rows()


def slam_fx(c):
    for x0, x1, y in ((44, 62, 60), (47, 58, 61)):
        for x in range(x0, x1):
            if (x * 7) % 5 != 0:
                put(c, x, y, 'k')
    for (x, y, r) in ((46, 56, 2), (59, 56, 2), (52, 53, 1.5), (62, 58, 1.5)):
        fill(c, ellipse(x, y, r, r), 'b')
        put(c, int(x) - 1, int(y) - 1, 'W')


def sweep_fx(c):
    smear(c, 46, 31, 12, 23, -2.3, -0.3)


def follow_fx(c):
    smear(c, 45, 40, 14, 18, -0.9, 0.85, 'S', 'M')


def cast_fx(big):
    def f(c):
        glow_orb(c, 15, 9 if big else 12, 4 if big else 3)
        for (x, y) in ((8, 4), (22, 3), (9, 17), (23, 14)) if big else ((10, 8), (20, 16)):
            put(c, x, y, 'z')
    return f


frames = [
    # idle x4 — breathing, both hands resting on the planted greatsword
    ("idle", frame(by=0, near_hand=(40, 31), far_hand=(36, 33), ang=1.42, sway=0)),
    ("idle", frame(by=1, near_hand=(40, 32), far_hand=(36, 34), ang=1.42, sway=1)),
    ("idle", frame(by=2, near_hand=(40, 32), far_hand=(36, 34), ang=1.42, sway=1, glow=2)),
    ("idle", frame(by=1, near_hand=(40, 31), far_hand=(36, 33), ang=1.42, sway=0, glow=2)),
    # run x4 — heavy stomp with big bob, sword dragged low
    ("run", frame(by=2, feet=((17, 0), (44, 0)), near_hand=(42, 38), far_hand=(21, 37), ang=0.9, sway=-2, lean=1)),
    ("run", frame(by=-1, feet=((26, 4), (35, 0)), near_hand=(42, 36), far_hand=(23, 35), ang=0.85, sway=0, lean=1)),
    ("run", frame(by=2, feet=((40, 0), (20, 0)), near_hand=(42, 38), far_hand=(26, 38), ang=0.9, sway=2, lean=1)),
    ("run", frame(by=-1, feet=((30, 0), (30, 4)), near_hand=(42, 36), far_hand=(23, 35), ang=0.85, sway=0, lean=1)),
    # windup (sweep) — sword pulled back over the shoulder, body coiled back
    ("windup", frame(by=1, bx=-2, feet=((22, 0), (36, 0)), near_hand=(30, 26), far_hand=(27, 28),
                     ang=-2.55, sword_behind=True, sway=2, glow=2, length=33, lean=-2)),
    # attack x3 — wide horizontal sweep with smear, follow-through, recover
    ("attack", frame(by=1, bx=2, feet=((22, 0), (40, 0)), near_hand=(44, 31), far_hand=(40, 32), ang=-0.12,
                     sway=-2, fx=sweep_fx, length=17, lean=3)),
    ("attack", frame(by=2, bx=2, feet=((22, 0), (40, 0)), near_hand=(45, 40), far_hand=(40, 40), ang=0.95,
                     sway=-2, fx=follow_fx, length=20, lean=3)),
    ("attack", frame(by=1, bx=1, feet=((23, 0), (39, 0)), near_hand=(43, 38), far_hand=(38, 39), ang=1.25,
                     sway=-1, length=24, lean=1)),
    # slam windup — sword raised overhead with both hands, rising up
    ("slam_windup", frame(by=-1, bx=-1, feet=((24, 0), (36, 0)), near_hand=(33, 8), far_hand=(30, 9),
                          ang=2.75, sword_behind=True, sway=1, glow=2, length=32, lean=-2)),
    # slam x2 — crouched impact, blade driven into the ground, crack + debris
    ("slam", frame(by=5, bx=1, feet=((20, 0), (40, 0)), near_hand=(44, 40), far_hand=(40, 41), ang=0.62,
                   sway=-3, fx=slam_fx, lean=4)),
    ("slam", frame(by=3, bx=1, feet=((21, 0), (39, 0)), near_hand=(44, 39), far_hand=(40, 40), ang=0.62,
                   sway=-1, lean=2)),
    # cast x2 — free hand raised beside the head, spectral orb above it
    ("cast", frame(by=0, near_hand=(40, 31), far_hand=(17, 17), ang=1.42, sway=1, glow=2, fx=cast_fx(False),
                   cast=True, lean=-1)),
    ("cast", frame(by=-1, near_hand=(40, 30), far_hand=(16, 14), ang=1.42, sway=2, glow=2, fx=cast_fx(True),
                   cast=True, lean=-2)),
    # hurt — thrown back
    ("hurt", frame(by=1, bx=-3, near_hand=(38, 34), far_hand=(32, 36), ang=1.55, sway=3, hurt=True, glow=2,
                   lean=-5, length=28)),
    # death x5
    ("death", death(0)), ("death", death(1)), ("death", death(2)), ("death", death(3)), ("death", death(4)),
]

if __name__ == "__main__":
    def headroom(rows):            # shift down 1px so horn tips get a top outline
        c = Canvas(64, 64)
        c.g = [list(r) for r in ["." * 64] + rows[:-1]]
        return c.outline('k').rows()
    frames = [(t, headroom(rows)) for t, rows in frames]
    for t, rows in frames:
        assert len(rows) == 64 and all(len(r) == 64 for r in rows), t
    print(json.dumps({"legend": L, "frames": frames}, separators=(',', ':')))
