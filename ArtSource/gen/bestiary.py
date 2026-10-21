"""Parametric bestiary: draws every new enemy (32x32) and boss (64x64) from six body plans.
Regular strip: idle x2, run x4, windup x2, attack x3, hurt x1, death x4  (16 frames)
Boss strip:    idle x2, run x4, windup x2, attack x3, cast x3, hurt x1, death x5  (20 frames)
usage: python bestiary.py <id> > out.json   |   python bestiary.py --list
Prints {legend, frames:[[tag, rows]...], size, tags}."""
import json
import math
import sys

sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
from compose import Canvas  # noqa: E402
from rig import ellipse, poly, thick, rect, put, fill, shade  # noqa: E402

L = {"k": "#090a14", "1": "#394a50", "2": "#577277", "3": "#819796", "4": "#c7cfcc", "H": "#ebede9", "5": "#4d2b32",
     "6": "#7a4841", "7": "#ad7757", "8": "#c09473", "a": "#884b2b", "b": "#be772b", "c": "#de9e41", "d": "#e8c170",
     "e": "#602c2c", "f": "#a53030", "g": "#cf573c", "h": "#da863e", "i": "#253a5e", "j": "#3c5e8b", "l": "#4f8fba",
     "m": "#73bed3", "n": "#a4dddb", "o": "#402751", "p": "#7a367b", "q": "#a23e8c", "r": "#c65197", "R": "#df84a5",
     "s": "#25562e", "t": "#468232", "u": "#75a743", "v": "#a8ca58", "V": "#d0da91", "x": "#d7b594", "y": "#e7d5b3",
     "z": "#151d28", "Z": "#202e37", "w": "#10141f", "A": "#241527", "B": "#411d31", "C": "#752438", "D": "#1e1d39",
     "E": "#19332d", "F": "#341c27", "G": "#172038", "W": "#a8b5b2"}

R_ = {  # ramps (deep, dark, base, light)
    "steel": ("1", "2", "3", "4"), "wood": ("5", "6", "7", "8"), "gold": ("a", "b", "c", "d"), "red": ("e", "f", "g", "h"),
    "blue": ("G", "j", "l", "m"), "ice": ("j", "l", "m", "n"), "violet": ("o", "p", "q", "r"), "pink": ("p", "q", "r", "R"),
    "green": ("E", "s", "t", "u"), "moss": ("s", "t", "u", "v"), "bone": ("7", "8", "x", "y"), "sand": ("6", "7", "8", "x"),
    "dark": ("w", "z", "Z", "1"), "night": ("D", "o", "p", "q"), "crimson": ("A", "B", "C", "f"), "brass": ("5", "a", "b", "c"),
    "slate": ("z", "Z", "1", "2"), "teal": ("E", "Z", "j", "l"), "glass": ("G", "l", "n", "H"), "flesh": ("6", "7", "8", "x"),
    "ember": ("F", "e", "f", "g"), "pale": ("2", "3", "W", "4"),
}

# ---------------------------------------------------------------- poses
REG = [("idle", 2), ("run", 4), ("windup", 2), ("attack", 3), ("hurt", 1), ("death", 4)]
BOSS = [("idle", 2), ("run", 4), ("windup", 2), ("attack", 3), ("cast", 3), ("hurt", 1), ("death", 5)]


class Pose:
    def __init__(self, tag, i, n):
        self.tag, self.i, self.n = tag, i, n
        self.t = i / max(1, n)
        self.bob = 0
        self.step = 0.0          # -1..1 leg swing
        self.lean = 0.0          # + forward (right)
        self.reach = 0.0         # 0..1 attack extension
        self.raise_ = 0.0        # 0..1 arms/weapon raised
        self.squash = 0.0        # hurt squash
        self.dead = 0.0          # 0..1 collapse
        self.glow = 0.0          # cast glow
        if tag == "idle":
            self.bob = i % 2
        elif tag == "run":
            self.step = math.sin(self.t * 2 * math.pi)
            self.bob = 1 if i % 2 else 0
            self.lean = 0.5
        elif tag == "windup":
            self.lean = -1.0 - i * 0.5
            self.raise_ = 0.5 + 0.5 * i
        elif tag == "attack":
            self.lean = 1.5 - i * 0.6
            self.reach = 1.0 if i < 2 else 0.5
            self.raise_ = 0.0
        elif tag == "cast":
            self.raise_ = 1.0
            self.glow = 0.5 + 0.5 * math.sin(i * 2.1)
            self.bob = -(i % 2)
        elif tag == "hurt":
            self.squash = 1.0
            self.lean = -1.5
        elif tag == "death":
            self.dead = (i + 1) / n


# ---------------------------------------------------------------- helpers
def eyes(c, x, y, color="g", gap=3, size=1):
    for dx in (0, gap):
        fill(c, rect(x + dx, y, size, size), color)


def dither_fade(c, amount):
    """Death fade: knock out a growing checker of pixels."""
    if amount <= 0.5:
        return
    keep = 1.0 - (amount - 0.5) * 2
    for y in range(c.h):
        for x in range(c.w):
            if c.g[y][x] != "." and ((x * 7 + y * 13) % 10) / 10.0 > keep:
                c.g[y][x] = "."


def collapse(c, p, floor_y):
    """Squash everything toward the floor as the creature dies."""
    if p.dead <= 0:
        return
    k = 1.0 - 0.55 * min(1.0, p.dead * 1.3)
    rows = [row[:] for row in c.g]
    for y in range(c.h):
        for x in range(c.w):
            c.g[y][x] = "."
    for y in range(c.h):
        ny = int(floor_y - (floor_y - y) * k)
        if 0 <= ny < c.h:
            for x in range(c.w):
                if rows[y][x] != ".":
                    c.g[ny][x] = rows[y][x]
    dither_fade(c, p.dead)


# ---------------------------------------------------------------- body plans
def humanoid(c, p, s, u):
    """Upright biped. s = spec dict, u = unit scale (1 small, 2 boss)."""
    W = c.w
    cx = W // 2 + int(p.lean * u * 0.6)
    feet = c.h - 2
    hgt = s.get("height", 1.18 if u > 1 else 1.0)
    hip = feet - int(8 * u * hgt) + p.bob
    sh = hip - int(8 * u * hgt)
    hy = sh - int(4 * u * s.get("headsize", 1.0)) + int(p.squash * u)
    body, skin = R_[s["body"]], R_[s.get("skin", s["body"])]
    # legs
    if not s.get("robe"):
        for side, ph in ((-1, 1), (1, -1)):
            fx = cx + side * 2 * u + int(p.step * ph * 2 * u)
            shade(c, thick(cx + side * 2 * u, hip, fx, feet - u, 2.2 * u), R_[s.get("legs", s["body"])], sep=False)
            fill(c, rect(fx - u, feet - u, 3 * u, u + 1), R_[s.get("legs", s["body"])][0])
    # torso / robe
    tw = s.get("width", 4.5) * u
    bottom = feet if s.get("robe") else hip + u
    flare = 2 * u if s.get("robe") else 0
    top_w = tw * (0.8 if s.get("robe") else 1.0)
    shade(c, poly([(cx - top_w, sh), (cx + top_w, sh), (cx + tw + flare, bottom), (cx - tw - flare, bottom)]), body)
    trim = s.get("trim", R_[s["body"]][0])
    if s.get("robe"):
        for k in (-1, 0, 1):   # folds
            x0 = cx + k * tw * 0.5
            c.line(int(x0), int(sh + 3 * u), int(x0 + k * flare * 0.6), int(bottom - u), body[1])
        fill(c, rect(cx - tw - flare + 1, bottom - u, 2 * (tw + flare) - 1, u), trim)
        c.line(int(cx + u), int(sh + u), int(cx + u), int(bottom - u), trim)
    else:
        c.line(int(cx - tw + u), int(sh + 2 * u), int(cx + tw - u), int(sh + 2 * u), body[1])
    if s.get("mantle"):
        shade(c, ellipse(cx, sh + u, tw + 1.5 * u, 2.5 * u), R_[s["mantle"]], sep=False)
    if s.get("belt"):
        fill(c, rect(cx - tw, hip - u, 2 * tw + 1, u), s["belt"])
        put(c, cx + u, hip - u, "d")
    # back arm
    reach = p.reach * 4 * u
    up = p.raise_ * 6 * u
    # head
    head = s.get("head", "plain")
    hr = 3.6 * u * s.get("headsize", 1.0)
    if head == "skull":
        shade(c, ellipse(cx + u, hy, hr, hr * 0.9), R_[s.get("skullramp", "bone")])
        for dx in (0, 3 * u):   # deep sockets with a cold pinpoint of light
            fill(c, ellipse(cx - u * 0.5 + dx, hy, 0.9 * u, 1.1 * u), "k")
            put(c, cx - u * 0.5 + dx, hy, s.get("eye", "g"))
        c.line(int(cx), int(hy + hr * 0.6), int(cx + 3 * u), int(hy + hr * 0.6), "6")
    elif head == "hood":
        shade(c, ellipse(cx + u, hy, hr * 1.15, hr * 1.1), R_[s.get("hood", s["body"])])
        fill(c, ellipse(cx + 2 * u, hy + u, hr * 0.6, hr * 0.55), "k")
        eyes(c, int(cx + u), int(hy + u), s.get("eye", "g"), gap=int(2 * u), size=u)
    elif head == "helm":
        shade(c, ellipse(cx + u, hy, hr, hr), R_[s.get("helm", "steel")])
        fill(c, rect(cx - u, hy, 5 * u, u), "k")
        put(c, cx + u, hy, s.get("eye", "g")); put(c, cx + 3 * u, hy, s.get("eye", "g"))
    elif head == "fish":
        shade(c, ellipse(cx + 2 * u, hy, hr * 1.2, hr * 0.8), R_[s.get("hood", "teal")])
        put(c, cx + 3 * u, hy - u, "H"); put(c, cx + 3 * u, hy - u + 1, "k")
        c.line(int(cx + 4 * u), int(hy + u), int(cx + 2 * u), int(hy + u), "e")
    elif head == "cap":   # mushroom
        shade(c, ellipse(cx + u, hy + u, hr * 0.8, hr * 0.8), skin)
        cap = {q for q in ellipse(cx + u, hy - u, hr * 1.5, hr) if q[1] <= hy}
        shade(c, cap, R_[s.get("hood", "pink")])
        for k in range(3):
            put(c, cx - u + k * 2 * u, hy - 2 * u, "y")
        eyes(c, int(cx), int(hy + u), s.get("eye", "k"), gap=int(2 * u), size=u)
    elif head == "mask" and s.get("mask") == "pharaoh":
        nemes = poly([(cx - hr, hy - hr * 0.6), (cx + hr + 2 * u, hy - hr * 0.6), (cx + hr + 3 * u, hy + hr * 1.6), (cx - hr - u, hy + hr * 1.6)])
        shade(c, nemes, R_["gold"])
        for yy in range(int(hy - hr * 0.4), int(hy + hr * 1.6), 2 * u):
            c.line(int(cx - hr), yy, int(cx + hr + 2 * u), yy, "j")
        shade(c, ellipse(cx + u, hy + u, hr * 0.7, hr * 0.8), R_["gold"], sep=False)
        put(c, cx, hy, "k"); put(c, cx + 2 * u, hy, "k")
        put(c, cx, hy, s.get("eye", "l")); put(c, cx + 2 * u, hy, s.get("eye", "l"))
        shade(c, rect(cx + u - u // 2, hy + hr * 0.9, u, 3 * u), R_["blue"], sep=False)   # braided beard
    elif head == "mask" and s.get("mask") == "clock":
        shade(c, ellipse(cx + u, hy, hr, hr), R_["gold"])
        fill(c, ellipse(cx + u, hy, hr * 0.75, hr * 0.75), "y")
        for k in range(12):
            a = k * math.pi / 6
            put(c, cx + u + math.cos(a) * hr * 0.6, hy + math.sin(a) * hr * 0.6, "6")
        c.line(int(cx + u), int(hy), int(cx + u), int(hy - hr * 0.55), "k")
        c.line(int(cx + u), int(hy), int(cx + u + hr * 0.4), int(hy + hr * 0.2), "k")
    elif head == "mask":
        shade(c, ellipse(cx + u, hy, hr, hr), R_[s.get("hood", "gold")])
        fill(c, rect(cx, hy - u, u, 2 * u), "k"); fill(c, rect(cx + 2 * u, hy - u, u, 2 * u), "k")
    elif head == "wrapped":
        shade(c, ellipse(cx + u, hy, hr, hr), R_["sand"])
        for yy in range(int(hy - hr), int(hy + hr), 2):
            c.line(int(cx - hr + u), yy, int(cx + hr + u), yy + 1, "6")
        put(c, cx + 2 * u, hy, s.get("eye", "l"))
    else:
        shade(c, ellipse(cx + u, hy, hr, hr), skin)
        eyes(c, int(cx + u), int(hy), s.get("eye", "k"), gap=int(2 * u), size=u)
    if s.get("horns"):
        for side in (-1, 1):
            shade(c, thick(cx + u + side * hr * 0.6, hy - hr * 0.6, cx + u + side * hr * 1.2, hy - hr * 1.5, 1.4 * u), R_[s["horns"]], sep=False)
    if s.get("crown"):
        for k in range(-1, 2):
            shade(c, poly([(cx + u + k * 2 * u - u, hy - hr), (cx + u + k * 2 * u + u, hy - hr), (cx + u + k * 2 * u, hy - hr - 2.5 * u)]), R_["gold"], sep=False)
    # front arm + weapon
    ax, ay = cx + 2 * u, sh + u
    hx, hy2 = ax + 3 * u + reach, ay + 5 * u - up
    shade(c, thick(ax, ay, hx, hy2, 2 * u), skin if s.get("bare_arms") else body, sep=False)
    w = s.get("weapon")
    if w == "sword":
        shade(c, thick(hx, hy2, hx + 5 * u + reach, hy2 - 5 * u + up * 1.2, 1.6 * u), R_["steel"], sep=False)
    elif w == "spear":
        c.line(int(hx - 4 * u), int(hy2 + 3 * u), int(hx + 7 * u + reach), int(hy2 - 4 * u), "6")
        shade(c, poly([(hx + 7 * u + reach, hy2 - 4 * u), (hx + 10 * u + reach, hy2 - 6 * u), (hx + 8 * u + reach, hy2 - 3 * u)]), R_["steel"], sep=False)
    elif w == "staff":
        c.line(int(hx), int(hy2 - 8 * u), int(hx), int(feet), "6")
        shade(c, ellipse(hx, hy2 - 9 * u, 1.8 * u + p.glow * u, 1.8 * u + p.glow * u), R_[s.get("orb", "ice")], sep=False)
    elif w == "claws":
        for k in range(3):
            c.line(int(hx), int(hy2 + k), int(hx + 3 * u + reach), int(hy2 + k - u), s.get("claw", "4"))
    elif w == "hammer":
        c.line(int(hx), int(hy2), int(hx + 3 * u), int(hy2 - 7 * u + up), "6")
        shade(c, rect(hx + u, hy2 - 10 * u + up, 5 * u, 4 * u), R_[s.get("hammer", "steel")], sep=False)
    if s.get("tail"):
        c.line(int(cx - tw), int(hip), int(cx - tw - 4 * u), int(hip - 3 * u + p.bob), R_[s["tail"]][2])
    if p.glow > 0 and s.get("aura"):
        for k in range(6):
            a = k * math.pi / 3 + p.i
            put(c, cx + math.cos(a) * 7 * u, sh + math.sin(a) * 7 * u, s["aura"])


def beast(c, p, s, u):
    """Four-legged hunter seen from the side, facing right."""
    W = c.w
    feet = c.h - 2
    lean = int(p.lean * u)
    by = feet - int(6 * u) + p.bob + int(p.squash * u)
    bx = W // 2 + lean
    body = R_[s["body"]]
    bl = s.get("length", 7) * u
    # legs (back pair then front pair)
    for k, (ox, ph) in enumerate(((-bl * 0.6, 1), (bl * 0.55, -1))):
        for d in (0, 1):
            sw = p.step * ph * (1 if d else -1) * 2 * u
            lx = bx + ox + d * u
            shade(c, thick(lx, by + u, lx + sw, feet, 1.8 * u), R_[s.get("legs", s["body"])], sep=False)
    shade(c, ellipse(bx, by - u, bl, 3.2 * u), body)
    # head
    hx = bx + bl + u + int(p.reach * 3 * u)
    hy = by - 3 * u - int(p.raise_ * 2 * u)
    shade(c, ellipse(hx, hy, 3.2 * u, 2.6 * u), body, sep=False)
    shade(c, poly([(hx + u, hy - u), (hx + 6 * u, hy + u), (hx + u, hy + 2 * u)]), body, sep=False)   # snout
    if p.tag in ("attack", "windup"):
        c.line(int(hx + 2 * u), int(hy + 2 * u), int(hx + 5 * u), int(hy + 3 * u), "H")
    put(c, hx + u, hy - u, s.get("eye", "g"))
    for side in (0, 1):  # ears
        shade(c, poly([(hx - u + side * u, hy - 2 * u), (hx + side * u, hy - 5 * u), (hx + u + side * u, hy - 2 * u)]), body, sep=False)
    # tail
    tx = bx - bl
    c.line(int(tx), int(by - u), int(tx - 4 * u), int(by - 4 * u + p.bob), body[2])
    if s.get("spikes"):
        for k in range(4):
            x = bx - bl * 0.6 + k * bl * 0.4
            shade(c, poly([(x - u, by - 3 * u), (x + u, by - 3 * u), (x, by - 6 * u)]), R_[s["spikes"]], sep=False)
    if s.get("mane"):
        shade(c, ellipse(bx + bl * 0.6, by - 3 * u, 3 * u, 2.5 * u), R_[s["mane"]], sep=False)


def crab(c, p, s, u):
    """Wide low shell on six legs with claws or mandibles."""
    W = c.w
    feet = c.h - 2
    cx = W // 2 + int(p.lean * u)
    cy = feet - int(5 * u) + p.bob + int(p.squash * u)
    shell = R_[s["body"]]
    for k in range(3):
        for side in (-1, 1):
            sw = p.step * (1 if (k + (side > 0)) % 2 else -1) * u
            x0 = cx + side * (2 + k * 2) * u
            c.line(int(x0), int(cy), int(x0 + side * 3 * u + sw), int(feet), shell[0])
    shade(c, ellipse(cx, cy - u, s.get("width", 7) * u, 4 * u), shell)
    if s.get("pattern"):
        for k in range(-2, 3):
            put(c, cx + k * 2 * u, cy - 2 * u, s["pattern"])
    claw = s.get("claws")
    if claw:
        for side in (-1, 1):
            ox = cx + side * (s.get("width", 7) + 2) * u + int(p.reach * side * 2 * u)
            oy = cy - 3 * u - int(p.raise_ * 3 * u)
            shade(c, ellipse(ox, oy, 2.6 * u, 2 * u), R_[claw], sep=False)
            c.line(int(ox), int(oy), int(ox + side * 2 * u), int(oy - 2 * u), "k")
    eyes(c, int(cx - u), int(cy - 5 * u), s.get("eye", "k"), gap=int(2 * u), size=u)
    if s.get("gear"):
        for k in range(8):
            a = k * math.pi / 4 + p.i * 0.4
            put(c, cx + math.cos(a) * 3 * u, cy - u + math.sin(a) * 2 * u, s["gear"])


def blob(c, p, s, u):
    """Squishy body that stretches when moving: slimes, sporelings, bombs, maws."""
    W = c.w
    feet = c.h - 2
    sq = 1.0 + (0.25 if p.tag in ("windup",) else 0) - (0.2 if p.tag == "attack" else 0)
    rx = s.get("size", 5) * u * (1.0 + (sq - 1) * 0.8)
    ry = s.get("size", 5) * u * (1.0 / sq) * 0.85
    cx = W // 2 + int(p.lean * u * 0.5)
    cy = feet - ry - (2 * u if p.tag == "attack" else 0) + p.bob
    shade(c, ellipse(cx, cy, rx, ry), R_[s["body"]])
    kind = s.get("kind")
    if kind == "mushroom":
        cap = {q for q in ellipse(cx, cy - ry * 0.6, rx * 1.3, ry * 0.8) if q[1] <= cy - ry * 0.4}
        shade(c, cap, R_[s.get("cap", "pink")])
        for k in range(-1, 2):
            put(c, cx + k * 2 * u, cy - ry, "y")
    elif kind == "bomb":
        c.line(int(cx + rx * 0.5), int(cy - ry), int(cx + rx * 0.8), int(cy - ry - 3 * u), "6")
        if p.i % 2 or p.tag == "windup":
            shade(c, ellipse(cx + rx * 0.8, cy - ry - 3 * u, 1.2 * u, 1.2 * u), R_["gold"], sep=False)
        fill(c, rect(cx - rx, cy, 2 * rx, u), "c")
    elif kind == "maw":
        mouth = {q for q in ellipse(cx + u, cy + u, rx * 0.7, ry * (0.25 + 0.4 * p.reach + 0.3 * p.raise_))}
        fill(c, mouth, "A")
        for k in range(-2, 3):
            put(c, cx + u + k * 2 * u, cy + u - ry * 0.25, "y")
    eyes(c, int(cx - u), int(cy - ry * 0.2), s.get("eye", "k"), gap=int(3 * u), size=u)


def floater(c, p, s, u):
    """Hovers above its shadow: wisps, eyes, wraiths, eels, sprites. Bobbing is the run cycle."""
    W = c.w
    feet = c.h - 2
    hover = int(math.sin(p.t * 2 * math.pi) * u) if p.tag in ("idle", "run") else 0
    cx = W // 2 + int(p.lean * u)
    cy = feet - 10 * u + hover - int(p.raise_ * 2 * u) + int(p.squash * u)
    kind = s.get("kind")
    body = R_[s["body"]]
    if kind == "eye":
        shade(c, ellipse(cx, cy, 5 * u, 5 * u), body)
        shade(c, ellipse(cx + u, cy, 2.6 * u, 2.6 * u), R_[s.get("iris", "red")], sep=False)
        fill(c, ellipse(cx + 1.5 * u, cy, 1.1 * u, 1.1 * u), "k")
        put(c, cx - u, cy - 2 * u, "H")
        for k in range(5):
            a = math.pi * (0.2 + 0.15 * k)
            c.line(int(cx + math.cos(a) * 4 * u), int(cy + math.sin(a) * 4 * u),
                   int(cx + math.cos(a) * 7 * u), int(cy + math.sin(a) * 7 * u + hover), body[1])
    elif kind == "eel":
        pts = []
        for k in range(9):
            x = cx - 7 * u + k * 1.8 * u
            y = cy + math.sin(k * 0.9 + p.i * 1.3) * 2 * u
            pts.append((x, y))
        for (x0, y0), (x1, y1) in zip(pts, pts[1:]):
            shade(c, thick(x0, y0, x1, y1, 3 * u), body, sep=False)
        hx, hy = pts[-1]
        shade(c, ellipse(hx + u, hy, 2.4 * u, 2 * u), body, sep=False)
        put(c, hx + 2 * u, hy - u, s.get("eye", "n"))
    elif kind == "shard":
        shade(c, poly([(cx, cy - 6 * u), (cx + 4 * u, cy), (cx, cy + 5 * u), (cx - 4 * u, cy)]), body)
        c.line(int(cx), int(cy - 5 * u), int(cx), int(cy + 4 * u), "H")
        for side in (-1, 1):
            shade(c, poly([(cx + side * 4 * u, cy - u), (cx + side * 8 * u, cy - 4 * u + hover), (cx + side * 6 * u, cy + u)]), R_["glass"], sep=False)
    else:   # wisp / wraith
        tail = poly([(cx - 4 * u, cy), (cx + 4 * u, cy), (cx + 2 * u + hover, cy + 9 * u), (cx - u, cy + 6 * u), (cx - 3 * u - hover, cy + 8 * u)])
        shade(c, tail, body)
        shade(c, ellipse(cx, cy - u, 4.4 * u, 4.4 * u), body, sep=False)
        if kind == "wraith":
            fill(c, ellipse(cx + u, cy - u, 2.6 * u, 2.4 * u), "k")
        eyes(c, int(cx - u), int(cy - u), s.get("eye", "H"), gap=int(2 * u), size=u)
        if s.get("arms"):
            for side in (-1, 1):
                c.line(int(cx + side * 4 * u), int(cy + u), int(cx + side * (7 + p.reach * 3) * u), int(cy + (3 - p.raise_ * 6) * u), body[2])
    if p.glow > 0 or p.tag == "windup":
        for k in range(8):
            a = k * math.pi / 4 + p.i
            put(c, cx + math.cos(a) * 8 * u, cy + math.sin(a) * 8 * u, s.get("spark", "n"))


def golem(c, p, s, u):
    """Heavy blocky giant with a glowing core: slag, brass, roots."""
    W = c.w
    feet = c.h - 2
    cx = W // 2 + int(p.lean * u * 0.5)
    hip = feet - 7 * u + p.bob
    top = hip - 11 * u + int(p.squash * u)
    body = R_[s["body"]]
    for side, ph in ((-1, 1), (1, -1)):
        lx = cx + side * 3 * u + int(p.step * ph * 1.5 * u)
        shade(c, rect(lx - 2 * u, hip, 4 * u, feet - hip + 1), body)
    shade(c, poly([(cx - 7 * u, top + 2 * u), (cx + 7 * u, top + 2 * u), (cx + 6 * u, hip + u), (cx - 6 * u, hip + u)]), body)
    for k in (-1, 1):   # plate seams
        c.line(int(cx + k * 3 * u), int(top + 3 * u), int(cx + k * 3 * u), int(hip), body[1])
    c.line(int(cx - 6 * u), int(top + 9 * u), int(cx + 6 * u), int(top + 9 * u), body[1])
    shade(c, ellipse(cx + u, top, 3.4 * u, 3 * u), body, sep=False)
    eyes(c, int(cx), int(top), s.get("eye", "d"), gap=int(2 * u), size=u)
    for side in (-1, 1):   # pauldrons
        shade(c, ellipse(cx + side * 7 * u, top + 2 * u, 3 * u, 2.2 * u), body, sep=False)
        if s.get("spikes"):
            shade(c, poly([(cx + side * 6 * u, top), (cx + side * 8 * u, top), (cx + side * 7.5 * u, top - 4 * u)]), R_[s["spikes"]], sep=False)
    if s.get("cracks"):
        for k in range(3):
            c.line(int(cx - 4 * u + k * 4 * u), int(top + 4 * u), int(cx - 3 * u + k * 3 * u), int(top + 8 * u + k), s["cracks"])
    core = R_[s.get("core", "ember")]
    shade(c, ellipse(cx, top + 6 * u, 2.4 * u + p.glow * u, 2.4 * u + p.glow * u), core, sep=False)
    for side in (-1, 1):
        ax = cx + side * 8 * u
        ay = top + 3 * u
        fx = ax + side * u + (int(p.reach * 3 * u) if side > 0 else 0)
        fy = ay + 8 * u - int(p.raise_ * 10 * u)
        shade(c, thick(ax, ay, fx, fy, 3.4 * u), body, sep=False)
        shade(c, ellipse(fx, fy, 2.4 * u, 2.4 * u), body, sep=False)
    if s.get("vines"):
        for k in range(5):
            c.line(int(cx - 6 * u + k * 3 * u), int(top + 2 * u), int(cx - 5 * u + k * 3 * u), int(hip), s["vines"])
    if s.get("rivets"):
        for k in range(-2, 3):
            put(c, cx + k * 2 * u, top + 3 * u, s["rivets"])


def choir(c, p, s, u):
    """The Mirrored Choir: three crystal singers. The middle one is real; the reflections are violet and smaller."""
    W = c.w
    feet = c.h - 2
    for k, (ox, scale, ramp) in enumerate(((-17, 0.75, R_["violet"]), (17, 0.75, R_["violet"]), (0, 1.0, R_["glass"]))):
        sway = int(math.sin(p.t * 2 * math.pi + k * 2.1) * u) if p.tag in ("idle", "run", "cast") else 0
        cx = W // 2 + ox + int(p.lean * u * 0.5)
        top = feet - int(26 * u * scale) + sway + int(p.squash * u)
        body = poly([(cx, top + 6 * u * scale), (cx + 5 * u * scale, top + 14 * u * scale), (cx + 3 * u * scale, feet),
                     (cx - 3 * u * scale, feet), (cx - 5 * u * scale, top + 14 * u * scale)])
        shade(c, body, ramp)
        c.line(int(cx), int(top + 7 * u * scale), int(cx), int(feet - u), ramp[3])
        for side in (-1, 1):   # wings of shards
            reach = (3 + p.raise_ * 3) * u * scale
            shade(c, poly([(cx + side * 4 * u * scale, top + 10 * u * scale), (cx + side * (9 * u * scale + reach), top + (4 - p.raise_ * 3) * u * scale),
                           (cx + side * 7 * u * scale, top + 14 * u * scale)]), ramp, sep=False)
        shade(c, ellipse(cx, top + 3 * u * scale, 3 * u * scale, 3 * u * scale), ramp, sep=False)   # head gem
        mouth = 1 + (1 if p.tag in ("cast", "attack") else 0)
        fill(c, rect(cx - u, top + 4 * u * scale, 2 * u, mouth * u), "k")
        if k == 2:
            for j in range(16):
                if j % 4 == 3:
                    continue
                a = j * math.pi / 8
                put(c, cx + math.cos(a) * 7 * u, top - 2 * u + math.sin(a) * 2 * u, "d")


PLANS = {"choir": choir, "humanoid": humanoid, "beast": beast, "crab": crab, "blob": blob, "floater": floater, "golem": golem}

# ---------------------------------------------------------------- the bestiary
SPECS = {
    # B2 Mycelial Hollow
    "sporeling": dict(plan="blob", body="flesh", kind="mushroom", cap="pink", size=5.5, eye="k"),
    "mire_toad": dict(plan="blob", body="moss", size=5.5, eye="c", kind="maw"),
    "fungal_shambler": dict(plan="humanoid", body="moss", skin="flesh", head="cap", hood="violet", weapon="claws", width=5, eye="r", height=1.05),
    # B3 Drowned Cistern
    "brine_crab": dict(plan="crab", body="red", claws="ember", eye="k", pattern="h"),
    "drowned_sailor": dict(plan="humanoid", body="teal", skin="pale", head="fish", hood="teal", weapon="sword", belt="6", eye="n"),
    "eelshade": dict(plan="floater", body="night", kind="eel", eye="n"),
    # B4 Ember Forge
    "cinder_imp": dict(plan="humanoid", body="ember", skin="red", head="plain", horns="dark", tail="ember", weapon="claws", claw="d", eye="d", height=0.8, headsize=1.1, bare_arms=True),
    "slag_golem": dict(plan="golem", body="slate", core="gold", eye="c"),
    "forge_sentry": dict(plan="humanoid", body="brass", head="helm", helm="brass", weapon="spear", eye="g", width=4),
    # B5 Frostbone Caverns
    "frost_wisp": dict(plan="floater", body="ice", kind="wisp", eye="G", spark="H"),
    "rime_wolf": dict(plan="beast", body="pale", legs="steel", eye="l", spikes="ice"),
    "ice_revenant": dict(plan="humanoid", body="ice", head="skull", weapon="sword", eye="l", belt="j"),
    # B6 Thornwild Ruins
    "briar_crawler": dict(plan="crab", body="green", eye="g", pattern="f", width=6),
    "thorn_dryad": dict(plan="humanoid", body="moss", skin="moss", head="hood", hood="green", robe=True, weapon="staff", orb="red", eye="v", aura="v", trim="f"),
    "root_horror": dict(plan="golem", body="wood", core="green", vines="t", eye="v"),
    # B7 Clockwork Ossuary
    "gear_beetle": dict(plan="crab", body="brass", gear="d", eye="g", width=6),
    "brass_sentinel": dict(plan="golem", body="brass", core="ice", rivets="d", eye="m"),
    "tick_bomb": dict(plan="blob", body="dark", kind="bomb", size=6, eye="g"),
    # B8 Sandsunk Necropolis
    "sand_wraith": dict(plan="floater", body="sand", kind="wraith", eye="d", arms=True),
    "scarab_swarm": dict(plan="crab", body="blue", eye="c", pattern="c", width=5),
    "mummified_priest": dict(plan="humanoid", body="sand", head="wrapped", robe=True, weapon="staff", orb="gold", eye="l", aura="c", trim="j", mantle="blue"),
    # B9 Glass Abyss
    "shard_sprite": dict(plan="floater", body="glass", kind="shard", spark="n"),
    "mirror_knight": dict(plan="humanoid", body="glass", head="helm", helm="glass", weapon="sword", eye="r", belt="q"),
    "prism_eye": dict(plan="floater", body="pale", kind="eye", iris="violet"),
    # B10 Heart of Dusk
    "hollow_acolyte": dict(plan="humanoid", body="crimson", head="hood", hood="crimson", robe=True, weapon="staff", orb="red", eye="g", aura="g", trim="c"),
    "dusk_hound": dict(plan="beast", body="night", eye="g", mane="crimson"),
    "hunger_maw": dict(plan="blob", body="crimson", kind="maw", size=6, eye="d"),

    # ---- bosses (64x64, unit 2) ----
    "mother_morel": dict(boss=True, plan="humanoid", body="violet", skin="flesh", head="cap", hood="pink", robe=True, weapon="staff", orb="moss", eye="k", width=5.5, aura="v", trim="u", mantle="moss", headsize=1.3),
    "tidecaller": dict(boss=True, plan="humanoid", body="blue", skin="pale", head="fish", hood="teal", robe=True, weapon="spear", eye="n", crown=True, width=5, trim="n", mantle="teal", headsize=1.2),
    "furnace_saint": dict(boss=True, plan="golem", body="slate", core="gold", rivets="c", eye="d", cracks="g", spikes="ember"),
    "pale_matriarch": dict(boss=True, plan="humanoid", body="ice", head="skull", skullramp="pale", robe=True, weapon="staff", orb="ice", eye="l", crown=True, aura="n", trim="H", mantle="pale", headsize=1.2),
    "verdant_hierophant": dict(boss=True, plan="golem", body="wood", core="red", vines="t", eye="g", spikes="green", cracks="u"),
    "horologist": dict(boss=True, plan="humanoid", body="slate", head="mask", mask="clock", hood="gold", robe=True, weapon="staff", orb="gold", eye="k", aura="d", trim="c", mantle="brass", headsize=1.2),
    "dune_pharaoh": dict(boss=True, plan="humanoid", body="blue", legs="gold", head="mask", mask="pharaoh", hood="gold", weapon="hammer", hammer="gold", crown=True, eye="l", belt="c", width=5, mantle="gold", headsize=1.2),
    "mirrored_choir": dict(boss=True, plan="choir", body="glass"),
    "the_cantor": dict(boss=True, plan="humanoid", body="night", head="hood", hood="crimson", robe=True, weapon="staff", orb="red", eye="d", crown=True, aura="g", width=5.5, trim="c", mantle="crimson", headsize=1.25),
}


def frames_for(name):
    s = SPECS[name]
    boss = s.get("boss", False)
    size, u, tags = (64, 2, BOSS) if boss else (32, 1, REG)
    out = []
    for tag, n in tags:
        for i in range(n):
            p = Pose(tag, i, n)
            c = Canvas(size, size)
            PLANS[s["plan"]](c, p, s, u)
            c.outline("k")
            collapse(c, p, size - 2)
            out.append([tag, c.rows()])
    return out, size, [t for t, _ in tags]


if __name__ == "__main__":
    if sys.argv[1] == "--list":
        print(" ".join(SPECS))
        sys.exit()
    frames, size, tags = frames_for(sys.argv[1])
    print(json.dumps({"legend": L, "frames": frames, "size": size, "tags": tags}))
