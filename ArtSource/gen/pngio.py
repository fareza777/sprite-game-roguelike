"""Minimal stdlib PNG read/write (8-bit RGB/RGBA, non-interlaced) for palette remaps."""
import struct
import zlib


def read(path):
    d = open(path, 'rb').read()
    i, idat, w, h, ct = 8, b'', 0, 0, 6
    while i < len(d):
        n, = struct.unpack('>I', d[i:i + 4]); k = d[i + 4:i + 8]; c = d[i + 8:i + 8 + n]; i += 12 + n
        if k == b'IHDR':
            w, h, bd, ct = struct.unpack('>IIBB', c[:10])
        elif k == b'IDAT':
            idat += c
    bpp = 4 if ct == 6 else 3
    raw = zlib.decompress(idat); st = w * bpp; prev = bytearray(st); p = 0; px = []
    for y in range(h):
        f = raw[p]; line = bytearray(raw[p + 1:p + 1 + st]); p += 1 + st
        for x in range(st):
            a = line[x - bpp] if x >= bpp else 0; b = prev[x]; cc = prev[x - bpp] if x >= bpp else 0
            if f == 1: line[x] = (line[x] + a) & 255
            elif f == 2: line[x] = (line[x] + b) & 255
            elif f == 3: line[x] = (line[x] + (a + b) // 2) & 255
            elif f == 4:
                pa = abs(b - cc); pb = abs(a - cc); pc = abs(a + b - 2 * cc)
                line[x] = (line[x] + (a if pa <= pb and pa <= pc else (b if pb <= pc else cc))) & 255
        px.append([tuple(line[x * bpp:x * bpp + bpp]) + ((255,) if bpp == 3 else ()) for x in range(w)])
        prev = line
    return w, h, px


def write(path, w, h, px):
    rows = b''.join(b'\x00' + bytes(v for p in row for v in p) for row in px)
    def chunk(k, data): return struct.pack('>I', len(data)) + k + data + struct.pack('>I', zlib.crc32(k + data) & 0xffffffff)
    open(path, 'wb').write(b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, 6, 0, 0, 0)) +
                          chunk(b'IDAT', zlib.compress(rows, 9)) + chunk(b'IEND', b''))


def hexof(p): return '#%02x%02x%02x' % p[:3]
