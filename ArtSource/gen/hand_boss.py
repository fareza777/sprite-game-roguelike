"""Boss drawing helpers: block the big volumes in with rim-lit shapes, then hand-place the details on top."""
import sys

sys.path.insert(0, r"D:/Isprite Game Pixel/ArtSource/gen")
from compose import Canvas  # noqa: E402
from rig import ellipse, fill, poly, put, rect, shade, thick  # noqa: E402,F401

RAMP = {
    "violet": ("o", "p", "q", "r"), "pink": ("p", "q", "r", "R"), "flesh": ("6", "7", "8", "x"), "pale": ("2", "3", "W", "4"),
    "moss": ("s", "t", "u", "v"), "green": ("E", "s", "t", "u"), "teal": ("E", "Z", "j", "l"), "blue": ("G", "i", "j", "l"),
    "sea": ("i", "j", "l", "m"), "ice": ("j", "l", "m", "n"), "glass": ("G", "l", "n", "H"), "steel": ("1", "2", "3", "4"),
    "slate": ("z", "Z", "1", "2"), "iron": ("w", "z", "Z", "1"), "ember": ("F", "e", "f", "g"), "fire": ("f", "g", "h", "d"),
    "gold": ("a", "b", "c", "d"), "brass": ("5", "a", "b", "c"), "wood": ("5", "6", "7", "8"), "bark": ("F", "5", "6", "7"),
    "sand": ("6", "7", "8", "x"), "linen": ("8", "x", "y", "H"), "bone": ("7", "8", "x", "y"), "crimson": ("A", "B", "C", "f"),
    "night": ("D", "o", "p", "q"), "dusk": ("w", "D", "o", "p"),
}


def sheet(w, h, draw):
    """Run draw(c) on a fresh w x h canvas and return its rows."""
    c = Canvas(w, h)
    draw(c)
    return c.rows()


def spots(c, at, rows):
    """Paste the same small grid at several (x, y) positions."""
    for x, y in at:
        c.paste(rows, x, y)
