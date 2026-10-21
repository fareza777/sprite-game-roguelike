"""B3 Drowned Cistern, hand-drawn: brine crab, drowned sailor, eelshade."""
from hand_core import Creature, biped, crawler, floater, mirror, part, swap

# ---------------------------------------------------------------- brine crab
_CLAW = [
    "...hg..",
    "..hggf.",
    ".hgf.g.",
    ".ggf.gf",
    "hgggggf",
    "gggfff.",
    ".ff....",
    ".f.....",
]
_CLAW_SNAP = [
    "..hgg...",
    ".hgggff.",
    "hggf....",
    "gggf.hg.",
    "ggggggf.",
    ".fffff..",
    "..f.....",
]
brine_crab = Creature(
    layers=[
        ("legs", part(
            rest=(["...f.f.f....f.f.f...", "..f.f.f......f.f.f..", ".f.f.f........f.f.f.", ".f.f.f........f.f.f.", "e.e.e..........e.e.e"], 6, 26),
            a=(["...f.f.f....f.f.f...", "..f.f.f......f.f.f..", ".f.f.f........f.f.f.", ".f.f.f........f.f.f.", "e.e.e..........e.e.e"], 6, 26),
            b=(["...f.f.f....f.f.f...", "...f.f.f....f.f.f...", "..f.f.f......f.f.f..", ".f.f.f........f.f.f.", ".e.e.e........e.e.e."], 6, 26),
        )),
        ("back", part(rest=(mirror(_CLAW), 1, 15), up=(mirror(_CLAW), 1, 12))),
        ("head", part(
            rest=(["Hk..Hk", "kk..kk", ".f...f", ".f...f", ".f...f"], 13, 14),
            hurt=(["55..55", "......", ".f...f", ".f...f", ".f...f"], 13, 14),
        )),
        ("body", part(rest=([
            ".....fggggggf.....",
            "...fghhhggggggf...",
            "..fghhgggggggggf..",
            ".fgghgggggghggggf.",
            "fggggggggggggggggf",
            "fgggghgggggggghggf",
            "efggggggggggggggfe",
            ".eeffff5555ffffee.",
            "...eeeeeeeeeeee...",
        ], 7, 18))),
        ("arm", part(
            rest=(_CLAW, 24, 15),
            up=(_CLAW, 24, 11),
            fwd=(_CLAW_SNAP, 24, 17),
        )),
        ("fx", part(smear=(["..HH..", "....H.", ".....H", ".....H", "....H.", "..HH.."], 26, 13))),
    ],
    plan=crawler,
)

# ---------------------------------------------------------------- drowned sailor
_SAILOR_HEAD = [
    "...ijjjji...",
    "..ijlllljj..",
    "..jlmllllji.",
    ".44HHHHHH44.",
    ".sW444W4442.",
    ".s4WZn44Zn2.",
    ".s444444442.",
    ".ss42ZZ2422.",
    "..s2222222..",
    "...s2222....",
]
drowned_sailor = Creature(
    layers=[
        ("legs", part(
            rest=(["jji.jji", "jji.jji", "jji.jji", "j.i.j.i", "32..32.", "332.332", "222.222"], 11, 24),
            a=(["jji.jji.", "jji..jji", "jji...jji", "j.i....j.i", "32.....32", "332....332", "222....222"], 10, 24),
            b=(["jji.jji", "jji.jji", ".jji.jji", ".j.i.jj", "..32.32", "..332.", "..222."], 10, 24),
        )),
        ("body", part(rest=([
            "...2442...",
            ".HHHHHHH4.",
            "Hjjjjjjjjj",
            "HHHHHHHH44",
            "jjjjjjjjji",
            "HHHHHHH44i",
            "6666c6666.",
            "ijjjjjjii.",
            "ijjjjjjii.",
        ], 10, 15))),
        ("head", part(rest=(_SAILOR_HEAD, 9, 5), hurt=(swap(_SAILOR_HEAD, {5: ".s4W55445524", 7: ".ss42ZZZ422."}), 9, 5))),
        ("arm", part(
            rest=(["jH.....", "Hj.....", "jH.....", "33.....", ".5c....", "..44...", "...W4..", "....44.", ".....4."], 19, 16),
            up=(["..3..", "..43.", "..43.", "..W4.", "..43.", ".5cc5", "..33.", "..Hj.", ".jH..", ".Hj..", "jH...", "Hj...", "jH...", "Hj..."], 19, 1),
            fwd=(["jHjH3c.......", "HjHj3544444W4", ".....c......."], 19, 16),
            follow=(["jH......", "Hj......", ".jH.....", "..33....", "..5c....", "....44..", ".....W4.", "......44"], 19, 16),
        )),
        ("fx", part(smear=(["..HH...", "....Hn.", ".....Hn", ".....Hn", ".....Hn", "....Hn.", "..HHn.."], 24, 10))),
    ],
    plan=biped,
)

# ---------------------------------------------------------------- eelshade
_EEL_HEAD = [
    "...m.m.......",
    "...ompppo....",
    "..opqqqqqpp..",
    ".opqrqqqqHnp.",
    "opqqqqqqqqnqp",
    "oppqqqqqpAAAA",
    ".ooppppppppo.",
    "..ooooooo....",
]
eelshade = Creature(
    layers=[
        ("tail", part(
            rest=(["............ooo..", "......ooppppqqqq.", "...oopqqqqqqpppp.", ".oopqqppoooooo...", "opqpo............", "opo..............", ".o..............."], 3, 12),
            a=(["............ooo..", "......ooppppqqqq.", "...oopqqqqqqpppp.", ".oopqqppoooooo...", "opqpo............", "opo..............", ".o..............."], 3, 12),
            b=(["o................", "opo..............", "opqpo............", ".oopqqppooooo....", "...oopqqqqqqppppq", "......oopppqqqq..", "............ooo.."], 3, 10),
        )),
        ("body", part(
            rest=(_EEL_HEAD, 17, 10),
            up=(_EEL_HEAD, 17, 9),
            fwd=(swap(_EEL_HEAD, {5: "oppqqqqqpAAA.", 6: ".ooppppppAyAy", 7: "..oooooooAAA."}), 17, 10),
            hurt=(swap(_EEL_HEAD, {3: ".opqrqqqq55p."}), 17, 10),
        )),
        ("fx", part(smear=(["..nn..", "....n.", ".....n", ".....n", "....n.", "..nn.."], 26, 9))),
    ],
    plan=floater,
)

CREATURES = {"brine_crab": brine_crab, "drowned_sailor": drowned_sailor, "eelshade": eelshade}
