import json, random
random.seed(3)
W,H=13,11
floor=set((x,y) for x in range(1,12) for y in range(2,10))   # y grows downward (screen rows)
for p in [(3,4),(8,6)]:
    for dx in (0,1):
        for dy in (0,1): floor.discard((p[0]+dx,p[1]+dy))
def isf(x,y): return (x,y) in floor
ops=[]
def blit(tx,ty,sx,sy):
    ops.append({"kind":"blit","fromLayer":"Layer 1 2","from":{"x":sx*16,"y":sy*16,"width":16,"height":16},"to":{"x":tx*16,"y":ty*16},"skipTransparent":False})
for y in range(H):
    for x in range(W):
        if isf(x,y):
            r=random.random()
            idx = 0 if r<0.35 else 1 if r<0.55 else 2 if r<0.72 else 3 if r<0.8 else 4 if r<0.88 else 5 if r<0.93 else 6 if r<0.97 else 7
            blit(x,y,idx,0); continue
        # wall: face if floor directly below (screen y+1)
        if isf(x,y+1):
            blit(x,y,random.choice([0,0,0,1,2,3]),1); continue
        mask=0
        if isf(x,y-1): mask|=1          # floor north
        if isf(x+1,y): mask|=2
        if isf(x,y+2) and not isf(x,y+1): mask|=4 if False else 0
        if (x,y+1) not in floor and isf(x,y+2): mask|=4   # face below
        if isf(x-1,y): mask|=8
        blit(x,y,mask%8,2+mask//8)
print(json.dumps(ops))
