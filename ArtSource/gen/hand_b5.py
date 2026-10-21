"""B5 Frostbone Caverns, hand-drawn: frost wisp, rime wolf, ice revenant."""
from hand_core import Creature, biped, floater, limbs, part, quad, swap

# ---------------------------------------------------------------- frost wisp (caster: spits ice)
_WISP = [
    "....nH......",
    "...nHn..n...",
    "..mnHnn.nm..",
    ".mnHHnnmnnm.",
    "mnHHnnnnnnnm",
    "mnHGnnnGnnml",
    "mnHGnnnGnnml",
    "lmnnnnnnnnml",
    ".lmnnnnnnml.",
    ".llmmmmmmll.",
    "..jllllllj..",
    "...jjjjjj...",
]
frost_wisp = Creature(
    layers=[
        ("tail", part(
            rest=([".lml.ml.", "..lj.l..", "..j..j..", "...j...."], 12, 20),
            a=([".lml.ml.", "..lj.l..", "..j..j..", "...j...."], 12, 20),
            b=([".ml.lml.", "..l..jl.", "...j..j.", "......j."], 12, 20),
        )),
        ("body", part(
            rest=(_WISP, 10, 9),
            up=(swap(_WISP, {0: "..H.nH..H...", 1: "...nHn..n.H."}), 10, 8),
            fwd=(swap(_WISP, {7: "lmnnnnnGGnml", 8: ".lmnnnnGGml."}), 10, 9),
            hurt=(swap(_WISP, {5: "mnHnGnnnGnml", 6: "mnHnnnnnnnml"}), 10, 9),
        )),
        ("fx", part(smear=(["..H...", ".nHn..", "mnHHnm", ".nHn..", "..H..."], 24, 12))),
    ],
    plan=floater,
)

# ---------------------------------------------------------------- rime wolf (charger)
_NEAR = ["3W3", "3W3", "3W3", "3W3", "3W3", "33W3", "2223"]
_FAR = ["12", "12", "12", "12", "12", "112", "111"]
_WOLF_HEAD = [
    "..3W.......",
    ".3W4W......",
    ".3WWWW4....",
    "3WWlkWWW4..",
    "3WWWWWWWWWk",
    "23WWW33333.",
    ".2233......",
]
rime_wolf = Creature(
    layers=[
        ("back", part(
            rest=(["34....", "3W4...", ".3W4..", "..3W4W", "...33W"], 1, 13),
            a=(["......", "......", "33W4..", ".3WW4W", "...33W"], 1, 13),
        )),
        ("legs", part(
            rest=(limbs([(1, 0, _FAR), (4, 0, _NEAR), (11, 0, _FAR), (14, 0, _NEAR)], 7), 5, 24),
            a=(limbs([(4, -3, _FAR), (7, -3, _NEAR), (10, 3, _FAR), (13, 3, _NEAR)], 7), 5, 24),
            b=(limbs([(1, 2, _FAR), (4, 2, _NEAR), (12, -2, _FAR), (15, -2, _NEAR)], 7), 5, 24),
            c=(limbs([(2, -1, _FAR), (5, 1, _NEAR), (11, 1, _FAR), (14, -1, _NEAR)], 7), 5, 24),
            crouch=(limbs([(1, -1, _FAR[2:]), (4, -1, _NEAR[2:]), (11, 1, _FAR[2:]), (14, 1, _NEAR[2:])], 5), 5, 26),
        )),
        ("body", part(rest=([
            "...........n.m.n..",
            "......n..mnmnmn...",
            "....3WWm44WWW44W..",
            "..3WW4444WWWW44WW.",
            ".3W44WWWWWWWWWWWW3",
            "3WWWWWWWWWWWWWWWW3",
            "3WWWWW3333WWWWWWW3",
            "23WWW32222WWWWWW32",
            ".2333...223WWWW32.",
            "..22......23333W2.",
            "..........2222....",
        ], 5, 14))),
        ("head", part(
            rest=(_WOLF_HEAD, 20, 11),
            up=(_WOLF_HEAD, 20, 9),
            fwd=(_WOLF_HEAD[:5] + ["23WWWH.H.H.", "23W........", "233WH.H.H..", ".2333333..."], 20, 11),
            hurt=(swap(_WOLF_HEAD, {3: "3WW11WWW4.."}), 19, 11),
        )),
        ("fx", part(smear=(["H..", ".H.", "..H", ".H.", "H.."], 29, 13))),
    ],
    plan=quad,
)

# ---------------------------------------------------------------- ice revenant (melee)
_REV_HEAD = [
    "..n..n..n..",
    "..m.nm.m...",
    "..44444W4..",
    ".4HHHHHH44.",
    ".4HkkHHkk4.",
    ".4HklHHkl4.",
    ".4HHH4HHH3.",
    "..4H4H4H3..",
    "...33333...",
]
ice_revenant = Creature(
    layers=[
        ("back", part(rest=(["ij..", "jj..", "jji.", "jjji", ".jji", ".jij", "..ii"], 7, 14))),
        ("legs", part(
            rest=([".4...4.", ".W...W.", ".4...4.", ".3...3.", ".W...W.", "lmn.lmn", "lmm.lmm", "lmm.lmm", "jjj.jjj"], 11, 22),
            a=([".4...4..", ".W....W.", "4......4", "3......3", "W.......W", "lmn....lmn", "lmm....lmm", "lmm....lmm", "jjj....jjj"], 10, 22),
            b=([".4...4.", ".W...W.", "..4.4..", "..3.3..", "..W.W..", "..lmnlmn", "..lmmjjj", "..lmm...", "..jjj..."], 10, 22),
        )),
        ("body", part(rest=([
            "..i444W3i..",
            ".ijWHHHW3ji",
            "ijHHHHHH3jj",
            "ijjjjHjjjjj",
            "ijHHHHHH3jj",
            ".ijjjHjjjji",
            ".ijjj55jjj.",
            "..i3333..i.",
            "...i.....i.",
        ], 9, 13))),
        ("head", part(rest=(_REV_HEAD, 10, 4), hurt=(swap(_REV_HEAD, {4: ".4H33HH334.", 5: ".4HHHHHHH4."}), 10, 4))),
        ("arm", part(
            rest=(["W4......", "4W......", ".W4.....", ".44.....", "..jj....", "...nH...", "....nH..", ".....nH.", "......nm"], 19, 14),
            up=(["..H..", "..nH.", "..nH.", "..nH.", "..nH.", "..nm.", ".jjjj", "..44.", "..W4.", ".4W..", ".W4..", "4W...", "W4...", "4W...", "W...."], 19, 0),
            fwd=(["W4W4jj......", "4W4WjjnHHHHn", "......nnmmm."], 19, 15),
            follow=(["W4.....", "4W.....", ".W4....", "..44...", "..jj...", "....nH.", ".....nH", "......n"], 19, 15),
        )),
        ("fx", part(smear=(["..HH...", "....Hn.", ".....Hn", ".....Hn", ".....Hn", "....Hn.", "..HHn.."], 24, 10))),
    ],
    plan=biped,
)

CREATURES = {"frost_wisp": frost_wisp, "rime_wolf": rime_wolf, "ice_revenant": ice_revenant}
