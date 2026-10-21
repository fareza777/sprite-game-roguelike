"""Hand-drawn bestiary entry point. usage: python hand_bestiary.py <id> > out.json | --list
Prints {legend, frames:[[tag, rows]...], size, tags} like bestiary.py."""
import json
import sys

sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
from bestiary import L  # noqa: E402
import hand_b2, hand_b3, hand_b4, hand_b5, hand_b6, hand_b7, hand_b8, hand_b9, hand_b10  # noqa: E402,E401
import hand_bosses_a, hand_bosses_b  # noqa: E402,E401

CREATURES = {}
for module in (hand_b2, hand_b3, hand_b4, hand_b5, hand_b6, hand_b7, hand_b8, hand_b9, hand_b10, hand_bosses_a, hand_bosses_b):
    CREATURES.update(module.CREATURES)

if __name__ == "__main__":
    if sys.argv[1] == "--list":
        print(" ".join(CREATURES))
        sys.exit()
    cr = CREATURES[sys.argv[1]]
    frames = cr.frames()
    tags = list(dict.fromkeys(t for t, _ in frames))
    print(json.dumps({"legend": L, "frames": frames, "size": cr.size, "tags": tags}))
