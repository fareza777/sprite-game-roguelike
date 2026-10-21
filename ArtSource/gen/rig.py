"""Shared pixel-rig toolkit: shape sets, rim-lit shading with separation lines, outlines, sweeps.
Used by the hero/enemy/boss generators. All shapes are sets of (x, y) integer pixels."""
import math

import sys
sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
from compose import Canvas  # noqa: E402


def ellipse(cx, cy, rx, ry):
    return {(x, y) for y in range(int(cy - ry) - 1, int(cy + ry) + 2)
            for x in range(int(cx - rx) - 1, int(cx + rx) + 2)
            if ((x - cx) / max(rx, .5)) ** 2 + ((y - cy) / max(ry, .5)) ** 2 <= 1.0}


def poly(pts):
    xs = [p[0] for p in pts]; ys = [p[1] for p in pts]
    out = set()
    for y in range(int(min(ys)), int(max(ys)) + 1):
        for x in range(int(min(xs)), int(max(xs)) + 1):
            px, py, inside, j = x + .5, y + .5, False, len(pts) - 1
            for i in range(len(pts)):
                (xi, yi), (xj, yj) = pts[i], pts[j]
                if (yi > py) != (yj > py) and px < (xj - xi) * (py - yi) / (yj - yi) + xi:
                    inside = not inside
                j = i
            if inside:
                out.add((x, y))
    return out


def thick(x0, y0, x1, y1, w):
    out = set()
    n = max(abs(x1 - x0), abs(y1 - y0), 1)
    r = w / 2
    for i in range(int(n * 2) + 1):
        t = i / (n * 2)
        cx, cy = x0 + (x1 - x0) * t, y0 + (y1 - y0) * t
        for y in range(int(cy - r) - 1, int(cy + r) + 2):
            for x in range(int(cx - r) - 1, int(cx + r) + 2):
                if (x - cx) ** 2 + (y - cy) ** 2 <= r * r:
                    out.add((x, y))
    return out


def rect(x, y, w, h):
    return {(i, j) for i in range(int(x), int(x + w)) for j in range(int(y), int(y + h))}


def put(c, x, y, ch):
    x, y = int(x), int(y)
    if 0 <= x < c.w and 0 <= y < c.h:
        c.g[y][x] = ch


def fill(c, pts, ch):
    for x, y in pts:
        put(c, x, y, ch)


def shade(c, pts, ramp, sep=True, light=(-1, -1)):
    """Rim-lit fill. ramp = (deep, dark, base, light). Light comes from the top-left by default.
    sep draws a 1px dark line where the part overlaps something drawn earlier."""
    deep, dark, base, lit = ramp
    lx, ly = light
    if sep:
        for x, y in pts:
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                p = (x + dx, y + dy)
                if p not in pts and 0 <= p[0] < c.w and 0 <= p[1] < c.h and c.g[p[1]][p[0]] != '.':
                    put(c, p[0], p[1], 'k')
    for x, y in pts:
        if (x - lx, y) not in pts or (x, y - ly) not in pts:
            ch = deep
        elif (x - 2 * lx, y - ly) not in pts or (x - lx, y - 2 * ly) not in pts:
            ch = dark
        elif (x + lx, y) not in pts or (x, y + ly) not in pts:
            ch = lit
        else:
            ch = base
        put(c, x, y, ch)


def sweep(c, cx, cy, r0, r1, a0, a1, outer='H', inner='S'):
    """Crescent swing trail from angle a0 (hairline) to a1 (full width), drawn only on empty pixels."""
    for y in range(c.h):
        for x in range(c.w):
            a = math.atan2(y - cy, x - cx)
            if not (a0 <= a <= a1) or c.g[y][x] != '.':
                continue
            t = (a - a0) / (a1 - a0)
            rin = r1 - 1.5 - (r1 - r0 - 1.5) * t * t
            d = math.hypot(x - cx, y - cy)
            if rin <= d <= r1:
                put(c, x, y, outer if d > r1 - 1.6 else inner)


def glow(c, cx, cy, r, chars):
    """Concentric dithered glow; chars from outer to inner."""
    for i, ch in enumerate(chars):
        rr = r * (1 - i / len(chars))
        for x, y in ellipse(cx, cy, rr, rr):
            if i == 0 and (x + y) % 2:
                continue
            put(c, x, y, ch)


def finish(c, pad_top=0):
    c.outline('k')
    return c.rows()
