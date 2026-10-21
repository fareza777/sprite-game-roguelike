"""B4 Ember Forge, hand-drawn: cinder imp, slag golem, forge sentry."""
from hand_core import Creature, biped, mirror, part, swap

# ---------------------------------------------------------------- cinder imp (shooter: flings fire)
_IMP_HEAD = [
    ".Z........Z.",
    ".1Z......Z1.",
    "..1gghhgg1..",
    ".fghhggggggf",
    ".fghdkggdkgf",
    ".fgggggggggf",
    ".efgHeHeHgfe",
    "..efffffffe.",
]
_FIRE = ["..h.", ".hdh", "hdHd", ".hdh"]
cinder_imp = Creature(
    layers=[
        ("back", part(
            rest=(["e.........", "Fe........", "Fee.......", "Feee.e....", "FeeeeFe...", ".FeeeFFe..", "..F..FF...",
                   ".......fe.", "......fe..", ".....gf...", "....ggg..."], 4, 11),
            up=(["e.e.......", "FeFe......", "FeeFe.....", "Feeeee....", "FeeeeFe...", ".FeeeFFe..", "..F..FF...",
                 ".......fe.", "......fe..", ".....gf...", "....ggg..."], 4, 8),
        )),
        ("legs", part(
            rest=([".fe.fe", ".fe.fe", ".gf.gf", ".gf.gf", ".fe.fe", ".fe.fe", ".ef.ef", "1ZZ1ZZ"], 12, 23),
            a=([".fe.fe.", "fe...fe", "fe...fe", "gf....gf", "gf....gf", "fe....fe", "ef....ef", "1ZZ...1ZZ"], 11, 23),
            b=([".fe.fe", ".fe.fe", ".gf.gf", ".gf.gf", ".fe.fe", ".fe.ZZ", ".ef...", "1ZZ..."], 12, 23),
        )),
        ("body", part(rest=(["..fggf..", ".fghhgf.", "fghggggf", "fgggggge", ".fgggfe.", ".eaaaae.", "..efffe."], 12, 16))),
        ("head", part(rest=(_IMP_HEAD, 10, 8), hurt=(swap(_IMP_HEAD, {4: ".fgh55gg55gf"}), 10, 8))),
        ("arm", part(
            rest=(["gf.", "gfe", ".gf", ".gf", ".dd", "d.d"], 19, 17),
            up=(_FIRE + ["..d.", "..gf", "..gf", "..gf", "..gf", "..gf", ".gf."], 18, 5),
            fwd=(["gffffdd", "geeeedd"], 19, 17),
            follow=(["gf..", ".gf.", ".gfe", "..gf", "..dd", ".d.d"], 19, 17),
        )),
        ("fx", part(smear=(_FIRE, 27, 15))),
    ],
    plan=biped,
)

# ---------------------------------------------------------------- slag golem (tank: slams)
_GOLEM_ARM = ["..Z12Z.", ".Z1222Z", ".Z1222Z", ".Z12c2Z", ".Z1222Z", ".Z1222Z", "..Z122Z", ".Z12222Z", ".Z1c222Z", ".Z12222Z", "..ZZZZZ."]
_GOLEM_HEAD = ["..Z1221..", ".Z122221Z", "Z12dc2dc1", "Z1222222Z", ".ZZcccZZ.", "..ZZZZZ.."]
slag_golem = Creature(
    layers=[
        ("back", part(rest=(mirror(_GOLEM_ARM), 1, 11), up=(mirror(_GOLEM_ARM), 1, 9))),
        ("legs", part(
            rest=(["Z1222Z..Z1222Z", "Z1222Z..Z1222Z", "Z12c2Z..Z122cZ", "Z1222Z..Z1222Z", "Z1222Z..Z1222Z", "Z1222Z..Z1222Z",
                   "Z12222Z.Z12222Z", "ZZZZZZZ.ZZZZZZZ"], 8, 23),
            a=(["Z1222Z...Z1222Z", "Z1222Z...Z1222Z", "Z12c2Z....Z122cZ", "Z1222Z....Z1222Z", "1222Z......Z1222Z", "1222Z......Z1222Z",
                "22222Z.....Z12222Z", "ZZZZZZ.....ZZZZZZZ"], 6, 23),
            b=(["Z1222Z..Z1222Z", "Z1222Z..Z1222Z", "Z12c2Z..Z122cZ", "Z1222Z..Z1222Z", "Z1222Z..ZZZZZZ", "Z1222Z........",
                "Z12222Z.......", "ZZZZZZZ......."], 8, 23),
        )),
        ("body", part(rest=([
            "..Z1122Z......Z2211Z..",
            ".Z122222ZZZZZZ222221Z.",
            "Z12222222222222222221Z",
            "Z12222c22222222c22221Z",
            "Z1222cd2222222222c222Z",
            ".Z122c22222hh2222c22Z.",
            ".Z12222222hddh222c22Z.",
            ".Z12222222hddh22222Z..",
            "..Z1222c222hh2222221Z.",
            "..Z12222c22222222c21Z.",
            "..ZZ1222c22222222c2ZZ.",
            "...ZZ1222222222222ZZ..",
            "....ZZ11111111111ZZ...",
            ".....ZZZZZZZZZZZZZ....",
        ], 5, 10))),
        ("head", part(rest=(_GOLEM_HEAD, 11, 5), hurt=(swap(_GOLEM_HEAD, {2: "Z12cc2cc1"}), 11, 5))),
        ("arm", part(
            rest=(_GOLEM_ARM, 21, 11),
            up=(_GOLEM_ARM[::-1][:1] + [".Z12222Z", ".Z1c222Z", ".Z12222Z", "..Z122Z", ".Z1222Z", ".Z1222Z", ".Z12c2Z", ".Z1222Z",
                                         ".Z1222Z", "..Z12Z."], 21, 0),
            fwd=(["Z12222ZZZZ.", "Z122c2Z122Z", "Z12222Z1c2Z", ".ZZZZZZ122Z", "......ZZZZ."], 20, 12),
            follow=([".Z12Z..", "Z1222Z.", "Z12c2Z.", ".Z1222Z", ".Z1222Z", "..Z1222Z", ".Z122222Z", ".Z1c2222Z", ".Z122222Z", "..ZZZZZZ."], 21, 14),
        )),
        ("fx", part(smear=([".d...", "c.h.d", ".hcd.", "dch.c"], 26, 25))),
    ],
    plan=biped,
)

# ---------------------------------------------------------------- forge sentry (turret: fires a forge cannon)
_SENTRY_HEAD = ["...5aa5...", "..abccba..", ".abcddcbba", ".ab55gg55a", ".abbccbbba", "..5abba55.", "...5555..."]
forge_sentry = Creature(
    layers=[
        ("back", part(rest=([".3..", "3.3.", ".21.", ".12.", ".12.", ".12.", "512."], 8, 8),
                      up=(["3.3.", ".3..", ".21.", ".12.", ".12.", ".12.", "512."], 8, 8))),
        ("legs", part(
            rest=([".5a5.5a5.", ".5a5.5a5.", ".121.121.", ".5a5.5a5.", ".5a5.5a5.", ".5a5.5a5.", "5aab55aab", "555555555"], 11, 23),
            a=([".5a5..5a5.", ".5a5..5a5.", ".121...121", "5a5....5a5", "5a5....5a5", "5a5....5a5", "aab5...5aab", "5555...5555"], 10, 23),
            b=([".5a5.5a5.", ".5a5.5a5.", ".121.121.", ".5a5.5a5.", ".5a5.5555", ".5a5.....", "5aab.....", "5555....."], 11, 23),
        )),
        ("body", part(rest=([
            "..5aabbaa5..",
            ".5abccccba5.",
            "5abcddccbba5",
            "5abc1221cba5",
            "5abcghhgcba5",
            "5abcgddgcba5",
            "5abbcggcbba5",
            ".5abbbbbba5.",
            ".5d5d5d5d5..",
            "..5aaaaaa5..",
        ], 10, 13))),
        ("head", part(rest=(_SENTRY_HEAD, 11, 6), hurt=(swap(_SENTRY_HEAD, {3: ".ab5555555a"}), 11, 6))),
        ("arm", part(
            rest=(["5a5.....", "abc5....", "abc51111", "5ab12223", ".5512223", "...51111"], 19, 15),
            up=(["5a5......", "abc51111h", "abc12223g", "5ab12223h", ".55.1111."], 19, 14),
            fwd=(["5a5.....", "abc5....", "abc51111", "5ab12223", ".5512223", "...51111"], 18, 15),
        )),
        ("fx", part(smear=([".d..", "dhd.", "hHhd", "dhd.", ".d.."], 27, 15))),
    ],
    plan=biped,
)

CREATURES = {"cinder_imp": cinder_imp, "slag_golem": slag_golem, "forge_sentry": forge_sentry}
