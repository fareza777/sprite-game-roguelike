"""B8 Sandsunk Necropolis, hand-drawn: sand wraith, scarab swarm, mummified priest."""
from hand_core import Creature, biped, blob, floater, merge, part, swap, sway

# ---------------------------------------------------------------- sand wraith (caster)
_WRAITH = [
    ".....8x8......",
    "....8xyx87....",
    "...8xyyxx87...",
    "..8xyxxxxx87..",
    "..8xy5AAA587..",
    ".8xx5AdAdA57..",
    ".8xx5AAAAA57..",
    ".8xxx5AAA5x7..",
    "8xxxxx555xx87.",
    "8xyxxxxxxxx87.",
    "78xxxxxxxx877.",
    ".78xxxxxxx87..",
    "..778888887...",
]
_WRAITH_TAIL = ["78x8x887x87.", ".7.8x.7.x8..", ".7..8..7.8..", "....7....7.."]
sand_wraith = Creature(
    layers=[
        ("tail", part(
            rest=(_WRAITH_TAIL, 10, 18),
            a=(_WRAITH_TAIL, 10, 18),
            b=(["78x8x887x87.", "..8x.7.x8.7.", "..8..7..8.7.", "..7.....7..."], 10, 18),
        )),
        ("body", part(
            rest=(_WRAITH, 9, 6),
            up=(swap(_WRAITH, {0: "d....8x8..d...", 1: "8d..8xyx87d8..", 2: ".8d8xyyxx8d...", 3: "..8xyxxxxx87.."}), 9, 5),
            fwd=(swap(_WRAITH, {8: "8xxxxx555xxx8xd", 9: "8xyxxxxxxxxxx8dd", 10: "78xxxxxxxx877.d"}), 9, 6),
            hurt=(swap(_WRAITH, {5: ".8xx5A5A5A57.."}), 9, 6),
        )),
        ("fx", part(smear=([".x.8.", "x8y.x", ".yHy.", "x.8x.", ".8..x"], 25, 12))),
    ],
    plan=floater,
)

# ---------------------------------------------------------------- scarab swarm (hopper: a cluster of beetles)
_SCARAB = ["..d.d..", ".jlmlj.", "jlmmlli", "ilcdcli", ".iiiii.", "i.i.i.i"]
_SCARAB_HURT = [r.replace("l", "H").replace("m", "H") for r in _SCARAB]


def _swarm(a, b, c, shell=_SCARAB):
    return merge([(a[0], a[1], shell), (b[0], b[1], shell), (c[0], c[1], shell)])


scarab_swarm = Creature(
    layers=[
        ("body", part(
            rest=(_swarm((0, 5), (9, 6), (4, 0)), 8, 19),
            squat=(_swarm((0, 6), (9, 6), (4, 2)), 8, 18),
            tall=(_swarm((0, 3), (9, 6), (5, 0)), 8, 18),
            fwd=(_swarm((2, 5), (12, 4), (6, 0)), 8, 19),
            hurt=(_swarm((0, 5), (9, 6), (4, 0), _SCARAB_HURT), 8, 19),
        )),
        ("fx", part(smear=(["H..", ".H.", "..H", ".H.", "H.."], 27, 20))),
    ],
    plan=blob,
)

# ---------------------------------------------------------------- mummified priest (shooter: an orb staff)
def _bandage(rows, phase=0):
    """Fill every 'y' with diagonal wrap bands: a dark seam, a mid tone, then two light pixels."""
    tone = "8xyy"
    return ["".join(tone[(c - r + phase) % 4] if ch == "y" else ch for c, ch in enumerate(line)) for r, line in enumerate(rows)]


_MUMMY_HEAD = _bandage([
    "...xyyyx...",
    "..xyyyyyx..",
    ".xyyyyyyyx.",
    ".xyyyyyyyx.",
    ".xy5lyy5yx.",
    ".xyyyyyyyx.",
    ".xyyyyyyyx.",
    "..xyyyyyx..",
    "...88888...",
])
_MUMMY_HEM = _bandage([
    ".xyyyyyyyx..",
    ".xyyyyyyyx..",
    "xyyyyyyyyyx.",
    "xyyyyyyyyyx.",
    "xyyyyyyyyyx.",
    "x8.y8.x8.y8.",
    "8..8..8..8..",
], 2)


def _staff(orb, arm_rows, length):
    return orb + ["...6."] * 5 + arm_rows + ["...6."] * length


_ORB = ["..cdc", "..dHd", "..cdc"]
_ORB_HOT = ["..dHd", "..HHH", "..dHd"]
_MUMMY_ARM = ["xy.6.", ".xy6.", "..y8."]
mummified_priest = Creature(
    layers=[
        ("legs", part(rest=(_MUMMY_HEM, 9, 23), a=(sway(_MUMMY_HEM, -1), 9, 23), b=(sway(_MUMMY_HEM, 1), 9, 23))),
        ("body", part(rest=(_bandage([
            "..cdccdc...",
            ".cdjjjjdc..",
            "xycjjjjcyx.",
            "xyyyyyyyyx.",
            "xyyyyyyyyx.",
            "xyyjjjjyyx.",
            "xyyyyyyyyx.",
            "xyyyyyyyyx.",
            "xyyyyyyyyx.",
            "xyyyyyyyyx.",
        ], 1), 10, 13))),
        ("head", part(rest=(_MUMMY_HEAD, 10, 4), hurt=(swap(_MUMMY_HEAD, {4: ".x855y855yx"}), 10, 4))),
        ("arm", part(
            rest=(_staff(_ORB, _MUMMY_ARM, 13), 19, 7),
            up=(_staff(_ORB_HOT, _MUMMY_ARM, 13), 19, 2),
            fwd=(_staff(_ORB_HOT, _MUMMY_ARM, 13), 21, 7),
            follow=(_staff(_ORB, _MUMMY_ARM, 13), 20, 7),
        )),
        ("fx", part(smear=(["..d..", ".dHd.", "dHHHd", ".dHd.", "..d.."], 25, 6))),
    ],
    plan=biped,
)

CREATURES = {"sand_wraith": sand_wraith, "scarab_swarm": scarab_swarm, "mummified_priest": mummified_priest}
