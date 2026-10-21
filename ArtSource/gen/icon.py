"""48x48 launcher icon: knight helm (cyan visor, red plume) over a downward sword on a warm-glow crypt background.
Prints JSON {legend, frames:[["icon", rows]]}."""
import json
import math

N = 48
L = {"k": "#10141f", "a": "#151d28", "A": "#202e37", "m": "#394a50", "M": "#577277", "n": "#819796",
     "S": "#a8b5b2", "W": "#c7cfcc", "H": "#ebede9", "Q": "#411d31", "q": "#4d2b32", "r": "#752438",
     "R": "#a53030", "E": "#cf573c", "o": "#be772b", "G": "#de9e41", "y": "#e8c170", "C": "#73bed3",
     "c": "#a4dddb", "b": "#253a5e"}
g = [["k"] * N for _ in range(N)]


def put(x, y, ch):
    if 0 <= x < N and 0 <= y < N:
        g[y][x] = ch


# background: warm torch glow, banded + checker-dithered at band edges
for y in range(N):
    for x in range(N):
        d = math.hypot(x - 23.5, y - 26) / 30
        bands = [(0.35, "q"), (0.55, "Q"), (0.78, "a"), (9, "k")]
        for i, (lim, ch) in enumerate(bands):
            if d < lim:
                edge = False
                put(x, y, bands[i + 1][1] if edge else ch)
                break

# sword (behind): pommel, grip, crossguard, blade to a point
for y in range(3, 6): put(23, y, "G"); put(24, y, "y" if y == 3 else "G")
for y in range(6, 11): put(23, y, "r"); put(24, y, "R")
for x in range(13, 35): put(x, 11, "G"); put(x, 12, "o")
put(13, 11, "y"); put(34, 12, "o")
for y in range(13, 46):
    half = 3 if y < 38 else max(0, (45 - y) // 2)
    for x in range(24 - half, 24 + half):
        put(x, y, "H" if x == 24 - half else "W" if x < 24 else "S")
    if y < 40: put(23, y, "n")

# helm: dome + cheek plates, shaded from top-left
for y in range(14, 40):
    for x in range(11, 37):
        dx, dy = (x - 23.5) / 12.5, (y - 25) / 12
        inside = dx * dx + dy * dy <= 1 if y < 25 else abs(dx) <= 1 - max(0, (y - 33)) * 0.06
        if inside:
            shade = dx * 0.7 + dy * 0.5
            put(x, y, "W" if shade < -0.55 else "S" if shade < -0.15 else "n" if shade < 0.35 else "M" if shade < 0.7 else "m")
# visor slit with cyan glow, breaths below
for x in range(14, 34):
    put(x, 26, "k"); put(x, 27, "k")
for x in range(16, 32):
    put(x, 26, "C" if x % 5 else "c")
for x in (17, 20, 27, 30):
    for y in (31, 33, 35): put(x, y, "A")
# centre ridge + rivets
for y in range(15, 26): put(23, y, "H" if y < 20 else "W")
for x, y in ((14, 31), (33, 31), (15, 36), (32, 36)): put(x, y, "G")
# plume sweeping back from the crest
for i in range(16):
    t = i / 15
    cx, cy = 23 - t * 13, 13 - math.sin(t * math.pi) * 7 + t * 6
    for k in range(-2, 3):
        put(int(cx), int(cy + k), "R" if k < 1 else "r")
    put(int(cx), int(cy - 2), "E")

# outline everything that is not background
bg = set("kaQq")
add = []
for y in range(N):
    for x in range(N):
        if g[y][x] in bg:
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                X, Y = x + dx, y + dy
                if 0 <= X < N and 0 <= Y < N and g[Y][X] not in bg and g[Y][X] != "k":
                    add.append((x, y)); break
for x, y in add: put(x, y, "k")

if __name__ == "__main__":
    print(json.dumps({"legend": L, "frames": [["icon", ["".join(r) for r in g]]]}, separators=(",", ":")))
