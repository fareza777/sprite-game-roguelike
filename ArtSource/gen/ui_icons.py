"""16x16 HUD icons (gold, potion, attack, dodge, skill, pause) as grids; prints JSON {legend, frames}."""
import json
import sys

sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
from compose import Canvas

L = {
    "k": "#10141f", "H": "#ebede9", "S": "#c7cfcc", "s": "#819796", "M": "#577277", "m": "#394a50", "A": "#202e37",
    "a": "#151d28", "Y": "#e8c170", "G": "#de9e41", "o": "#be772b", "n": "#884b2b", "R": "#a53030", "E": "#cf573c",
    "r": "#752438", "Q": "#411d31", "c": "#ad7757", "B": "#4f8fba", "b": "#3c5e8b", "d": "#253a5e", "C": "#a4dddb",
    "T": "#73bed3",
}


def grid(rows):
    assert len(rows) == 16 and all(len(r) == 16 for r in rows), [len(r) for r in rows]
    return rows


GOLD = grid([
    "................",
    ".....kkkkkk.....",
    "...kkGGGGGGkk...",
    "..kGYYYYYYYGok..",
    "..kGYHHYYYYGok..",
    ".kGYHYGGGGYYGok.",
    ".kGYYGooooGYGok.",
    ".kGYYGoGGGGYGok.",
    ".kGYYGoGGGGYGok.",
    ".kGYYGGGGGGYGok.",
    ".kGGYYGGGGYYGok.",
    "..kGGYYYYYYGok..",
    "..koGGGGGGGGok..",
    "...kkoooooookk..",
    ".....kkkkkk.....",
    "................",
])

POTION = grid([
    "................",
    "......kkkk......",
    "......kcck......",
    "......kSsk......",
    ".....kkSskk.....",
    "....kRRRRRRk....",
    "...kREHRRRRrk...",
    "..kREHRRRRRRrk..",
    "..kREERRRRRRrk..",
    "..kRERRRRRRRrk..",
    "..kRRRRRRRRrrk..",
    "..krRRRRRRrrrk..",
    "...krrrrrrrrk...",
    "....kkkkkkkk....",
    "................",
    "................",
])


def attack():
    c = Canvas(16, 16)
    for i in range(10):                       # blade: edge + flat, tip at top-right
        c.g[11 - i][4 + i] = "H"
        c.g[12 - i][4 + i] = "S"
        c.g[12 - i][5 + i] = "s"
    c.g[1][14] = "H"
    c.paste(["GG...", "GoG..", ".GoG.", "..Go."], 2, 9)    # crossguard
    c.paste(["n.", "Qn", ".Q"], 1, 12)                       # grip
    c.g[14][0] = "Y"; c.g[13][1] = "G"                       # pommel
    return c.outline("k").rows()


def dodge():
    c = Canvas(16, 16)
    for y, x0, x1, ch in ((4, 1, 6, "T"), (7, 0, 5, "C"), (10, 2, 7, "T")):
        for x in range(x0, x1):
            c.g[y][x] = ch
    chevron = ["CC.....", ".CCC...", "..CHHH.", "...HHHH", "..CHHH.", ".CCC...", "CC....."]
    c.paste(chevron, 7, 4)
    c.paste(["T", "T"], 7, 4)
    return c.outline("k").rows()


SKILL = grid([
    "................",
    "...kkkkkkkkkk...",
    "..kGYYYYYYYYGk..",
    "..kYBBBBHBBBGk..",
    "..kYBTBBHBBbGk..",
    "..kYBBBBHBBbGk..",
    "..kYHHHHHHHHGk..",
    "..kGBBBBHBbbok..",
    "...kGBBBHBbok...",
    "...kGbBBHbbok...",
    "....kGbBHbok....",
    ".....kGbHok.....",
    "......kGok......",
    ".......kk.......",
    "................",
    "................",
])

PAUSE = grid([
    "................",
    ".kkkkkkkkkkkkkk.",
    ".kmMMMMMMMMMMmk.",
    ".kMaaaaaaaaaaAk.",
    ".kMakkkaakkkaAk.",
    ".kMakHkaakHkaAk.",
    ".kMakHkaakHkaAk.",
    ".kMakHkaakHkaAk.",
    ".kMakHkaakHkaAk.",
    ".kMakSkaakSkaAk.",
    ".kMakSkaakSkaAk.",
    ".kMakkkaakkkaAk.",
    ".kMaaaaaaaaaaAk.",
    ".kmAAAAAAAAAAmk.",
    ".kkkkkkkkkkkkkk.",
    "................",
])

frames = [("gold", GOLD), ("potion", POTION), ("attack", attack()), ("dodge", dodge()), ("skill", SKILL), ("pause", PAUSE)]

if __name__ == "__main__":
    for t, rows in frames:
        assert len(rows) == 16 and all(len(r) == 16 for r in rows), t
    print(json.dumps({"legend": L, "frames": frames}, separators=(",", ":")))
