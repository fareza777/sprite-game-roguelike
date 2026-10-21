"""Build a pixel TrueType font from a cell-grid bitmap font JSON (e.g. the CC0 pixel5x7 from aseprite-ai-artist).

usage: python make_font.py <font.json> <out.ttf> <family name>
Each '#' becomes a square outline; glyphs are proportional (trimmed) with 1px tracking.
"""
import json
import sys

from fontTools.fontBuilder import FontBuilder
from fontTools.pens.ttGlyphPen import TTGlyphPen

PX = 125          # font units per pixel -> 8px em at 1000 upm


def glyph_name(ch: str) -> str:
    return "space" if ch == " " else f"uni{ord(ch):04X}"


def build(src: str, out: str, family: str) -> None:
    font = json.load(open(src, encoding="utf-8"))
    cell_h, baseline = font["cellHeight"], font["baseline"]
    glyphs, advances, cmap = {}, {}, {}

    pen = TTGlyphPen(None)
    glyphs[".notdef"] = pen.glyph()
    advances[".notdef"] = (font["spaceWidth"] * PX, 0)

    for ch, rows in font["glyphs"].items():
        name = glyph_name(ch)
        cmap[ord(ch)] = name
        cols = [x for row in rows for x, c in enumerate(row) if c == "#"]
        pen = TTGlyphPen(None)
        if not cols:
            glyphs[name] = pen.glyph()
            advances[name] = (font["spaceWidth"] * PX, 0)
            continue
        left = min(cols)
        width = max(cols) - left + 1
        for y, row in enumerate(rows):
            for x, c in enumerate(row):
                if c != "#":
                    continue
                x0 = (x - left) * PX
                y0 = (baseline - y) * PX       # row `baseline` sits just above y=0
                # clockwise outer contour (TrueType convention)
                pen.moveTo((x0, y0))
                pen.lineTo((x0, y0 + PX))
                pen.lineTo((x0 + PX, y0 + PX))
                pen.lineTo((x0 + PX, y0))
                pen.closePath()
        glyphs[name] = pen.glyph()
        advances[name] = ((width + 1) * PX, 0)

    order = [".notdef"] + sorted(n for n in glyphs if n != ".notdef")
    fb = FontBuilder(1000, isTTF=True)
    fb.setupGlyphOrder(order)
    fb.setupCharacterMap(cmap)
    fb.setupGlyf(glyphs)
    fb.setupHorizontalMetrics({n: advances[n] for n in order})
    ascent, descent = (baseline + 1) * PX, (cell_h - baseline) * PX
    fb.setupHorizontalHeader(ascent=ascent, descent=-descent)
    fb.setupNameTable({"familyName": family, "styleName": "Regular"})
    fb.setupOS2(sTypoAscender=ascent, sTypoDescender=-descent, usWinAscent=ascent, usWinDescent=descent)
    fb.setupPost()
    fb.save(out)
    print(f"wrote {out}: {len(cmap)} glyphs")


if __name__ == "__main__":
    build(sys.argv[1], sys.argv[2], sys.argv[3])
