"""Render a generator's JSON frames to a scaled PNG contact sheet (stdlib only). usage: preview.py in.json out.png [scale] [cols]"""
import json, sys, zlib, struct
d = json.load(open(sys.argv[1])); L = d['legend']; F = d['frames']
S = int(sys.argv[3]) if len(sys.argv) > 3 else 4; cols = int(sys.argv[4]) if len(sys.argv) > 4 else 6
fw = len(F[0][1][0]); fh = len(F[0][1]); rows = (len(F) + cols - 1) // cols
W, H = cols * fw * S, rows * fh * S
bg = (60, 62, 72)
pix = [[bg] * W for _ in range(H)]
for n, (t, g) in enumerate(F):
    ox, oy = (n % cols) * fw * S, (n // cols) * fh * S
    for y, r in enumerate(g):
        for x, ch in enumerate(r):
            if ch == '.': continue
            h = L[ch]; col = tuple(int(h[i:i + 2], 16) for i in (1, 3, 5))
            for b in range(S):
                row = pix[oy + y * S + b]
                for a in range(S): row[ox + x * S + a] = col
raw = b''.join(b'\x00' + bytes(v for p in row for v in p) for row in pix)
def chunk(k, data): return struct.pack('>I', len(data)) + k + data + struct.pack('>I', zlib.crc32(k + data) & 0xffffffff)
open(sys.argv[2], 'wb').write(b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', W, H, 8, 2, 0, 0, 0)) + chunk(b'IDAT', zlib.compress(raw, 6)) + chunk(b'IEND', b''))
