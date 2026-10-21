"""Write generator JSON frames as a 1x RGBA horizontal strip PNG (stdlib only). usage: strip.py in.json out.png"""
import json, sys, zlib, struct
d = json.load(open(sys.argv[1])); L = d['legend']; F = d['frames']
fw = len(F[0][1][0]); fh = len(F[0][1]); W = fw * len(F)
rows = []
for y in range(fh):
    line = bytearray(b'\x00')
    for t, g in F:
        for ch in g[y]:
            if ch == '.': line += b'\x00\x00\x00\x00'
            else:
                h = L[ch]; line += bytes(int(h[i:i + 2], 16) for i in (1, 3, 5)) + b'\xff'
    rows.append(bytes(line))
def chunk(k, data): return struct.pack('>I', len(data)) + k + data + struct.pack('>I', zlib.crc32(k + data) & 0xffffffff)
open(sys.argv[2], 'wb').write(b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', W, fh, 8, 6, 0, 0, 0)) + chunk(b'IDAT', zlib.compress(b''.join(rows), 9)) + chunk(b'IEND', b''))
print(W, fh, len(F))
