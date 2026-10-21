"""B6 Thornwild Ruins, hand-drawn: briar crawler, thorn dryad, root horror."""
from hand_core import Creature, biped, crawler, limbs, part, swap, sway

# ---------------------------------------------------------------- briar crawler (melee bramble beetle)
_LEG = ["t", "t", "s", "s", "s", "s", "E"]


def _crawler_legs(slants):
    return limbs([(1 + 4 * k, s, _LEG) for k, s in enumerate(slants)], 7)


briar_crawler = Creature(
    layers=[
        ("legs", part(
            rest=(_crawler_legs([-1, 1, -1, 1, 1]), 7, 24),
            a=(_crawler_legs([-2, 2, -2, 2, -2]), 7, 24),
            b=(_crawler_legs([2, -2, 2, -2, 2]), 7, 24),
        )),
        ("body", part(rest=([
            "...f...f...f......",
            "...sf..sf..sf.....",
            "..stttttttttts....",
            ".sttuuvuuuuuutts..",
            "sttuvvuuuuuuuutts.",
            "stuuuuufuuuufuuts.",
            "sttuuuuuuuuuuutts.",
            ".ssttttttttttsss..",
            "..ssssssssssss....",
        ], 5, 16))),
        ("head", part(
            rest=(["..ss..", ".sttt.", "sttgtg", "stttts", ".ssss."], 20, 19),
            hurt=(["..ss..", ".sttt.", "stt5t5", "stttts", ".ssss."], 20, 19),
        )),
        ("arm", part(
            rest=(["x.", ".8", "x."], 26, 20),
            up=(["x.", ".8", ".8", "x."], 26, 18),
            fwd=(["xx..", "..88", "xx.."], 26, 20),
        )),
        ("fx", part(smear=(["H..", ".H.", "..H", ".H.", "H.."], 29, 18))),
    ],
    plan=crawler,
)

# ---------------------------------------------------------------- thorn dryad (shooter: flings thorns)
_DRYAD_HEAD = [
    "....vu.u....",
    "..uvVvuvu...",
    ".uvVuuuvuu..",
    "tuvutttuuut.",
    "tuut8xxx8ut.",
    "tut8xkxxkxt.",
    "tut8xxxxx8tt",
    "stt88x5xx8ts",
    ".sst8888tss.",
    "..sstttss...",
]
_DRYAD_HEM = [
    ".67787776...",
    ".677877765..",
    "6778777765..",
    "67787777765.",
    "677877777665",
    "t5t55t5t5t5.",
    ".5.5..5..5..",
]
_THORN = ["..f.", ".fgf", "..f."]
thorn_dryad = Creature(
    layers=[
        ("legs", part(rest=(_DRYAD_HEM, 9, 24), a=(sway(_DRYAD_HEM, -1), 9, 24), b=(sway(_DRYAD_HEM, 1), 9, 24))),
        ("body", part(rest=([
            "...xx8.....",
            ".tuvvuts...",
            "tuvVvuutt..",
            "stuuufutts.",
            ".stttttss..",
            ".6778776...",
            ".67787765..",
            ".677f7765..",
            "t67787765..",
            ".6778f776..",
        ], 10, 14))),
        ("head", part(rest=(_DRYAD_HEAD, 9, 4), hurt=(swap(_DRYAD_HEAD, {5: "tut8x5xx5xt."}), 9, 4))),
        ("arm", part(
            rest=(["ut.", "ut.", ".7.", ".76", "..7", "..8f"], 19, 15),
            up=(_THORN + ["..7.", ".76.", ".76.", ".76.", "76..", "76..", "ut..", "ut.."], 19, 4),
            fwd=(["ut7776..", "ut77768f"], 19, 15),
            follow=(["ut..", "ut7.", ".776", "...8"], 19, 15),
        )),
        ("fx", part(smear=(["..f.", "ffgh", "..f."], 27, 15))),
    ],
    plan=biped,
)

# ---------------------------------------------------------------- root horror (caster: lashes with roots)
_ROOT_HEAD = [
    ".6.......6..",
    ".65..6..65..",
    "..65.65.6...",
    "...6666666..",
    "..67788776..",
    ".6778888776.",
    ".67AA877AA6.",
    ".67Av877Av6.",
    ".6775AAA576.",
    "..66777766..",
]
_ROOTS = [
    ".56..56..56.",
    ".56..56..56.",
    "566..566.566",
    "56...566..566",
    "566.5566.5566",
    "5555.555.5555",
]
root_horror = Creature(
    layers=[
        ("legs", part(rest=(_ROOTS, 9, 25), a=(sway(_ROOTS, -1), 9, 25), b=(sway(_ROOTS, 1), 9, 25))),
        ("body", part(rest=([
            "....5667776655......",
            "..567788887776655...",
            ".5677888887777766...",
            "56778887766777766t..",
            "5677887tvVt7777665..",
            "5677877vVVv7777665..",
            "5677877tvvt777766t..",
            "56677877tt7777766...",
            "t5667777777776665...",
            ".556667777766655....",
            "..55566666665555....",
            "...5555555555.......",
        ], 6, 13))),
        ("head", part(rest=(_ROOT_HEAD, 9, 3), hurt=(swap(_ROOT_HEAD, {7: ".67AA877AA6."}), 9, 3))),
        ("arm", part(
            rest=(["76...", "776..", ".776.", "..76.", "..76.", ".5t6.", "5..5."], 22, 14),
            up=(["..vV.", ".vVHv", "..v6.", "..76.", ".776.", ".76..", ".76..", "776..", "76...", "76...", "76...", "76..."], 21, 2),
            fwd=(["7766666t", "777777765", "55...5..5"], 22, 15),
            follow=(["76....", "776...", ".7766.", "...766", "....5t"], 22, 14),
        )),
        ("fx", part(smear=([".v..V", "V.v..", ".Vv.v", "v..V."], 27, 13))),
    ],
    plan=biped,
)

CREATURES = {"briar_crawler": briar_crawler, "thorn_dryad": thorn_dryad, "root_horror": root_horror}
