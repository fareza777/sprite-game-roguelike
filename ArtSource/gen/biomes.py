"""Biome tile sheets: recolours the Sunken Crypt sheet (128x96) into nine more biomes and draws three
biome-specific floor decals into the free cells of row 5 (cols 5..7: decalBiome0..2). Apollo palette only.
usage: python biomes.py <crypt_tiles.png> <out_dir>   -> <out_dir>/biome_<id>_src.png for each biome"""
import math
import os
import sys

import pngio

STONE = ['#090a14', '#10141f', '#151d28', '#202e37', '#394a50', '#577277', '#819796', '#a8b5b2', '#c7cfcc']
MOSS = ['#19332d', '#25562e', '#468232']
BLUE = ['#172038', '#253a5e', '#3c5e8b', '#4f8fba']
BLOOD = ['#341c27', '#411d31', '#602c2c']

# id: (stone ramp x9, moss x3, blue x4, blood x3)
BIOMES = {
    "crypt": (STONE, MOSS, BLUE, BLOOD),
    "hollow": (['#090a14', '#10141f', '#241527', '#1e1d39', '#402751', '#7a367b', '#a23e8c', '#c65197', '#df84a5'],
               ['#25562e', '#468232', '#a8ca58'], ['#253a5e', '#4f8fba', '#73bed3', '#a4dddb'], ['#241527', '#411d31', '#752438']),
    "cistern": (['#090a14', '#10141f', '#172038', '#19332d', '#253a5e', '#3c5e8b', '#577277', '#73bed3', '#a4dddb'],
                ['#19332d', '#25562e', '#468232'], ['#172038', '#3c5e8b', '#4f8fba', '#73bed3'], ['#19332d', '#202e37', '#394a50']),
    "forge": (['#090a14', '#10141f', '#241527', '#341c27', '#4d2b32', '#7a4841', '#884b2b', '#be772b', '#de9e41'],
              ['#a53030', '#cf573c', '#da863e'], ['#602c2c', '#a53030', '#cf573c', '#da863e'], ['#341c27', '#602c2c', '#a53030']),
    "frost": (['#090a14', '#10141f', '#172038', '#253a5e', '#3c5e8b', '#4f8fba', '#73bed3', '#a4dddb', '#ebede9'],
              ['#577277', '#819796', '#a8b5b2'], ['#253a5e', '#4f8fba', '#a4dddb', '#ebede9'], ['#3c5e8b', '#4f8fba', '#73bed3']),
    "thornwild": (['#090a14', '#10141f', '#19332d', '#202e37', '#25562e', '#468232', '#75a743', '#a8ca58', '#d0da91'],
                  ['#752438', '#a53030', '#cf573c'], ['#25562e', '#468232', '#75a743', '#a8ca58'], ['#4d2b32', '#7a4841', '#ad7757']),
    "clockwork": (['#090a14', '#10141f', '#151d28', '#202e37', '#4d2b32', '#884b2b', '#be772b', '#c09473', '#de9e41'],
                  ['#253a5e', '#3c5e8b', '#4f8fba'], ['#884b2b', '#be772b', '#de9e41', '#e8c170'], ['#202e37', '#394a50', '#577277']),
    "necropolis": (['#090a14', '#10141f', '#341c27', '#4d2b32', '#7a4841', '#ad7757', '#c09473', '#d7b594', '#e7d5b3'],
                   ['#253a5e', '#3c5e8b', '#4f8fba'], ['#884b2b', '#be772b', '#de9e41', '#e8c170'], ['#602c2c', '#884b2b', '#be772b']),
    "glass": (['#090a14', '#090a14', '#10141f', '#1e1d39', '#172038', '#253a5e', '#4f8fba', '#a4dddb', '#ebede9'],
              ['#7a367b', '#a23e8c', '#c65197'], ['#3c5e8b', '#4f8fba', '#73bed3', '#a4dddb'], ['#402751', '#7a367b', '#a23e8c']),
    "dusk": (['#090a14', '#090a14', '#241527', '#341c27', '#411d31', '#752438', '#a53030', '#cf573c', '#da863e'],
             ['#402751', '#7a367b', '#a23e8c'], ['#1e1d39', '#402751', '#7a367b', '#c65197'], ['#602c2c', '#a53030', '#cf573c']),
}


def rgb(h):
    h = h.lstrip('#')
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4)) + (255,)


class Cell:
    """16x16 drawing surface at a sheet cell."""

    def __init__(self, px, col, row):
        self.px, self.ox, self.oy = px, col * 16, row * 16
        for y in range(16):
            for x in range(16):
                px[self.oy + y][self.ox + x] = (0, 0, 0, 0)

    def put(self, x, y, color):
        x, y = int(round(x)), int(round(y))
        if 0 <= x < 16 and 0 <= y < 16:
            self.px[self.oy + y][self.ox + x] = rgb(color)

    def disc(self, cx, cy, r, color, ry=None):
        ry = ry or r
        for y in range(16):
            for x in range(16):
                if ((x - cx) / r) ** 2 + ((y - cy) / ry) ** 2 <= 1:
                    self.put(x, y, color)

    def line(self, x0, y0, x1, y1, color):
        n = int(max(abs(x1 - x0), abs(y1 - y0), 1))
        for i in range(n + 1):
            self.put(x0 + (x1 - x0) * i / n, y0 + (y1 - y0) * i / n, color)


# ---------------- biome decals (3 per biome) ----------------
def crypt(cs):
    a, b, c = cs
    for x, y in ((4, 9), (9, 7), (11, 11)):          # bone pile
        a.line(x - 2, y + 1, x + 2, y - 1, '#c7cfcc'); a.put(x - 2, y + 2, '#a8b5b2'); a.put(x + 2, y - 2, '#a8b5b2')
    a.disc(7, 10, 1.6, '#c7cfcc'); a.put(6, 10, '#090a14'); a.put(8, 10, '#090a14')
    for p in ((4, 6), (6, 5), (9, 8), (11, 6), (8, 11), (5, 10)):   # urn shards
        b.put(*p, '#7a4841'); b.put(p[0] + 1, p[1], '#ad7757')
    for i in range(7):                                 # loose chain
        c.put(3 + i * 1.6, 8 + math.sin(i) * 2, '#577277' if i % 2 else '#819796')


def hollow(cs):
    a, b, c = cs
    for x, y, r in ((5, 9, 2.4), (10, 7, 3), (11, 12, 1.8)):   # glowing mushrooms
        a.line(x, y + 1, x, y + r + 1, '#d0da91')
        a.disc(x, y, r, '#a23e8c', r * 0.6); a.put(x - 1, y - 1, '#df84a5')
    for i in range(14):                                 # spore glitter
        b.put((i * 7) % 13 + 1, (i * 5) % 12 + 2, '#a8ca58' if i % 3 else '#73bed3')
    for i in range(12):                                 # fairy ring
        ang = i * math.pi / 6
        c.put(8 + 5 * math.cos(ang), 8 + 4 * math.sin(ang), '#c65197' if i % 2 else '#75a743')


def cistern(cs):
    a, b, c = cs
    a.disc(8, 8, 6, '#253a5e', 4); a.disc(8, 8, 4.5, '#3c5e8b', 3); a.disc(7, 7, 1.5, '#73bed3', 0.8)   # puddle
    for i in range(16):                                 # ripple rings
        ang = i * math.pi / 8
        b.put(8 + 6 * math.cos(ang), 8 + 4 * math.sin(ang), '#4f8fba'); b.put(8 + 3 * math.cos(ang), 8 + 2 * math.sin(ang), '#73bed3')
    for x in range(3, 13, 2):                           # algae strands
        c.line(x, 13, x + (1 if x % 4 else -1), 6 + x % 3, '#468232'); c.put(x, 12, '#25562e')


def forge(cs):
    a, b, c = cs
    pts = [(2, 12), (5, 9), (8, 10), (11, 6), (14, 4)]  # lava vein
    for p, q in zip(pts, pts[1:]):
        a.line(*p, *q, '#cf573c')
    for p in pts[1:-1]:
        a.put(p[0], p[1] - 1, '#e8c170')
    b.disc(8, 9, 4, '#4d2b32', 2.6); b.disc(8, 8, 2.6, '#884b2b', 1.6); b.put(7, 7, '#de9e41')   # slag drip
    for i in range(10):                                 # ember scatter
        c.put((i * 5) % 13 + 2, (i * 7) % 11 + 3, '#da863e' if i % 2 else '#e8c170')


def frost(cs):
    a, b, c = cs
    for x, h in ((5, 6), (8, 9), (11, 5)):              # ice crystals
        a.line(x, 13, x, 13 - h, '#a4dddb'); a.line(x + 1, 13, x + 1, 14 - h, '#73bed3'); a.put(x, 13 - h, '#ebede9')
    for i in range(18):                                 # frost patch
        b.put((i * 7) % 14 + 1, (i * 11) % 13 + 2, '#c7cfcc' if i % 3 else '#ebede9')
    c.line(3, 10, 12, 7, '#c7cfcc'); c.disc(12, 7, 1.4, '#ebede9'); c.put(5, 11, '#a4dddb')      # frozen bone


def thornwild(cs):
    a, b, c = cs
    for i in range(14):                                 # thorn vine
        x, y = 1 + i, 9 + 3 * math.sin(i * 0.7)
        a.put(x, y, '#25562e'); a.put(x, y - 1, '#468232')
        if i % 3 == 0:
            a.put(x, y - 2, '#a53030')
    for x, y in ((4, 6), (10, 9), (7, 12)):             # flowers
        b.disc(x, y, 1.3, '#cf573c'); b.put(x, y, '#e8c170')
    c.line(2, 4, 8, 10, '#7a4841'); c.line(8, 10, 13, 8, '#7a4841'); c.line(8, 10, 9, 14, '#4d2b32')   # roots


def clockwork(cs):
    a, b, c = cs
    for cx, cy, r in ((6, 7, 4), (11, 11, 2.6)):        # gears
        for i in range(16):
            ang = i * math.pi / 8
            rr = r + (0.8 if i % 2 == 0 else 0)
            a.put(cx + rr * math.cos(ang), cy + rr * math.sin(ang), '#be772b')
        a.disc(cx, cy, r * 0.45, '#884b2b')
    for i in range(10):                                 # spring
        b.put(3 + i, 8 + (2 if i % 2 else -2), '#c09473'); b.put(3 + i, 8, '#884b2b')
    c.disc(8, 8, 3, '#de9e41'); c.disc(8, 8, 1.4, '#4d2b32'); c.put(7, 6, '#e8c170')   # cog


def necropolis(cs):
    a, b, c = cs
    for y in (5, 9, 13):                                # sand ripples
        for x in range(1, 15):
            a.put(x, y + math.sin(x * 0.8) * 0.8, '#e7d5b3' if x % 3 else '#d7b594')
    b.disc(8, 9, 3.4, '#253a5e', 2.6); b.disc(8, 8, 2, '#4f8fba', 1.6); b.line(8, 5, 8, 12, '#090a14')   # scarab
    b.put(4, 7, '#253a5e'); b.put(12, 7, '#253a5e'); b.put(4, 11, '#253a5e'); b.put(12, 11, '#253a5e')
    for p in ((4, 6), (7, 4), (10, 8), (5, 11), (11, 12)):   # lapis pottery shards
        c.put(*p, '#3c5e8b'); c.put(p[0] + 1, p[1], '#de9e41')


def glass(cs):
    a, b, c = cs
    for x, y, h in ((4, 12, 6), (8, 13, 9), (12, 11, 5)):   # shards
        a.line(x, y, x + 1, y - h, '#a4dddb'); a.line(x + 1, y, x + 2, y - h + 2, '#4f8fba'); a.put(x + 1, y - h, '#ebede9')
    b.disc(8, 8, 3, '#7a367b'); b.disc(8, 8, 1.8, '#c65197'); b.put(7, 7, '#ebede9')   # prism
    for p, q in (((8, 8), (2, 3)), ((8, 8), (14, 5)), ((8, 8), (5, 14)), ((8, 8), (13, 13))):   # crack star
        c.line(*p, *q, '#73bed3')


def dusk(cs):
    a, b, c = cs
    for k in range(3):                                  # dark veins
        pts = [(1 + k * 2, 14), (5, 10 - k), (9, 9 + k), (14, 3 + k * 3)]
        for p, q in zip(pts, pts[1:]):
            a.line(*p, *q, '#752438' if k else '#a53030')
    b.disc(8, 8, 4, '#411d31', 2.6); b.disc(8, 8, 2.5, '#cf573c', 1.8); b.disc(8, 8, 1, '#090a14'); b.put(7, 7, '#e8c170')   # eye
    for i in range(9):                                  # ichor drops
        c.disc((i * 5) % 12 + 2, (i * 7) % 11 + 3, 0.9, '#a53030')


DECALS = {"crypt": crypt, "hollow": hollow, "cistern": cistern, "forge": forge, "frost": frost,
          "thornwild": thornwild, "clockwork": clockwork, "necropolis": necropolis, "glass": glass, "dusk": dusk}


def build(src, out_dir):
    w, h, base = pngio.read(src)
    for biome, (stone, moss, blue, blood) in BIOMES.items():
        mapping = {}
        for group_src, group_dst in ((STONE, stone), (MOSS, moss), (BLUE, blue), (BLOOD, blood)):
            for s, d in zip(group_src, group_dst):
                mapping[rgb(s)[:3]] = rgb(d)
        px = [[(mapping.get(p[:3], p) if p[3] else p) for p in row] for row in base]
        DECALS[biome]([Cell(px, col, 5) for col in (5, 6, 7)])
        path = os.path.join(out_dir, f"biome_{biome}_src.png")
        pngio.write(path, w, h, px)
        print(path)


if __name__ == "__main__":
    build(sys.argv[1], sys.argv[2])
