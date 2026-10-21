"""B10 Heart of Dusk, hand-drawn: hollow acolyte, dusk hound, hunger maw."""
from hand_b5 import rime_wolf
from hand_core import Creature, biped, blob, part, quad, recolor, squash, stretch, swap, sway

# ---------------------------------------------------------------- hollow acolyte (caster: a blood-red orb)
_ACOLYTE_HEAD = [
    "....BCCB....",
    "...BCffCB...",
    "..BCfCCCCB..",
    ".BCfCCCCCCB.",
    ".BCCAAAACCB.",
    ".BCAAgAgACB.",
    ".BCAAAAAACB.",
    ".BBCAAAACBB.",
    "..BBCCCCBB..",
]
_ACOLYTE_HEM = [
    ".ABBCCdCCBBA.",
    ".ABCCCdCCCBA.",
    "ABBCCCdCCCBBA",
    "ABCCCCdCCCCBA",
    "ABCCCCdCCCCBA",
    "AABBBBdBBBBAA",
    ".A.A.A.A.A.A.",
]
hollow_acolyte = Creature(
    layers=[
        ("legs", part(rest=(_ACOLYTE_HEM, 8, 24), a=(sway(_ACOLYTE_HEM, -1), 8, 24), b=(sway(_ACOLYTE_HEM, 1), 8, 24))),
        ("body", part(rest=([
            "..BCCCCCB...",
            ".BCfCCCCCB..",
            "BCfCCdCCCCB.",
            "BCCCCdCCCCB.",
            "BCCCCdCCCCB.",
            "BCCCCdCCCBB.",
            "BBCCCdCCCBB.",
            "BBCCCdCCBBA.",
            "ABBCCdCCBBA.",
            "ABBCCdCCBBA.",
            "ABBCCdCCBBA.",
        ], 9, 13))),
        ("head", part(rest=(_ACOLYTE_HEAD, 9, 4), hurt=(swap(_ACOLYTE_HEAD, {5: ".BCAA5A5ACB."}), 9, 4))),
        ("arm", part(
            rest=(["CB..", "CCB.", ".CCB", ".xgh", ".hgf"], 19, 15),
            up=([".gh.", "ghHg", ".gf.", ".x..", "CB..", "CB..", "CB..", "CCB.", "CCB.", "CCB.", ".CB.", ".CB.", ".CB."], 19, 2),
            fwd=(["CCCCCxgh", "BBBBBhHg", "......gf"], 19, 15),
            follow=(["CB...", "CCB..", ".CCBx", "...gh", "...hf"], 19, 15),
        )),
        ("fx", part(smear=([".gh.", "ghHg", "hHHh", "ghHg", ".gh."], 26, 13))),
    ],
    plan=biped,
)

# ---------------------------------------------------------------- dusk hound (charger: the rime wolf's build in dusk colours)
_DUSK = {"2": "D", "3": "o", "W": "p", "4": "q", "H": "y", "1": "A", "n": "f", "m": "C", "l": "g"}


def _dusk_part(group):
    prt = dict(rime_wolf.layers)[group]
    return {st: (recolor(rows, _DUSK), ox, oy) for st, (rows, ox, oy) in prt.items()}


dusk_hound = Creature(layers=[(g, _dusk_part(g)) for g, _ in rime_wolf.layers], plan=quad)

# ---------------------------------------------------------------- hunger maw (melee: all mouth)
_MAW = [
    "......BCCCCB........",
    "....BCCffffCCB......",
    "...BCfRffffffCB.....",
    "..BCfRfffffffffCB...",
    ".BCffffdkfffdkffCB..",
    ".BCfffffffffffffCB..",
    "BCffffffffffffffffB.",
    "BCfHAHAHAHAHAHAHfCB.",
    "BCfAAAAAAAAAAAAAfCB.",
    "BCfAAArRRrAAAAAAfCB.",
    "BCfHAHAHAHAHAHAHfCB.",
    ".BCffffffffffffCB...",
    "..BCCCfffffCCCB.....",
    "...BBBBBBBBBB.......",
    ".....B..B...B.......",
    "....BB..BB..BB......",
]
_MAW_OPEN = _MAW[:8] + ["BCfAAAAAAAAAAAAAfCB.", "BCfAAAAAAAAAAAAAfCB.", "BCfAAArRRrAAAAAAfCB."] + _MAW[10:]
hunger_maw = Creature(
    layers=[
        ("body", part(
            rest=(_MAW, 6, 15),
            squat=(squash(_MAW, 5), 6, 16),
            tall=(stretch(_MAW, 5), 6, 14),
            fwd=(_MAW_OPEN, 7, 13),
            hurt=(swap(_MAW, {4: ".BCffff55fff55ffCB.."}), 6, 15),
        )),
        ("fx", part(smear=(["H.H.", ".H.H", "H.H.", ".H.H"], 26, 20))),
    ],
    plan=blob,
)

CREATURES = {"hollow_acolyte": hollow_acolyte, "dusk_hound": dusk_hound, "hunger_maw": hunger_maw}
