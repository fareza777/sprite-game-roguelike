"""B7 Clockwork Ossuary, hand-drawn: gear beetle, brass sentinel, tick bomb."""
from hand_core import Creature, biped, blob, crawler, limbs, mirror, part, squash, stretch, swap

# ---------------------------------------------------------------- gear beetle (charger: rams with its horn)
_BEETLE_LEG = ["2", "2", "1", "1", "1", "11"]


def _beetle_legs(slants):
    return limbs([(3 + 6 * k, s, _BEETLE_LEG) for k, s in enumerate(slants)], 6)


gear_beetle = Creature(
    layers=[
        ("legs", part(
            rest=(_beetle_legs([-1, 0, 1]), 5, 25),
            a=(_beetle_legs([-2, 2, -2]), 5, 25),
            b=(_beetle_legs([2, -2, 2]), 5, 25),
        )),
        ("body", part(rest=([
            "......3.3.3.......",
            ".....3232323......",
            "....a2315132a.....",
            "...abc23332cba....",
            "..abccdccccccba...",
            ".abcdddcccccccba..",
            "abccddcccccccccb..",
            "abcccccccccccccba.",
            "5abbbbbbbbbbbbba5.",
            ".55aaaaaaaaaaaa55.",
            "..555555555555....",
        ], 4, 14))),
        ("head", part(
            rest=(["..aab.", ".abcgb", ".abbba", "..aaa."], 21, 19),
            hurt=(["..aab.", ".abc5b", ".abbba", "..aaa."], 21, 19),
        )),
        ("arm", part(
            rest=(["..c", ".cb", "cb."], 25, 16),
            up=(["..c", ".cb", ".cb", "cb."], 25, 14),
            fwd=(["bccdH"], 26, 20),
        )),
        ("fx", part(smear=(["H...", ".H..", "..HH", ".H..", "H..."], 28, 18))),
    ],
    plan=crawler,
)

# ---------------------------------------------------------------- brass sentinel (tank: shield and fist)
_FIST = ["..5ab5.", ".5abbb5", ".5abbb5", ".5abdb5", ".5abbb5", ".5abbb5", "..5abb5", ".5abbbb5", ".5adbbb5", ".5abbbb5", "..55555."]
_SHIELD = ["..5555..", ".5abba5.", "5abccba5", "5bcmmcb5", "5bcmlcb5", "5bccccb5", "5abccba5", ".5abba5.", "..5555.."]
_SENT_HEAD = ["...5aa5...", "..abccba..", ".abcddcba.", ".a5mmmm5a.", ".abbbbbba.", "..555555.."]
brass_sentinel = Creature(
    layers=[
        ("back", part(rest=(_SHIELD, 1, 12), up=(_SHIELD, 0, 10))),
        ("legs", part(
            rest=(["5abbb5..5abbb5", "5abbb5..5abbb5", "5d5d55..5d5d55", "5abbb5..5abbb5", "5abbb5..5abbb5", "5abbb5..5abbb5",
                   "5abbbb5.5abbbb5", "5555555.5555555"], 8, 23),
            a=(["5abbb5...5abbb5", "5abbb5...5abbb5", "5d5d55....5d5d55", "5abbb5....5abbb5", "abbb5......5abbb5", "abbb5......5abbb5",
                "bbbbb5.....5abbbb5", "555555.....5555555"], 6, 23),
            b=(["5abbb5..5abbb5", "5abbb5..5abbb5", "5d5d55..5d5d55", "5abbb5..5abbb5", "5abbb5..555555", "5abbb5........",
                "5abbbb5.......", "5555555......."], 8, 23),
        )),
        ("body", part(rest=([
            "...5aabbbbbbbbbbaa5...",
            "..5abccccccccccccba5..",
            ".5abcdccccccccccccba5.",
            ".5abcc5d5d5d5d5ccbba5.",
            ".5abcc5jllllj5cccbba5.",
            ".5abcc5lmnnml5cccbba5.",
            ".5abcc5lnHnml5cccbba5.",
            ".5abcc5jlmmlj5cccbba5.",
            ".5abcc5d5d5d5d5ccbba5.",
            "..5abccccccccccccba5..",
            "..5aabbbbbbbbbbbbaa5..",
            "...55aaaaaaaaaaaa55...",
            "....55555555555555....",
        ], 5, 10))),
        ("head", part(rest=(_SENT_HEAD, 11, 5), hurt=(swap(_SENT_HEAD, {3: ".a555555a."}), 11, 5))),
        ("arm", part(
            rest=(_FIST, 21, 11),
            up=([".55555.", ".5abbbb5", ".5adbbb5", ".5abbbb5", "..5abb5", ".5abbb5", ".5abbb5", ".5abdb5", ".5abbb5", ".5abbb5", "..5ab5."], 21, 0),
            fwd=(["5abbbb5555.", "5abdbb5abb5", "5abbbb5adb5", ".555555abb5", "......5555."], 20, 12),
            follow=([".5ab5..", "5abbb5.", "5abdb5.", ".5abbb5", ".5abbb5", "..5abbb5", ".5abbbbb5", ".5adbbbb5", ".5abbbbb5", "..555555."], 21, 14),
        )),
        ("fx", part(smear=([".d...", "c.m.d", ".mcd.", "dcm.c"], 26, 25))),
    ],
    plan=biped,
)

# ---------------------------------------------------------------- tick bomb (bomber: waddles in and bursts)
_BOMB = [
    "...........d.",
    "..........dHd",
    "...........6.",
    "..........6..",
    "....zzzz5a...",
    "..zZZ11Zzz...",
    ".zZ12221Zzz..",
    "zZ1byyyyb1Zz.",
    "zZbyyykyyb1z.",
    "zZbyyykyyb1z.",
    "zZbyyykkkb1z.",
    "zZbyyyyyyb1z.",
    "zZ1byyyyb1Zz.",
    ".zZZ1111ZZz..",
    "..zzZZZZzz...",
    "...z..z..z...",
    "..zz..z..zz..",
]
_BOMB_HOT = [r.replace("y", "h") for r in _BOMB]
tick_bomb = Creature(
    layers=[
        ("body", part(
            rest=(_BOMB, 9, 14),
            squat=(squash(_BOMB, 11), 9, 15),
            tall=(stretch(_BOMB, 11), 9, 13),
            fwd=(_BOMB_HOT, 9, 14),
            hurt=([r.replace("y", "H") for r in _BOMB], 9, 14),
        )),
        ("fx", part(smear=([".d.h.", "dhHhd", ".hdh.", "d.h.d"], 22, 14))),
    ],
    plan=blob,
)

CREATURES = {"gear_beetle": gear_beetle, "brass_sentinel": brass_sentinel, "tick_bomb": tick_bomb}
