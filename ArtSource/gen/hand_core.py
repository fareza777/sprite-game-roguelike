"""Hand-drawn bestiary core. A creature is a stack of hand-authored pixel grids (parts), each with a few
states, posed per frame by a plan table. Frame layout matches bestiary.py (regular 16 / boss 20 frames).

A part is {state: (rows, ox, oy)}; rows use the bestiary legend, '.' is transparent. Grids are drawn without
their outer outline: the canvas outlines the whole creature in 'k' at the end, like the hand-made skeleton."""
import sys

sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
from compose import Canvas  # noqa: E402

REG = [("idle", 2), ("run", 4), ("windup", 2), ("attack", 3), ("hurt", 1), ("death", 4)]
BOSS = [("idle", 2), ("run", 4), ("windup", 2), ("attack", 3), ("cast", 3), ("hurt", 1), ("death", 5)]
UPPER = ("back", "armback", "body", "head", "arm", "fx")


def part(**states):
    """part(rest=(rows, ox, oy), up=(rows, ox, oy), ...). A bare rows list means offset (0, 0)."""
    return {k: (v if isinstance(v, tuple) else (v, 0, 0)) for k, v in states.items()}


class Creature:
    def __init__(self, layers, plan, size=32, boss=False, overlays=None):
        """layers: [(group, part)] in draw order. plan: (tag, i) -> {group: (state, dx, dy), 'all': (dx, dy)}."""
        self.layers, self.plan, self.size, self.boss = layers, plan, size, boss
        self.overlays = overlays or []

    def compose(self, spec):
        c = Canvas(self.size, self.size)
        u = 2 if self.boss else 1   # bosses are drawn at twice the resolution, so poses move twice as far
        gx, gy = spec.get("all", (0, 0))
        gx, gy = gx * u, gy * u
        for group, prt in self.layers:
            st, dx, dy = spec.get(group, ("rest", 0, 0))
            dx, dy = dx * u, dy * u
            if st is None:
                continue
            rows, ox, oy = prt.get(st) or prt.get("rest") or (None, 0, 0)
            if rows is None:
                continue
            c.paste(rows, ox + dx + gx, oy + dy + gy)
        for fn in self.overlays:
            fn(c, spec)
        c.outline("k")
        return c

    def frames(self):
        out = []
        tags = BOSS if self.boss else REG
        for tag, n in tags:
            for i in range(n):
                if tag == "death":
                    c = self.compose(self.plan("hurt", 0))
                    fall(c, (i + 1) / n, self.size)
                else:
                    c = self.compose(self.plan(tag, i))
                out.append([tag, c.rows()])
        return out


def fall(c, t, size):
    """Death: sag toward the floor, then crumble away in a dither."""
    floor = size - 2
    k = 1.0 - 0.62 * min(1.0, t * 1.25)
    src = [row[:] for row in c.g]
    for y in range(c.h):
        for x in range(c.w):
            c.g[y][x] = "."
    for y in range(c.h):
        sy = int(round(floor - (floor - y) / k)) if k > 0 else -1
        if 0 <= sy < c.h:
            for x in range(c.w):
                c.g[y][x] = src[sy][x]
    if t > 0.5:
        gone = (t - 0.5) * 1.7
        for y in range(c.h):
            for x in range(c.w):
                if c.g[y][x] != "." and BAYER[y % 4][x % 4] < gone * 16:
                    c.g[y][x] = "."


BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]


def squash(rows, at):
    """Drop row `at` (a plain body row) so the shape sits one pixel lower."""
    return rows[:at] + rows[at + 1:]


def stretch(rows, at):
    """Repeat row `at` so the shape stands one pixel taller."""
    return rows[:at + 1] + rows[at:]


def limbs(legs, height):
    """Compose a grid of legs: legs = [(x, slant, leg_rows)]; row r of a leg shifts by slant * r / (height - 1),
    so the hip stays put and the foot swings. Later legs draw over earlier ones (far legs first)."""
    out = {}
    for x, slant, rows in legs:
        top = height - len(rows)
        for r, line in enumerate(rows):
            y = top + r
            sx = x + round(slant * y / max(1, height - 1))
            for i, ch in enumerate(line):
                if ch != ".":
                    out[(sx + i, y)] = ch
    w = max(px for px, _ in out) + 1
    return ["".join(out.get((i, y), ".") for i in range(w)) for y in range(height)]


def recolor(rows, mapping):
    """Copy of a grid with colour keys swapped: mapping = {old: new}."""
    return ["".join(mapping.get(ch, ch) for ch in line) for line in rows]


def merge(items):
    """Combine several grids into one: items = [(x, y, rows)], later items draw on top."""
    out = {}
    for x, y, rows in items:
        for j, line in enumerate(rows):
            for i, ch in enumerate(line):
                if ch != ".":
                    out[(x + i, y + j)] = ch
    w = max(px for px, _ in out) + 1
    h = max(py for _, py in out) + 1
    return ["".join(out.get((i, j), ".") for i in range(w)) for j in range(h)]


def sway(rows, amount):
    """Shear a hem or tail: the top row stays, the bottom row shifts by `amount` pixels."""
    n = max(1, len(rows) - 1)
    out = []
    for r, line in enumerate(rows):
        d = round(amount * r / n)
        out.append("." * d + line if d >= 0 else line[-d:])
    return out


def mirror(rows):
    """Flip a grid left to right."""
    w = max(len(r) for r in rows)
    return [r.ljust(w, ".")[::-1] for r in rows]


def swap(rows, changes):
    """Copy of rows with some rows replaced: {index: new_row}."""
    return [changes.get(i, r) for i, r in enumerate(rows)]


def upper(dx, dy, **states):
    """Move every upper-body group by (dx, dy); states overrides the state of named groups."""
    spec = {g: (states.get(g, "rest"), dx, dy) for g in UPPER}
    return spec


def biped(tag, i):
    """Upright walker: legs stay planted while the upper body breathes, leans and strikes."""
    if tag == "idle":
        return upper(0, i)
    if tag == "run":
        legs = ("a", "rest", "b", "rest")[i]
        s = upper(1, 1 if legs != "rest" else 0)
        s["legs"] = (legs, 1, 0)
        return s
    if tag == "windup":
        s = upper(-1 - i, i, arm="up", armback="up")
        s["legs"] = ("rest", -1, 0)
        return s
    if tag == "attack":
        s = upper((2, 2, 1)[i], 0, arm=("fwd", "fwd", "follow")[i], fx=("smear", "rest", "rest")[i])
        s["legs"] = ("b" if i < 2 else "rest", 1, 0)
        return s
    if tag == "cast":
        s = upper(0, -(i % 2), arm="up", armback="up", fx="cast%d" % i)
        return s
    if tag == "hurt":
        s = upper(-2, 0, head="hurt", arm="rest")
        s["legs"] = ("rest", -1, 0)
        return s
    return {}


def floater(tag, i):
    """Hovers above the floor: the run cycle is a bob, attacks are a lunge."""
    bob = {"idle": (0, 1), "run": (0, -1, -1, 0)}.get(tag)
    if tag in ("idle", "run"):
        dy = bob[i % len(bob)]
        return {"all": (1 if tag == "run" else 0, dy), "tail": ("a" if i % 2 == 0 else "b", 0, 0)}
    if tag == "windup":
        return {"all": (-1 - i, -1), "body": ("up", 0, 0), "tail": ("b", 0, 0)}
    if tag == "attack":
        return {"all": ((3, 2, 1)[i], 0), "body": ("fwd", 0, 0), "fx": (("smear", "rest", "rest")[i], 0, 0), "tail": ("a", 0, 0)}
    if tag == "cast":
        return {"all": (0, -(i % 2)), "body": ("up", 0, 0), "fx": ("cast%d" % i, 0, 0)}
    if tag == "hurt":
        return {"all": (-2, 0), "body": ("hurt", 0, 0), "tail": ("b", 0, 0)}
    return {}


def crawler(tag, i):
    """Low many-legged body; legs scuttle, claws or mandibles do the work."""
    def lift(dy, arm="rest", back="rest", head="rest"):
        return {"body": ("rest", 0, dy), "arm": (arm, 0, dy), "back": (back, 0, dy), "head": (head, 0, dy)}
    if tag == "idle":
        return lift(i)
    if tag == "run":
        s = lift(i % 2)
        s.update({"all": (1, 0), "legs": (("a", "b", "a", "b")[i], 0, 0)})
        return s
    if tag == "windup":
        s = lift(1, arm="up", back="up")
        s["all"] = (-1 - i, 0)
        return s
    if tag == "attack":
        s = lift(0, arm=("fwd", "fwd", "rest")[i])
        s.update({"all": ((3, 2, 1)[i], 0), "fx": (("smear", "rest", "rest")[i], 0, 0), "legs": ("b", 0, 0)})
        return s
    if tag == "hurt":
        s = lift(1, head="hurt")
        s["all"] = (-2, 0)
        return s
    return {}


def blob(tag, i):
    """Squash-and-stretch body: the whole shape swaps grids."""
    if tag == "idle":
        return {"body": (("rest", "squat")[i], 0, 0)}
    if tag == "run":
        return {"all": (1, 0), "body": (("squat", "rest", "tall", "rest")[i], 0, 0)}
    if tag == "windup":
        return {"all": (-1 - i, 0), "body": ("squat", 0, 0)}
    if tag == "attack":
        return {"all": ((3, 2, 1)[i], 0), "body": (("fwd", "fwd", "rest")[i], 0, 0), "fx": (("smear", "rest", "rest")[i], 0, 0)}
    if tag == "hurt":
        return {"all": (-2, 0), "body": ("hurt", 0, 0)}
    return {}


def quad(tag, i):
    """Four-legged runner seen from the side, facing right."""
    if tag == "idle":
        return {"body": ("rest", 0, i), "head": ("rest", 0, i), "back": ("rest", 0, i)}
    if tag == "run":
        legs = ("a", "b", "c", "b")[i]
        dy = (0, 1, 0, 1)[i]
        return {"all": (1, 0), "legs": (legs, 0, 0), "body": ("rest", 0, dy), "head": ("rest", 0, dy), "back": ("a" if i % 2 else "rest", 0, dy)}
    if tag == "windup":
        return {"all": (-1 - i, 0), "legs": ("crouch", 0, 0), "body": ("rest", 0, 1), "head": ("up", 0, 1), "back": ("rest", 0, 1)}
    if tag == "attack":
        return {"all": ((3, 2, 1)[i], 0), "legs": ("c", 0, 0), "head": (("fwd", "fwd", "rest")[i], 0, 0), "fx": (("smear", "rest", "rest")[i], 0, 0)}
    if tag == "hurt":
        return {"all": (-2, 0), "head": ("hurt", 0, 1), "body": ("rest", 0, 1), "back": ("rest", 0, 1)}
    return {}


PLANS = {"biped": biped, "floater": floater, "crawler": crawler, "blob": blob, "quad": quad}
