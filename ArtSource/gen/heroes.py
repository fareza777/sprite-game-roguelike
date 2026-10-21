"""The four new UNDERSUN heroes (48x48, chibi proportions matching the Knight, facing right).

Wren the Lamplighter (ranged), Morrow the Gravesinger (caster), Brakka the Bellbreaker (heavy),
Sable the Duskblade (assassin). Each hero: idle 4, run 4, attack1 4, attack2 4, attack3 5, dodge 4, skill 4,
hurt 2, death 5 = 36 frames, same tag layout as the Knight.

usage: python heroes.py <hero>   -> JSON {legend, frames:[[tag, rows]]}
"""
import json
import math
import sys

sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
from rig import Canvas, ellipse, fill, glow, poly, put, rect, shade, sweep, thick  # noqa: E402

LEGEND = {
    "k": "#10141f", "f": "#e7d5b3", "F": "#c09473", "j": "#ad7757",
    "D": "#19332d", "d": "#25562e", "n": "#468232", "N": "#75a743",
    "L": "#4d2b32", "l": "#7a4841", "t": "#ad7757", "T": "#c09473",
    "u": "#1e1d39", "p": "#402751", "P": "#7a367b", "U": "#a23e8c",
    "a": "#151d28", "A": "#202e37", "m": "#394a50", "M": "#577277",
    "Q": "#411d31", "r": "#752438", "R": "#a53030", "E": "#cf573c",
    "s": "#819796", "S": "#a8b5b2", "W": "#c7cfcc", "H": "#ebede9",
    "O": "#884b2b", "o": "#be772b", "G": "#de9e41", "y": "#e8c170",
    "c": "#4f8fba", "C": "#73bed3", "w": "#a4dddb", "e": "#da863e",
}
GREEN, LEATHER, SKIN = ("D", "d", "n", "N"), ("L", "l", "t", "T"), ("l", "j", "F", "f")
PURPLE, NAVY, CRIMSON = ("u", "p", "P", "U"), ("a", "A", "m", "M"), ("Q", "r", "R", "E")
BONE, GOLD, STEEL = ("s", "S", "W", "H"), ("O", "o", "G", "y"), ("m", "M", "S", "H")
DARK_LEATHER = ("a", "L", "l", "t")

FEET_Y = 41


# ---------------- weapons ----------------

def crossbow(c, hand, ang):
    hx, hy = hand
    ux, uy = math.cos(ang), math.sin(ang)
    px, py = -uy, ux
    shade(c, thick(hx - ux * 3, hy - uy * 3, hx + ux * 8, hy + uy * 8, 2.6), LEATHER)
    tip = (hx + ux * 7, hy + uy * 7)
    limbs = thick(tip[0] - px * 5.5, tip[1] - py * 5.5, tip[0] + px * 5.5, tip[1] + py * 5.5, 2.2)
    shade(c, limbs, ("L", "L", "l", "t"))
    put(c, tip[0] + ux * 2, tip[1] + uy * 2, "M")                       # iron nose of the stock
    for t in (-5, 5):
        a = (tip[0] + px * t, tip[1] + py * t)
        n = 6
        for i in range(n + 1):
            put(c, a[0] + (hx + ux * 1 - a[0]) * i / n, a[1] + (hy + uy * 1 - a[1]) * i / n, "W")
    for i in range(5):
        put(c, hx + ux * (2 + i), hy + uy * (2 + i) - 1, "H" if i == 4 else "S")


def staff(c, hand, ang, length=19, flame=True):
    hx, hy = hand
    ux, uy = math.cos(ang), math.sin(ang)
    shade(c, thick(hx - ux * 7, hy - uy * 7, hx + ux * length, hy + uy * length, 2), ("L", "l", "t", "T"))
    top = (hx + ux * length, hy + uy * length)
    bell = ellipse(top[0], top[1] + 1, 2.6, 2.4)
    shade(c, bell, GOLD)
    if flame:
        glow(c, top[0], top[1] - 4, 3.4, ["c", "C", "w"])


def bell_hammer(c, hand, ang, length=15):
    hx, hy = hand
    ux, uy = math.cos(ang), math.sin(ang)
    px, py = -uy, ux
    shade(c, thick(hx - ux * 3, hy - uy * 3, hx + ux * length, hy + uy * length, 2.5), ("L", "l", "t", "T"))
    hd = (hx + ux * length, hy + uy * length)
    # a bronze bell: flared mouth facing the swing direction
    # a bronze bell: crown at the haft, flared mouth outward - big enough to read at a glance
    pts = [(hd[0] - px * 3 - ux * 4, hd[1] - py * 3 - uy * 4), (hd[0] + px * 3 - ux * 4, hd[1] + py * 3 - uy * 4),
           (hd[0] + px * 4.5 + ux * 2, hd[1] + py * 4.5 + uy * 2), (hd[0] + px * 6.5 + ux * 6, hd[1] + py * 6.5 + uy * 6),
           (hd[0] - px * 6.5 + ux * 6, hd[1] - py * 6.5 + uy * 6), (hd[0] - px * 4.5 + ux * 2, hd[1] - py * 4.5 + uy * 2)]
    shade(c, poly(pts), GOLD)
    for t in range(-5, 6):  # dark rim band at the mouth
        put(c, hd[0] + px * t + ux * 5, hd[1] + py * t + uy * 5, "O")
    put(c, hd[0] - ux * 5, hd[1] - uy * 5, "y")
    return hd


def dagger(c, hand, ang, length=7):
    hx, hy = hand
    ux, uy = math.cos(ang), math.sin(ang)
    px, py = -uy, ux
    shade(c, thick(hx - px * 2, hy - py * 2, hx + px * 2, hy + py * 2, 1.5), GOLD, sep=False)
    for i in range(1, length):
        put(c, hx + ux * i, hy + uy * i, "H" if i < length - 1 else "W")
        put(c, hx + ux * i + px, hy + uy * i + py, "S")


# ---------------- the shared humanoid ----------------

class Pose:
    def __init__(self, by=0, bx=0, lean=0, feet=((21, 0), (27, 0)), hand_f=(31, 30), hand_b=(17, 30),
                 wpn=0.0, fx=None, ball=False, lying=0, hurt=False, crouch=0, ghost=False):
        self.by, self.bx, self.lean, self.feet = by, bx, lean, feet
        self.hand_f, self.hand_b, self.wpn, self.fx = hand_f, hand_b, wpn, fx
        self.ball, self.lying, self.hurt, self.crouch, self.ghost = ball, lying, hurt, crouch, ghost


class Hero:
    """Subclasses fill in looks; draw() assembles a frame from a Pose."""
    body = LEATHER
    legs = DARK_LEATHER
    width = 10           # torso width
    head_r = (6.5, 6.0)
    robe = False

    def cape(self, c, p): pass
    def head(self, c, hx, hy, p): pass
    def weapon(self, c, p): pass
    def extra_front(self, c, p): pass

    def draw(self, p):
        c = Canvas(48, 48)
        if p.lying:
            self.draw_lying(c, p)
            c.outline("k")
            return c.rows()
        if p.ball:
            self.draw_ball(c, p)
            if p.fx: p.fx(c)
            c.outline("k")
            return c.rows()
        bx, by = p.bx, p.by + p.crouch
        hip_y = 33 + by
        sh_y = 24 + by
        cx = 24 + bx
        self.cape(c, p)
        # back arm
        shade(c, thick(cx - self.width / 2 + 1, sh_y + 1, p.hand_b[0] + bx, p.hand_b[1] + by, 2.6), self.sleeve_back())
        # legs (robed heroes only show feet)
        for i, (fx_, lift) in enumerate(p.feet):
            foot = (fx_ + bx, FEET_Y - lift)
            hip = (cx - 2 + i * 4, hip_y)
            ramp = self.legs if i == 1 else tuple("a" if ch in "Ll" else ch for ch in self.legs)
            if not self.robe:
                shade(c, thick(hip[0], hip[1], foot[0], foot[1] - 1, 3.2), ramp)
            shade(c, rect(foot[0] - 2, foot[1] - 1, 5, 2), ("a", "a", "A", "m"))
        # torso
        w = self.width
        bottom = FEET_Y - 1 if self.robe else hip_y + 1
        flare = 3 if self.robe else 0
        torso = poly([(cx - w / 2, sh_y), (cx + w / 2, sh_y), (cx + w / 2 + flare, bottom), (cx - w / 2 - flare, bottom)])
        shade(c, torso, self.body)
        self.torso_detail(c, cx, sh_y, hip_y, p)
        # head
        hx, hy = cx + p.lean + (-1 if p.hurt else 0), 16 + by + (1 if p.hurt else 0)
        self.head(c, hx, hy, p)
        # front arm + weapon
        self.weapon(c, p)
        shade(c, thick(cx + w / 2 - 1, sh_y + 1, p.hand_f[0] + bx, p.hand_f[1] + by, 2.6), self.sleeve_front())
        shade(c, ellipse(p.hand_f[0] + bx, p.hand_f[1] + by, 1.4, 1.4), SKIN, sep=False)
        self.extra_front(c, p)
        if p.fx:
            p.fx(c)
        if p.ghost:
            for y in range(48):
                for x in range(48):
                    if c.g[y][x] != "." and (x + y) % 2:
                        c.g[y][x] = "."
        c.outline("k")
        return c.rows()

    def sleeve_front(self): return self.body
    def sleeve_back(self): return tuple(("a" if i == 0 else ch) for i, ch in enumerate(self.body))
    def torso_detail(self, c, cx, sh_y, hip_y, p): pass

    def draw_ball(self, c, p):
        cx, cy = 24 + p.bx, 34 + p.by
        shade(c, ellipse(cx, cy, 7, 6.5), self.body)
        shade(c, ellipse(cx + 2, cy - 2, 4.5, 4.2), self.ball_head())
        for i in range(3):  # motion streaks behind the roll
            for x in range(cx - 14 + i * 2, cx - 9):
                put(c, x, cy - 3 + i * 3, "S" if i != 1 else "W")

    def ball_head(self): return self.body

    def draw_lying(self, c, p):
        stage = p.lying
        y = 38
        shade(c, poly([(12, y - 3), (30, y - 4), (32, y + 2), (12, y + 2)]), self.body)
        self.lying_head(c, 34, y - 1 - (2 if stage == 1 else 0), stage)
        if stage >= 2:
            self.dropped_weapon(c, stage)

    def lying_head(self, c, x, y, stage): shade(c, ellipse(x, y, 4.5, 4), SKIN)
    def dropped_weapon(self, c, stage): pass


# ---------------- Wren, the Lamplighter ----------------

class Wren(Hero):
    body = GREEN
    width = 9

    def cape(self, c, p):
        cx, by = 24 + p.bx, p.by + p.crouch
        sway = int(1.5 * math.sin(p.bx * 0.7 + p.by))
        shade(c, poly([(cx - 3, 21 + by), (cx + 2, 21 + by), (cx - 1, 35 + by), (cx - 6 + sway, 37 + by), (cx - 8 + sway, 33 + by)]), DARK_LEATHER, sep=False)

    def torso_detail(self, c, cx, sh_y, hip_y, p):
        for i in range(hip_y - sh_y - 2):                               # quiver strap across the chest
            put(c, cx - 3 + i * 7 // max(1, hip_y - sh_y - 3), sh_y + 1 + i, "l")
        fill(c, rect(cx - 4, hip_y - 3, 9, 1), "L")
        put(c, cx, hip_y - 3, "G")
        # the lantern on her hip - her light in the dark
        lx, ly = cx - 6, hip_y - 1
        shade(c, rect(lx - 1, ly, 3, 4), ("O", "o", "o", "G"), sep=False)
        put(c, lx, ly + 1, "y"); put(c, lx, ly + 2, "y" if p.by % 2 == 0 else "G")
        put(c, lx, ly - 1, "k")

    def head(self, c, hx, hy, p):
        shade(c, ellipse(hx + 1, hy + 1, 4.8, 4.6), SKIN)               # face
        hood = {q for q in ellipse(hx, hy, 6.6, 6.2) if not (q[0] > hx - 1 and q[1] > hy - 3)}
        shade(c, hood, GREEN)
        put(c, hx - 1, hy - 6, "N"); put(c, hx - 2, hy - 7, "n")      # hood tip
        for i in range(3): put(c, hx + i, hy - 3 + (i % 2), "e")         # ginger fringe
        put(c, hx + 3, hy + 1, "k" if not p.hurt else "R")            # eye
        put(c, hx + 2, hy + 4, "j")
        fill(c, rect(hx - 1, hy + 5, 6, 1), "R")                       # scarf

    def weapon(self, c, p):
        crossbow(c, (p.hand_f[0] + p.bx, p.hand_f[1] + p.by), p.wpn)

    def ball_head(self): return GREEN

    def lying_head(self, c, x, y, stage):
        shade(c, ellipse(x, y, 5, 4.5), GREEN)
        put(c, x + 2, y + 1, "F")

    def dropped_weapon(self, c, stage):
        crossbow(c, (8, 41), -0.1)


# ---------------- Morrow, the Gravesinger ----------------

class Morrow(Hero):
    body = PURPLE
    width = 10
    robe = True

    def torso_detail(self, c, cx, sh_y, hip_y, p):
        for y in range(sh_y + 2, FEET_Y - 1):
            put(c, cx, y, "o" if y % 3 else "G")                        # gold hymn trim
        fill(c, rect(cx - 6, FEET_Y - 2, 13, 1), "o")

    def head(self, c, hx, hy, p):
        shade(c, ellipse(hx, hy, 6.6, 6.4), PURPLE)                     # deep hood
        put(c, hx, hy - 7, "P"); put(c, hx - 1, hy - 8, "p")
        mask = ellipse(hx + 1, hy + 1, 3.6, 4.4)
        shade(c, mask, BONE, sep=False)
        put(c, hx, hy, "k"); put(c, hx + 3, hy, "k")                    # eye slits
        put(c, hx, hy + 1, "C" if p.fx is not None else "k")
        put(c, hx + 3, hy + 1, "C" if p.fx is not None else "k")
        for i in range(3): put(c, hx + 1 + (i % 2), hy + 3 + i, "s")    # a tear of ink

    def extra_front(self, c, p):
        # the soul that follows him
        ox, oy = 12 + p.bx, 18 + p.by + int(2 * math.sin(p.bx + p.by * 1.7))
        glow(c, ox, oy, 2.6, ["c", "C", "w"])

    def weapon(self, c, p):
        staff(c, (p.hand_f[0] + p.bx, p.hand_f[1] + p.by), p.wpn)

    def ball_head(self): return BONE

    def lying_head(self, c, x, y, stage):
        shade(c, ellipse(x, y, 5, 4.5), PURPLE)
        shade(c, ellipse(x + 1, y + 1, 2.6, 3), BONE, sep=False)

    def dropped_weapon(self, c, stage):
        staff(c, (10, 42), -0.05, 18, flame=stage < 4)


# ---------------- Brakka, the Bellbreaker ----------------

class Brakka(Hero):
    body = ("Q", "r", "R", "E")      # rust-red jerkin
    legs = ("a", "A", "m", "M")
    width = 14
    head_r = (6.5, 6.0)

    def torso_detail(self, c, cx, sh_y, hip_y, p):
        shade(c, ellipse(cx - 5, sh_y + 1, 3.6, 3), NAVY)               # iron pauldrons
        shade(c, ellipse(cx + 5, sh_y + 1, 3.6, 3), NAVY)
        fill(c, rect(cx - 6, hip_y - 2, 13, 2), "L")
        shade(c, rect(cx - 1, hip_y - 2, 3, 2), GOLD, sep=False)

    def head(self, c, hx, hy, p):
        shade(c, ellipse(hx + 1, hy + 1, 5.2, 5.2), SKIN)                # broad face
        helm = {q for q in ellipse(hx, hy - 1, 6.2, 5.0) if q[1] <= hy - 1}
        shade(c, helm, NAVY)
        fill(c, rect(hx - 6, hy - 1, 13, 1), "o")                        # bronze brow band
        for side in (-1, 1):                                             # short curled horns
            shade(c, thick(hx + side * 5, hy - 3, hx + side * 8, hy - 7, 2.4), BONE, sep=False)
        put(c, hx + 2, hy + 1, "k"); put(c, hx + 5, hy + 1, "k")         # eyes under the brow
        put(c, hx + 4, hy + 3, "j")                                      # nose
        beard = poly([(hx - 2, hy + 4), (hx + 7, hy + 4), (hx + 6, hy + 10), (hx + 2, hy + 12), (hx - 1, hy + 9)])
        shade(c, beard, CRIMSON)
        for y in range(hy + 6, hy + 11, 2): put(c, hx + 3, y, "r")       # braid
        put(c, hx + 3, hy + 11, "G")

    def weapon(self, c, p):
        bell_hammer(c, (p.hand_f[0] + p.bx, p.hand_f[1] + p.by), p.wpn)

    def ball_head(self): return SKIN

    def lying_head(self, c, x, y, stage):
        shade(c, ellipse(x, y, 4.6, 4.4), SKIN)
        shade(c, ellipse(x + 3, y + 3, 3.5, 2.5), CRIMSON)

    def dropped_weapon(self, c, stage):
        bell_hammer(c, (6, 41), -0.15, 14)


# ---------------- Sable, the Duskblade ----------------

class Sable(Hero):
    body = NAVY
    legs = ("a", "a", "A", "m")
    width = 9

    def cape(self, c, p):
        cx, by = 24 + p.bx, p.by + p.crouch
        wave = int(2 * math.sin(p.bx * 0.9 + p.by * 1.3 + 1))
        scarf = poly([(cx - 1, 21 + by), (cx + 2, 22 + by), (cx - 8, 25 + by + wave), (cx - 14, 23 + by + wave),
                      (cx - 11, 21 + by)])
        shade(c, scarf, CRIMSON, sep=False)

    def torso_detail(self, c, cx, sh_y, hip_y, p):
        for i in range(5): put(c, cx - 3 + i * 2, sh_y + 3 + i, "M")     # cross-strap
        fill(c, rect(cx - 4, hip_y - 2, 9, 1), "A")

    def head(self, c, hx, hy, p):
        shade(c, ellipse(hx, hy, 6.2, 5.8), NAVY)                         # hood
        put(c, hx - 2, hy - 6, "m")
        band = rect(hx - 1, hy - 1, 7, 3)
        fill(c, band, "a")
        put(c, hx + 3, hy, "H"); put(c, hx + 4, hy, "W")                 # eye glint
        shade(c, poly([(hx - 2, hy + 2), (hx + 6, hy + 2), (hx + 4, hy + 6), (hx - 1, hy + 6)]), CRIMSON, sep=False)

    def weapon(self, c, p):
        dagger(c, (p.hand_f[0] + p.bx, p.hand_f[1] + p.by), p.wpn)
        dagger(c, (p.hand_b[0] + p.bx, p.hand_b[1] + p.by), p.wpn + math.pi * 0.9)

    def ball_head(self): return CRIMSON

    def lying_head(self, c, x, y, stage):
        shade(c, ellipse(x, y, 4.8, 4.2), NAVY)
        fill(c, rect(x - 1, y + 2, 5, 2), "R")

    def dropped_weapon(self, c, stage):
        dagger(c, (8, 41), 0.05)
        dagger(c, (40, 43), 3.0)


# ---------------- effects ----------------

def muzzle(x, y):
    def f(c):
        for (dx, dy, ch) in ((0, 0, "H"), (1, 0, "y"), (2, 0, "G"), (1, -1, "y"), (1, 1, "y"), (3, 0, "o")):
            put(c, x + dx, y + dy, ch)
    return f


def bolt_trail(x, y, n=10):
    def f(c):
        for i in range(n):
            put(c, x + i, y, "W" if i > n - 3 else "S" if i % 2 else ".")
        put(c, x + n, y, "H")
    return f


def orb_burst(x, y, r=3.4):
    return lambda c: glow(c, x, y, r, ["c", "C", "w", "H"])


def swing(cx, cy, r0, r1, a0, a1, outer="H", inner="S"):
    return lambda c: sweep(c, cx, cy, r0, r1, a0, a1, outer, inner)


def ring(cx, cy, r, ch="G"):
    def f(c):
        for a in range(0, 360, 8):
            put(c, cx + math.cos(math.radians(a)) * r, cy + math.sin(math.radians(a)) * r * 0.45, ch)
    return f


def smoke(cx, cy):
    def f(c):
        for (dx, dy, ch) in ((-3, 0, "m"), (-5, -2, "M"), (2, -3, "m"), (4, 1, "M"), (0, -6, "m"), (-2, 3, "A"), (5, -4, "A")):
            put(c, cx + dx, cy + dy, ch); put(c, cx + dx + 1, cy + dy, ch)
    return f


def combine(*fns):
    return lambda c: [fn(c) for fn in fns if fn]


# ---------------- animation tables ----------------

def common_idle(base_wpn, hf=(31, 30), hb=(17, 30)):
    return [Pose(by=b, wpn=base_wpn, hand_f=(hf[0], hf[1] + b // 1), hand_b=hb) for b in (0, 1, 1, 0)]


def common_run(wpn, hf=(31, 30), hb=(17, 30)):
    feet = [((17, 0), (31, 1)), ((22, 2), (26, 0)), ((31, 1), (17, 0)), ((26, 0), (22, 2))]
    return [Pose(by=(1 if i % 2 == 0 else 0), lean=1, feet=feet[i], wpn=wpn, hand_f=(hf[0], hf[1] + (i % 2)),
                 hand_b=(hb[0] + (2 if i % 2 else -1), hb[1])) for i in range(4)]


def common_dodge(wpn):
    return [Pose(by=2, crouch=2, lean=2, wpn=wpn, hand_f=(30, 32)), Pose(ball=True, bx=1), Pose(ball=True, bx=4),
            Pose(by=1, crouch=1, bx=4, wpn=wpn, hand_f=(32, 31))]


def common_hurt(wpn):
    return [Pose(bx=-2, lean=-2, hurt=True, wpn=wpn, hand_f=(29, 28), hand_b=(15, 26)),
            Pose(bx=-1, lean=-1, hurt=True, wpn=wpn, hand_f=(30, 29))]


def common_death(wpn):
    return [Pose(by=2, crouch=3, lean=2, hurt=True, wpn=wpn, hand_f=(30, 34)), Pose(by=4, crouch=5, lean=4, hurt=True, wpn=wpn + 0.6, hand_f=(31, 37)),
            Pose(lying=1), Pose(lying=2), Pose(lying=4)]


def wren_frames():
    h = Wren()
    aim = 0.0
    idle = common_idle(aim)
    run = common_run(0.35, hf=(30, 31))
    a1 = [Pose(wpn=aim, hand_f=(31, 28)), Pose(bx=-1, wpn=aim, hand_f=(30, 28), fx=muzzle(40, 27)),
          Pose(bx=-1, wpn=aim, hand_f=(30, 28), fx=bolt_trail(38, 27)), Pose(wpn=aim, hand_f=(31, 29))]
    a2 = [Pose(wpn=-0.15, hand_f=(31, 27)), Pose(bx=-1, wpn=-0.15, hand_f=(30, 27), fx=muzzle(40, 25)),
          Pose(bx=-1, wpn=-0.15, hand_f=(30, 27), fx=bolt_trail(38, 25)), Pose(wpn=0, hand_f=(31, 29))]
    a3 = [Pose(by=1, crouch=1, wpn=-0.1, hand_f=(30, 28), hand_b=(26, 27)), Pose(by=1, crouch=1, wpn=-0.1, hand_f=(30, 28), hand_b=(27, 27), fx=muzzle(39, 26)),
          Pose(bx=-2, wpn=-0.1, hand_f=(29, 28), fx=combine(bolt_trail(37, 26, 11), bolt_trail(36, 29, 9))),
          Pose(bx=-1, wpn=0.0, hand_f=(30, 28), fx=muzzle(39, 28)), Pose(wpn=0.0, hand_f=(31, 29))]
    sk = [Pose(by=1, wpn=-0.6, hand_f=(30, 24), hand_b=(20, 26)), Pose(wpn=-0.6, hand_f=(31, 23), fx=orb_burst(35, 18, 3)),
          Pose(bx=-2, wpn=-0.3, hand_f=(30, 25), fx=combine(muzzle(39, 21), bolt_trail(36, 18, 10), bolt_trail(36, 24, 10))),
          Pose(bx=-1, wpn=0.0, hand_f=(31, 28))]
    return h, idle, run, a1, a2, a3, common_dodge(0.4), sk, common_hurt(0.5), common_death(0.3)


def morrow_frames():
    h = Morrow()
    up = -1.25
    idle = [Pose(by=b, wpn=up, hand_f=(30, 30 + b)) for b in (0, 1, 1, 0)]
    run = common_run(-1.0, hf=(30, 30))
    a1 = [Pose(wpn=-0.9, hand_f=(30, 29)), Pose(wpn=-0.5, hand_f=(32, 28), fx=orb_burst(41, 22, 3)),
          Pose(bx=1, wpn=-0.3, hand_f=(33, 28), fx=orb_burst(44, 24, 2.4)), Pose(wpn=-0.9, hand_f=(30, 29))]
    a2 = [Pose(wpn=-1.6, hand_f=(29, 27)), Pose(wpn=-1.0, hand_f=(31, 27), fx=orb_burst(39, 15, 3)),
          Pose(bx=1, wpn=-0.7, hand_f=(32, 28), fx=orb_burst(43, 18, 2.4)), Pose(wpn=-1.1, hand_f=(30, 29))]
    a3 = [Pose(by=1, wpn=-1.4, hand_f=(29, 26)), Pose(by=-1, wpn=-1.5, hand_f=(28, 21), hand_b=(21, 22)),
          Pose(by=-1, wpn=-1.5, hand_f=(28, 21), hand_b=(21, 22), fx=orb_burst(30, 4, 4)),
          Pose(wpn=-0.6, hand_f=(32, 27), fx=combine(orb_burst(41, 20, 3), orb_burst(38, 30, 2.5))), Pose(wpn=-1.1, hand_f=(30, 29))]
    sk = [Pose(by=1, wpn=-1.5, hand_f=(28, 24), hand_b=(19, 24)),
          Pose(by=-1, wpn=-1.55, hand_f=(28, 20), hand_b=(19, 20), fx=combine(orb_burst(10, 20, 2.6), orb_burst(38, 16, 2.6))),
          Pose(by=-1, wpn=-1.55, hand_f=(28, 20), hand_b=(19, 20), fx=combine(orb_burst(8, 26, 2.8), orb_burst(40, 22, 2.8), orb_burst(24, 6, 2.8))),
          Pose(wpn=-1.2, hand_f=(30, 28))]
    return h, idle, run, a1, a2, a3, common_dodge(-1.0), sk, common_hurt(-1.0), common_death(-1.1)


def brakka_frames():
    h = Brakka()
    rest = -2.2       # hammer resting back over the shoulder
    idle = [Pose(by=b, wpn=rest, hand_f=(29, 25 + b), hand_b=(20, 27)) for b in (0, 1, 1, 0)]
    run = common_run(-2.0, hf=(29, 26), hb=(19, 28))
    a1 = [Pose(by=-1, wpn=-2.6, hand_f=(26, 21), hand_b=(22, 22)), Pose(wpn=-1.2, hand_f=(30, 22), fx=swing(28, 26, 10, 18, -2.4, -0.9)),
          Pose(by=1, bx=2, wpn=0.55, hand_f=(32, 31), fx=combine(swing(28, 28, 10, 17, -1.2, 0.5), ring(42, 41, 6, "S"))),
          Pose(by=1, bx=1, wpn=0.7, hand_f=(31, 32))]
    a2 = [Pose(wpn=0.4, hand_f=(31, 30)), Pose(bx=-1, wpn=2.4, hand_f=(19, 28), fx=swing(24, 28, 9, 16, 1.6, 2.9, "S", "M")),
          Pose(bx=-2, wpn=2.8, hand_f=(17, 29)), Pose(wpn=rest, hand_f=(29, 26))]
    a3 = [Pose(by=-2, wpn=-1.7, hand_f=(27, 19), hand_b=(22, 20)), Pose(by=-3, wpn=-1.6, hand_f=(26, 18), hand_b=(22, 19)),
          Pose(by=2, crouch=1, bx=2, wpn=0.9, hand_f=(32, 32), fx=combine(swing(26, 26, 11, 19, -1.7, 0.8), ring(40, 41, 8, "G"))),
          Pose(by=2, crouch=1, bx=2, wpn=0.95, hand_f=(32, 33), fx=ring(40, 41, 10, "o")), Pose(wpn=rest, hand_f=(29, 26))]
    sk = [Pose(by=1, crouch=2, wpn=-1.9, hand_f=(27, 22)), Pose(by=-6, wpn=-1.6, hand_f=(26, 15), hand_b=(22, 16)),
          Pose(by=2, crouch=2, wpn=1.3, hand_f=(30, 34), fx=combine(ring(30, 41, 11, "G"), ring(30, 41, 15, "o"))),
          Pose(by=1, crouch=1, wpn=1.2, hand_f=(30, 33), fx=ring(30, 41, 18, "O"))]
    return h, idle, run, a1, a2, a3, common_dodge(-1.8), sk, common_hurt(-2.0), common_death(-1.5)


def sable_frames():
    h = Sable()
    grip = 0.25
    idle = [Pose(by=b + 1, crouch=1, wpn=grip, hand_f=(31, 30 + b), hand_b=(17, 31)) for b in (0, 1, 1, 0)]
    run = common_run(0.6, hf=(31, 31), hb=(16, 30))
    a1 = [Pose(by=1, wpn=-1.0, hand_f=(29, 25)), Pose(bx=2, wpn=0.2, hand_f=(34, 28), fx=swing(28, 28, 8, 14, -1.4, 0.2)),
          Pose(bx=2, wpn=0.7, hand_f=(33, 31)), Pose(by=1, wpn=grip, hand_f=(31, 30))]
    a2 = [Pose(by=1, wpn=0.9, hand_f=(30, 33), hand_b=(20, 26)), Pose(bx=2, wpn=grip, hand_f=(31, 31), hand_b=(33, 27), fx=swing(28, 29, 8, 13, -0.8, 0.9, "S", "M")),
          Pose(bx=2, wpn=grip, hand_f=(31, 31), hand_b=(34, 30)), Pose(by=1, wpn=grip, hand_f=(31, 30))]
    a3 = [Pose(by=2, crouch=2, wpn=-0.5, hand_f=(28, 27), hand_b=(20, 27)),
          Pose(bx=3, wpn=0.0, hand_f=(35, 28), hand_b=(34, 31), fx=swing(28, 28, 9, 15, -2.6, 0.3)),
          Pose(bx=3, wpn=1.2, hand_f=(34, 31), hand_b=(15, 26), fx=swing(26, 30, 9, 15, 0.0, 2.8, "S", "M")),
          Pose(bx=2, wpn=0.6, hand_f=(33, 30)), Pose(by=1, wpn=grip, hand_f=(31, 30))]
    sk = [Pose(by=2, crouch=2, wpn=grip, hand_f=(30, 31), fx=smoke(22, 30)), Pose(ghost=True, bx=4, wpn=grip, fx=smoke(16, 30)),
          Pose(ghost=True, bx=10, wpn=0.2, hand_f=(33, 29), fx=smoke(8, 30)), Pose(bx=10, by=1, wpn=0.3, hand_f=(33, 30), fx=swing(34, 28, 8, 13, -1.0, 0.5))]
    return h, idle, run, a1, a2, a3, common_dodge(0.4), sk, common_hurt(0.4), common_death(0.3)


HEROES = {"wren": wren_frames, "morrow": morrow_frames, "brakka": brakka_frames, "sable": sable_frames}
TAGS = ["idle", "run", "attack1", "attack2", "attack3", "dodge", "skill", "hurt", "death"]

if __name__ == "__main__":
    hero, *anims = HEROES[sys.argv[1]]()
    frames = []
    for tag, poses in zip(TAGS, anims):
        for pose in poses:
            rows = hero.draw(pose)
            assert len(rows) == 48 and all(len(r) == 48 for r in rows)
            frames.append([tag, rows])
    print(json.dumps({"legend": LEGEND, "frames": frames}, separators=(",", ":")))
