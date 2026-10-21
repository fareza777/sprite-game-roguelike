"""B9 Glass Abyss, hand-drawn: shard sprite, mirror knight, prism eye."""
from hand_core import Creature, biped, floater, limbs, part, swap

# ---------------------------------------------------------------- shard sprite (small shooter)
_SPRITE = ["...H...", "..nHn..", ".lnHHm.", "lnHnnml", "jlnkmkl", "jlnnnml", ".jlnml.", "..jlj..", "...j..."]
shard_sprite = Creature(
    layers=[
        ("tail", part(
            rest=(["H...........H", "nH.........Hn", ".nm.......mn.", "..lm.....ml..", "...j.....j..."], 10, 9),
            a=(["H...........H", "nH.........Hn", ".nm.......mn.", "..lm.....ml..", "...j.....j..."], 10, 9),
            b=([".............", "...j.....j...", "..lm.....ml..", ".nm.......mn.", "nH.........Hn"], 10, 11),
        )),
        ("body", part(
            rest=(_SPRITE, 13, 9),
            up=(_SPRITE, 13, 7),
            fwd=(swap(_SPRITE, {4: "jlnHmHl"}), 13, 9),
            hurt=(swap(_SPRITE, {4: "jln5m5l"}), 13, 9),
        )),
        ("fx", part(smear=(["..H..", ".nHn.", "nHHHn", ".nHn.", "..H.."], 22, 10))),
    ],
    plan=floater,
)

# ---------------------------------------------------------------- mirror knight (melee)
_KNIGHT_HEAD = [
    "....n......",
    "...nHn.....",
    "..lnnnnl...",
    ".lnHHnnml..",
    ".lnHnnnmlj.",
    ".jGGrGrGGj.",
    ".lnnnnnmlj.",
    "..ljjjjjj..",
]
_GLASS_LEG = [".ln", ".ln", ".jj", ".ln", ".ln", ".ln", "lnn", "jjj"]
mirror_knight = Creature(
    layers=[
        ("legs", part(
            rest=(limbs([(0, 0, _GLASS_LEG), (4, 0, _GLASS_LEG)], 8), 11, 23),
            a=(limbs([(1, -2, _GLASS_LEG), (4, 2, _GLASS_LEG)], 8), 10, 23),
            b=(limbs([(1, 1, _GLASS_LEG), (3, -1, _GLASS_LEG)], 8), 11, 23),
        )),
        ("body", part(rest=([
            "..lnnnml...",
            ".lnHHnnml..",
            "lnHnnnnmlj.",
            "lnnnHnnmlj.",
            "lnnnnHnmlj.",
            ".lnnnnHml..",
            ".jqqqqqqj..",
            ".lnnnnnml..",
            "..jlllllj..",
            "...j...j...",
        ], 10, 13))),
        ("head", part(rest=(_KNIGHT_HEAD, 10, 5), hurt=(swap(_KNIGHT_HEAD, {5: ".jGGGGGGGj."}), 10, 5))),
        ("arm", part(
            rest=(["ln.....", "nl.....", ".ln....", ".nn....", "..qq...", "...nH..", "....nH.", ".....nH", "......n"], 19, 14),
            up=(["..H..", "..nH.", "..nH.", "..nH.", "..nH.", "..nH.", ".qqqq", "..nn.", "..ln.", ".nl..", ".ln..", "nl...", "ln...", "nl..."], 19, 1),
            fwd=(["lnlnqqnHHHHHn", "nlnlqqnnnnnm."], 19, 15),
            follow=(["ln.....", "nl.....", ".ln....", "..nn...", "..qq...", "....nH.", ".....nH", "......n"], 19, 15),
        )),
        ("fx", part(smear=(["..HH...", "....Hr.", ".....Hr", ".....Hr", ".....Hr", "....Hr.", "..HHr.."], 24, 10))),
    ],
    plan=biped,
)

# ---------------------------------------------------------------- prism eye (turret: fires a beam)
_EYE = [
    "......n......",
    "...m..n..m...",
    "....4WWW4....",
    "..4WHHHHHW4..",
    ".4HHHqqqHHW4.",
    ".4HHqrRpqHW4.",
    "n4HHqpkkqHW4n",
    ".4HHqpkkqHW4.",
    ".4HHHqqqHW34.",
    "..43HHHHW34..",
    "....3333.....",
    "...m..n..m...",
    "......n......",
]
prism_eye = Creature(
    layers=[
        ("tail", part(
            rest=(["n.........n", "...........", "...........", "m.........m"], 10, 9),
            a=(["n.........n", "...........", "...........", "m.........m"], 10, 9),
            b=(["m.........m", "...........", "...........", "n.........n"], 10, 11),
        )),
        ("body", part(
            rest=(_EYE, 9, 6),
            up=(swap(_EYE, {5: ".4HHqrRpqHW4.", 6: "n4HHqpRRqHW4n", 7: ".4HHqpRRqHW4."}), 9, 5),
            fwd=(swap(_EYE, {6: "n4HHqprkqHW4n", 7: ".4HHqpkrqHW4."}), 9, 6),
            hurt=(swap(_EYE, {4: ".4HHHHHHHHW4.", 5: ".4HH333333W4.", 6: "n4HHHHHHHHW4n", 7: ".4HHHHHHHHW4."}), 9, 6),
        )),
        ("fx", part(smear=(["RRRRRR", "HHHHHH", "RRRRRR"], 23, 11))),
    ],
    plan=floater,
)

CREATURES = {"shard_sprite": shard_sprite, "mirror_knight": mirror_knight, "prism_eye": prism_eye}
