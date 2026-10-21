"""Cinematic panels for UNDERSUN (160x120 each): prologue pro_1..pro_6 and ending end_1..end_4.
Silhouette storytelling in the Apollo palette. Prints JSON {legend, frames:[[id, rows]]}."""
import json
import math
import random

W, H = 160, 120
L = {"0": "#090a14", "1": "#10141f", "2": "#151d28", "3": "#202e37", "4": "#394a50", "5": "#577277",
     "6": "#819796", "7": "#c7cfcc", "8": "#ebede9", "v": "#1e1d39", "p": "#402751", "P": "#7a367b",
     "b": "#253a5e", "B": "#3c5e8b", "c": "#4f8fba", "C": "#73bed3", "q": "#341c27", "Q": "#4d2b32",
     "r": "#752438", "R": "#a53030", "E": "#cf573c", "e": "#da863e", "o": "#be772b", "G": "#de9e41",
     "y": "#e8c170", "g": "#25562e", "n": "#468232", "N": "#75a743", "s": "#a8b5b2", "t": "#7a4841",
     "T": "#ad7757"}

BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]


class Panel:
    def __init__(self, seed):
        self.g = [["0"] * W for _ in range(H)]
        self.rnd = random.Random(seed)

    def put(self, x, y, ch):
        x, y = int(x), int(y)
        if 0 <= x < W and 0 <= y < H:
            self.g[y][x] = ch

    def get(self, x, y):
        return self.g[int(y)][int(x)] if 0 <= x < W and 0 <= y < H else "0"

    def sky(self, colors, y0=0, y1=H):
        n = len(colors) - 1
        for y in range(y0, y1):
            t = (y - y0) / max(1, y1 - y0 - 1) * n
            i = min(int(t), n - 1)
            f = t - i
            for x in range(W):
                self.put(x, y, colors[i + 1] if BAYER[y % 4][x % 4] / 16 < f else colors[i])

    def stars(self, count, ymax, chars="567"):
        for _ in range(count):
            self.put(self.rnd.randrange(W), self.rnd.randrange(ymax), self.rnd.choice(chars))

    def disc(self, cx, cy, r, rings, halo=None):
        """Concentric bands: rings = [(radius_fraction, char), ...] outer->inner handled by sorting."""
        for y in range(int(cy - r - 8), int(cy + r + 9)):
            for x in range(int(cx - r - 8), int(cx + r + 9)):
                d = math.hypot(x - cx, y - cy)
                if halo and r < d <= r + halo[0] and BAYER[y % 4][x % 4] / 16 < (1 - (d - r) / halo[0]) * 0.8:
                    self.put(x, y, halo[1])
                if d <= r:
                    ch = rings[-1][1]
                    for frac, c in rings:
                        if d <= r * frac:
                            ch = c
                    self.put(x, y, ch)

    def sun(self, cx, cy, r, halo=8):
        self.disc(cx, cy, r, [(1.0, "o"), (0.88, "G"), (0.7, "y"), (0.4, "8")], (halo, "e"))

    def ridge(self, base, amp, ch, freq=0.08, seed=0):
        for x in range(W):
            h = base - amp * (0.5 + 0.5 * math.sin(x * freq + seed)) - amp * 0.4 * math.sin(x * freq * 2.7 + seed * 3)
            for y in range(int(h), H):
                self.put(x, y, ch)

    def figure(self, x, base, h=12, ch="0", hood=True, lantern=False, cape=True, arms_up=False):
        """Robed silhouette standing on `base`, centred at x."""
        w = max(3, h // 3)
        for y in range(base - h + 3, base):
            spread = (y - (base - h + 3)) / max(1, h - 3)
            half = int(w / 2 + spread * (w / 2 if cape else 1))
            for dx in range(-half, half + 1):
                self.put(x + dx, y, ch)
        for dy in range(4):
            for dx in range(-1, 2):
                self.put(x + dx, base - h + dy, ch)
        if hood:
            self.put(x, base - h - 1, ch)
        if arms_up:
            for i in range(4):
                self.put(x - 2 - i // 2, base - h + 4 - i, ch)
                self.put(x + 2 + i // 2, base - h + 4 - i, ch)
        if lantern:
            self.put(x + w // 2 + 2, base - h + 5, "G")
            self.put(x + w // 2 + 2, base - h + 6, "y")
            self.put(x + w // 2 + 1, base - h + 4, ch)

    def tower(self, x0, w, top, base, ch="1", lit=0.5, spire=True):
        for x in range(x0, x0 + w):
            for y in range(top, base):
                self.put(x, y, ch)
        if spire:
            for i in range(w // 2 + 1):
                for x in range(x0 + i, x0 + w - i):
                    self.put(x, top - i - 1, ch)
        for _ in range(max(1, int(w * (base - top) * lit / 18))):
            self.put(self.rnd.randrange(x0 + 1, max(x0 + 2, x0 + w - 1)), self.rnd.randrange(top + 1, max(top + 2, base - 1)),
                     self.rnd.choice("yGo"))

    def city(self, base, height=26, ch="1", lit=0.5):
        x = 0
        while x < W:
            w = self.rnd.randint(4, 10)
            top = base - self.rnd.randint(height // 3, height)
            self.tower(x, w, top, base, ch, lit, spire=self.rnd.random() < 0.5)
            x += w + self.rnd.randint(0, 2)

    def rays(self, cx, cy, angles, length, ch, every=1):
        for a in angles:
            ra = math.radians(a)
            for t in range(0, length, every):
                x, y = cx + math.cos(ra) * t, cy + math.sin(ra) * t
                if BAYER[int(y) % 4][int(x) % 4] < 9:
                    self.put(x, y, ch)

    def frame(self):
        for x in range(W):
            self.put(x, 0, "0"); self.put(x, H - 1, "0")
        for y in range(H):
            self.put(0, y, "0"); self.put(W - 1, y, "0")

    def rows(self):
        self.frame()
        return ["".join(r) for r in self.g]


# ---------------- prologue ----------------

def pro_1():
    """The Hunger begins to eat the sun."""
    p = Panel(1)
    p.sky(["v", "p", "P", "r", "E"], 0, 92)
    p.stars(40, 50)
    p.sun(96, 46, 22, halo=10)
    # the Hunger: a black maw swallowing the sun's right side, tendrils clawing across the sky
    for y in range(10, 90):
        for x in range(90, W):
            d = math.hypot(x - 128, y - 44)
            edge = 26 + 5 * math.sin(y * 0.45) + 3 * math.sin(y * 1.3)
            if d < edge:
                p.put(x, y, "0" if d < edge - 2 else "v")
    for k in range(9):
        a = math.radians(150 + k * 14)
        x, y = 128.0, 44.0
        for t in range(70):
            x += math.cos(a) * 1.1 + p.rnd.uniform(-0.4, 0.4)
            y += math.sin(a) * 1.1 + p.rnd.uniform(-0.4, 0.4)
            a += 0.02 * math.sin(t * 0.3 + k)
            p.put(x, y, "0"); p.put(x, y + 1, "v" if t > 45 else "0")
    for x, y in ((118, 30), (122, 52), (116, 44)):  # teeth-like glints on the maw's lip
        p.put(x, y, "7")
    p.ridge(98, 8, "2", 0.06, 1)
    p.ridge(104, 5, "1", 0.11, 2)
    p.city(104, 12, "0", 0.25)
    return p.rows()


def pro_2():
    """The Choir of Dawn sings the sun down into the earth."""
    p = Panel(2)
    p.sky(["0", "v", "p", "Q", "E", "e"], 0, 104)
    p.stars(25, 40)
    p.sun(80, 100, 24, halo=14)          # sinking below the horizon
    # golden hymn lines pulling the sun downward
    for k in range(5):
        for x in range(W):
            y = 40 + k * 7 + 3 * math.sin(x * 0.12 + k)
            if BAYER[int(y) % 4][x % 4] < 5:
                p.put(x, y, "G" if k % 2 else "y")
    # ten choristers on the ridge, arms raised, haloed - silhouetted against the dying light
    for i in range(10):
        x = 10 + i * 15
        base = 104
        p.figure(x, base, h=18, ch="0", arms_up=True)
        for dx in (-1, 0, 1):
            p.put(x + dx, base - 21, "y")
    for x in range(W):
        for y in range(104, H):
            p.put(x, y, "0")
    return p.rows()


def pro_3():
    """Ten vaults, a chorister in each, the sun sealed at the bottom (cross-section)."""
    p = Panel(3)
    p.sky(["v", "b"], 0, 12)
    p.city(12, 8, "1", 0.4)
    colors = ["3", "g", "c", "R", "7", "n", "o", "T", "C", "P"]  # one tint per verse
    for i in range(10):
        y0 = 13 + i * 9
        half = 74 - i * 6
        for y in range(y0, y0 + 9):
            for x in range(W):
                inside = abs(x - 80) < half - (y - y0) * 0.6
                p.put(x, y, "2" if inside else "1")
            p.put(80 - half + 2, y, "3"); p.put(80 + half - 2, y, "3")
        # chamber floor line in the verse colour + a glowing chorister
        for x in range(80 - half + 4, 80 + half - 4):
            p.put(x, y0 + 8, colors[i])
        p.put(80, y0 + 6, "0"); p.put(80, y0 + 7, "0"); p.put(80, y0 + 5, colors[i])
    p.sun(80, 124, 14, halo=6)
    return p.rows()


def pro_4():
    """Emberwick at dusk; the lanterns are failing."""
    p = Panel(4)
    p.sky(["0", "v", "p", "Q", "q"], 0, 96)
    p.stars(55, 60)
    p.city(98, 40, "1", 0.35)
    p.city(110, 18, "0", 0.15)
    for x in range(W):
        for y in range(110, H):
            p.put(x, y, "0")
    # a great lantern in the foreground, its flame guttering
    lx, ly = 32, 30
    for y in range(ly, H):
        p.put(lx, y, "0"); p.put(lx + 1, y, "0")
    p.disc(lx, ly + 18, 16, [(1.0, "q")], (6, "Q"))     # the last of its glow
    for y in range(ly + 6, ly + 30):
        for x in range(lx - 10, lx + 12):
            frame_px = abs(x - lx - 1) >= 10 or y in (ly + 6, ly + 29) or (x - lx) % 7 == 0
            p.put(x, y, "0" if frame_px else "v")
    for (dx, dy, ch) in ((1, 22, "E"), (1, 21, "e"), (1, 20, "R"), (0, 23, "r"), (2, 23, "r"), (1, 23, "G")):
        p.put(lx + dx, ly + dy, ch)
    for i in range(16):  # smoke curling up from the guttering wick
        p.put(lx + 1 + int(math.sin(i * 0.6) * 3), ly + 4 - i, "4" if i % 2 else "3")
    return p.rows()


def pro_5():
    """A lone Unsung at the chasm's edge, the glow far below."""
    p = Panel(5)
    p.sky(["0", "v", "b", "B"], 0, 70)
    p.stars(45, 50)
    for a in range(0, 360, 3):  # the empty ring where the sun was
        p.put(110 + math.cos(math.radians(a)) * 17, 26 + math.sin(math.radians(a)) * 17, "5")
    # chasm glow
    for y in range(64, H):
        for x in range(W):
            d = math.hypot((x - 92) * 1.4, y - 150)
            ch = "y" if d < 40 else "G" if d < 52 else "e" if d < 64 else "E" if d < 76 else "r" if d < 88 else "q" if d < 100 else "0"
            p.put(x, y, ch)
    # cliff on the left with the figure
    for y in range(66, H):
        edge = 58 - (y - 66) * 0.25 + 3 * math.sin(y * 0.4)
        for x in range(0, int(edge)):
            p.put(x, y, "1" if x > edge - 3 else "0")
    p.figure(46, 66, h=16, ch="0", lantern=True)
    for y in range(66, H):  # right wall
        edge = 132 + (y - 66) * 0.2 + 3 * math.sin(y * 0.3 + 1)
        for x in range(int(edge), W):
            p.put(x, y, "1" if x < edge + 3 else "0")
    return p.rows()


def pro_6():
    """Descend: the stairway into the deep, lit from below."""
    p = Panel(6)
    for y in range(H):
        for x in range(W):
            d = math.hypot(x - 80, (y - 170) * 0.8)
            ch = "y" if d < 45 else "G" if d < 60 else "e" if d < 75 else "E" if d < 90 else "r" if d < 106 else "Q" if d < 122 else "q" if d < 138 else "0"
            p.put(x, y, ch)
    # spiral stair: steps alternating sides, getting smaller with depth
    for k in range(9):
        y = 18 + k * 11
        w = 40 - k * 3
        cx = 80 + (24 if k % 2 else -24) * (1 - k / 12)
        for x in range(int(cx - w / 2), int(cx + w / 2)):
            p.put(x, y, "2"); p.put(x, y + 1, "1"); p.put(x, y + 2, "0")
    for y in range(H):  # framing walls
        for x in range(0, 14 - y // 20):
            p.put(x, y, "0")
        for x in range(W - 14 + y // 20, W):
            p.put(x, y, "0")
    p.figure(62, 29, h=11, ch="0", lantern=True)
    return p.rows()


# ---------------- ending ----------------

def end_1():
    """The hymn tablet splits; light bursts through the crack - the hymn is over."""
    p = Panel(11)
    p.sky(["0", "v", "0"], 0, H)
    p.rays(80, 60, range(0, 360, 12), 80, "Q")
    tx0, tx1, ty0, ty1 = 46, 114, 14, 108
    crack = [80 + int(4 * math.sin(y * 0.5) + 2 * math.sin(y * 1.7)) for y in range(H)]
    for y in range(ty0, ty1):
        for x in range(tx0, tx1):
            if abs(x - crack[y]) <= 1:
                p.put(x, y, "8" if x == crack[y] else "y")
                continue
            shift = -2 if x < crack[y] else 2      # the halves drifting apart
            ch = "4" if (x + y) % 7 else "3"
            if x in (tx0, tx1 - 1) or y in (ty0, ty1 - 1): ch = "2"
            p.put(x + shift, y, ch)
    for row in range(8):  # engraved verse lines
        y = 24 + row * 10
        for x in range(tx0 + 6, tx1 - 6):
            if abs(x - crack[y]) > 2 and (x // 3 + row) % 5:
                p.put(x + (-2 if x < crack[y] else 2), y, "1")
    for k in range(18):  # sparks escaping the crack
        y = p.rnd.randrange(ty0, ty1)
        p.put(crack[y] + p.rnd.choice((-1, 1)) * p.rnd.randint(4, 30), y + p.rnd.randint(-6, 6), p.rnd.choice("yG8"))
    return p.rows()


def end_2():
    """The sun rises up through all ten vaults."""
    p = Panel(12)
    bands = ["3", "g", "c", "R", "7", "n", "o", "T", "C", "P"]
    for y in range(H):
        for x in range(W):
            p.put(x, y, bands[min(9, y // 12)] if (x + y) % 3 else "1")
    for y in range(H):
        for x in range(W):
            half = 14 + (H - y) * 0.12
            if abs(x - 80) < half:
                p.put(x, y, "8" if abs(x - 80) < half * 0.4 else "y" if abs(x - 80) < half * 0.75 else "G")
    p.sun(80, 52, 18, halo=10)
    p.rays(80, 52, range(200, 341, 14), 70, "y")
    return p.rows()


def end_3():
    """Dawn over Emberwick; the people look up."""
    p = Panel(13)
    p.sky(["B", "c", "e", "G", "y"], 0, 96)
    p.sun(80, 92, 26, halo=14)
    p.rays(80, 92, range(190, 351, 10), 90, "y")
    p.city(100, 34, "t", 0.6)
    for x in range(W):
        for y in range(100, H):
            p.put(x, y, "t" if y < 104 else "q")
    for i in range(14):
        p.figure(8 + i * 11, 116, h=8, ch="0", hood=False, cape=False, arms_up=i % 3 == 0)
    return p.rows()


def end_4():
    """The sun in the sky - and in its corona, a mouth."""
    p = Panel(14)
    p.sky(["q", "r", "R", "E", "e"], 0, H)
    p.disc(80, 56, 34, [(1.0, "E")], None)
    # corona flares shaped like teeth around the rim
    for k in range(22):
        a = math.radians(k * (360 / 22))
        for t in range(6):
            r = 34 + t
            if t < (6 if k % 2 else 3):
                p.put(80 + math.cos(a) * r, 56 + math.sin(a) * r, "0" if t > 3 else "r")
    p.sun(80, 56, 30, halo=0)
    # the barest suggestion of a closed mouth across the sun's face
    for x in range(62, 99):
        y = 66 + int(2 * math.sin((x - 62) / 36 * math.pi))
        p.put(x, y, "o")
    for x in range(66, 95, 4):
        p.put(x, 67 + int(2 * math.sin((x - 62) / 36 * math.pi)), "e")
    p.ridge(112, 6, "0", 0.09, 4)
    return p.rows()


PANELS = [("pro_1", pro_1), ("pro_2", pro_2), ("pro_3", pro_3), ("pro_4", pro_4), ("pro_5", pro_5), ("pro_6", pro_6),
          ("end_1", end_1), ("end_2", end_2), ("end_3", end_3), ("end_4", end_4)]

if __name__ == "__main__":
    frames = [(name, fn()) for name, fn in PANELS]
    for name, rows in frames:
        assert len(rows) == H and all(len(r) == W for r in rows), name
    print(json.dumps({"legend": L, "frames": frames}, separators=(",", ":")))
