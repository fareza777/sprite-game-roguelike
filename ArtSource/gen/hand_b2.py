"""B2 Mycelial Hollow, hand-drawn: sporeling, mire toad, fungal shambler."""
from hand_core import Creature, biped, blob, part, squash, stretch, swap

# ---------------------------------------------------------------- sporeling
_SPORELING = [
    ".....pqqqp.....",
    "...pqRRqqqqp...",
    "..pqRyyRqqyqp..",
    ".pqRRyyqqqqqqp.",
    ".pqqqqqqqyyqqqp",
    "pqyyqqqqqyyqqpp",
    "opqyqqqqqqqqppo",
    ".oopppppppppoo.",
    "...o6565656o...",
    "....xyyyyxx8...",
    "....ykxxxkx8...",
    "....xxx5xxx8...",
    "....8xxxxx88...",
    ".....8xxx88....",
    "....xx8..x8....",
    "....888..888...",
]
sporeling = Creature(
    layers=[
        ("body", part(
            rest=(_SPORELING, 9, 15),
            squat=(squash(_SPORELING, 12), 9, 16),
            tall=(stretch(_SPORELING, 12), 9, 14),
            fwd=(squash(_SPORELING, 12), 9, 16),
            hurt=(swap(_SPORELING, {10: "....y5xxx5x8...", 11: "....xx555xx8..."}), 9, 15),
        )),
        ("fx", part(smear=([".R..V.", "R.RV..", ".VRR.V", "R.V.R.", ".R..R."], 24, 15))),
    ],
    plan=blob,
)

# ---------------------------------------------------------------- mire toad
_TOAD = [
    "...........tuut....",
    "......tttttuvdcu...",
    "....ttuuvvuuvckdt..",
    "...tuuvvuuuuucddut.",
    "..tuuvuustuuutuuuut",
    ".tuuuuuuuuuuuuuuuut",
    ".tusuuuuuuusuuuuuuu",
    "stuuuuuuuuuuu55555t",
    "stuuuuuuuuuuVVVVVVs",
    "sttuuuuuuuuVVVVVVs.",
    "sstttuuutsVVVVVVs..",
    ".sstuuuts.sVVVs....",
    ".stuuuuts..tus.....",
    "ssssssss..ssss.....",
]
_TOAD_OPEN = swap(_TOAD, {7: "stuuuuuuuuuuuAAAAA.", 8: "stuuuuuuuuuuuuAAArRRRRRr", 9: "sttuuuuuuuuVVVVVVs....rr"})
mire_toad = Creature(
    layers=[
        ("body", part(
            rest=(_TOAD, 5, 17),
            squat=(squash(_TOAD, 9), 5, 18),
            tall=(stretch(_TOAD, 9), 5, 16),
            fwd=(_TOAD_OPEN, 5, 17),
            hurt=(swap(_TOAD, {2: "....ttuuvvuuvc5dt..", 3: "...tuuvvuuuuu55dut."}), 5, 17),
        )),
    ],
    plan=blob,
)

# ---------------------------------------------------------------- fungal shambler
_SHAMBLER_HEAD = [
    "......ppqqqp.....",
    "....pqqRRqqqqp...",
    "...pqRRRyyqqqqp..",
    "..pqRRqyyyqqqyqp.",
    ".pqRqqqqyqqqqyyqp",
    ".pqqqyyqqqqqqqqqp",
    "pqqqqyyqqqqqyqqpp",
    "opqqqqqqqqqqqqppo",
    ".oopppppppppppoo.",
    "..o56565656565o..",
    "....xyyyyxxx8....",
    "....y5Rxxx5Rx8...",
    "....x55xxx55x8...",
    ".....8x5x5xx8....",
]
_SHAMBLER_HEAD_HURT = _SHAMBLER_HEAD[:11] + ["....y55xxx55x8...", "....xxxxxxxxx8...", ".....8x555xx8...."]

fungal_shambler = Creature(
    layers=[
        ("legs", part(
            rest=([".uts.uts.", ".uts.uts.", ".uts.uts.", ".uts.uts.", "uuts.uuts", "ssss.ssss"], 10, 25),
            a=([".uts.uts.", ".uts..uts", "uts...uts", "uts....uts", "uuts...uuts", "sss.....sss"], 9, 25),
            b=([".uts.uts.", ".uts.uts.", "..uts.uts", "..uts.uts", "..uuts.ss", "..sss...."], 9, 25),
        )),
        ("body", part(rest=([
            "...tuuuut...",
            ".ttuvvuuutt.",
            "tuuvvuuuuuts",
            "tuvuuuquuuts",
            "tuuuuqRquuts",
            "tuuuuuquuuts",
            "stuuuuuuutss",
            ".stuuuuutts.",
            ".s6767676s..",
            "..s7676s....",
        ], 9, 16))),
        ("head", part(rest=(_SHAMBLER_HEAD, 7, 3), hurt=(_SHAMBLER_HEAD_HURT, 7, 3))),
        ("arm", part(
            rest=(["utt.", "uuts", ".uts", ".uts", ".uuts", "..yWy", "..y.y"], 18, 18),
            up=(["..y.y.", "..yWy.", "..uts.", "..uts.", ".uts..", ".uts..", ".uts..", ".uts..",
                 "uts...", "uts...", "uts...", "uts...", "uts...", "uts..."], 18, 4),
            fwd=([".......y..", "utttttuWy.", "uuuuuutWyy", "ssssss.y.."], 18, 17),
            follow=(["tu....", "tuu...", ".tuu..", "..tuu.", "...tuyy", "....yWy", ".....y."], 18, 18),
        )),
        ("fx", part(smear=(["..HH.....", "....HH...", ".....RH..", "......RH.", "......RH.", ".....RH..", "...RRH..."], 22, 12))),
    ],
    plan=biped,
)

CREATURES = {"sporeling": sporeling, "mire_toad": mire_toad, "fungal_shambler": fungal_shambler}
