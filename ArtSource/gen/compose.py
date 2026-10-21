import json
class Canvas:
    def __init__(s,w,h): s.w=w; s.h=h; s.g=[['.']*w for _ in range(h)]
    def paste(s,rows,x,y,flip=False):
        for j,r in enumerate(rows):
            if flip: r=r[::-1]
            for i,ch in enumerate(r):
                if ch=='.': continue
                X,Y=x+i,y+j
                if 0<=X<s.w and 0<=Y<s.h: s.g[Y][X]=ch
        return s
    def line(s,x0,y0,x1,y1,ch):
        dx=abs(x1-x0); dy=-abs(y1-y0); sx=1 if x0<x1 else -1; sy=1 if y0<y1 else -1; err=dx+dy
        while True:
            if 0<=x0<s.w and 0<=y0<s.h: s.g[y0][x0]=ch
            if x0==x1 and y0==y1: break
            e2=2*err
            if e2>=dy: err+=dy; x0+=sx
            if e2<=dx: err+=dx; y0+=sy
        return s
    def outline(s,ch='k',skip='k'):
        # 4-neighbour outside outline around non-transparent pixels
        add=[]
        for y in range(s.h):
            for x in range(s.w):
                if s.g[y][x]!='.': continue
                for dx,dy in ((1,0),(-1,0),(0,1),(0,-1)):
                    X,Y=x+dx,y+dy
                    if 0<=X<s.w and 0<=Y<s.h and s.g[Y][X] not in '.':
                        add.append((x,y)); break
        for x,y in add: s.g[y][x]=ch
        return s
    def rows(s): return ["".join(r) for r in s.g]
def op(rows,legend,x=0,y=0): return {"kind":"grid","x":x,"y":y,"legend":legend,"rows":rows}
