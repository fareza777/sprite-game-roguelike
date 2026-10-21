import json,sys,math
sys.path.insert(0,r"D:/Isprite Game Pixel/ArtSource/gen")
from compose import Canvas
L={"k":"#10141f","D":"#25562e","M":"#468232","G":"#75a743","H":"#a8ca58","S":"#d0da91","b":"#819796","e":"#10141f","w":"#d0da91","g":"#19332d"}
def blob(rx,ry,cx=16,base=28,lift=0,eyes=True,hurt=False):
    c=Canvas(32,32)
    cy=base-ry-lift
    for y in range(32):
        for x in range(32):
            dx=(x+0.5-cx)/rx; dy=(y+0.5-(cy+0.5))/ry
            if y>base-lift: continue
            if dx*dx+dy*dy<=1.0:
                # shade: lower-right darker, upper-left lighter
                t=dx*0.6+dy*0.8
                c.g[y][x]='D' if t>0.5 else ('M' if t>-0.25 else ('G' if t>-0.8 else 'H'))
    # flat-ish bottom
    for x in range(32):
        if c.g[base-lift][x]!='.' if 0<=base-lift<32 else False: pass
    # inner skull silhouette
    sx,sy=cx-2,cy+1
    for (dx,dy) in [(0,0),(1,0),(2,0),(-1,1),(3,1),(0,2),(2,2)]:
        X,Y=sx+dx,sy+dy
        if 0<=Y<32 and c.g[Y][X] in 'MG': c.g[Y][X]='b'
    # specular
    hx,hy=cx-int(rx*0.5),cy-int(ry*0.45)
    for dx,dy,ch in [(0,0,'S'),(1,0,'S'),(0,1,'H')]:
        if c.g[hy+dy][hx+dx]!='.': c.g[hy+dy][hx+dx]=ch
    if eyes:
        ey=cy-int(ry*0.1)
        ex=cx+int(rx*0.15)
        if hurt:
            for X in (ex,ex+3):
                c.g[ey][X]='e'; c.g[ey-1][X-1]='e'; c.g[ey+1][X+1]='e'
        else:
            for X in (ex,ex+3):
                c.g[ey][X]='e'; c.g[ey+1][X]='e'; c.g[ey][X+1]='e' if c.g[ey][X+1]!='.' else c.g[ey][X+1]
                c.g[ey][X]='w'
    c.outline('k')
    # ground shadow under airborne blobs
    if lift>0:
        for x in range(cx-rx+2,cx+rx-2): c.g[base][x]='g'
    return c.rows()
def splash(stage):
    c=Canvas(32,32)
    if stage==0: return blob(12,5,base=28)
    drops={1:[(8,24),(24,23),(12,20),(20,19),(16,26)],2:[(5,25),(27,24),(10,17),(22,16),(16,27),(13,27),(19,27)],3:[(4,27),(28,27),(9,26),(23,26),(14,28),(18,28)]}[stage]
    for x,y in drops:
        for dx,dy in [(0,0),(1,0),(0,1),(1,1)]:
            c.g[y+dy][x+dx]='M' if (dx+dy)%2 else 'G'
    if stage>=2:
        for x in range(11,22): c.g[28][x]='D'
        for x in range(12,21): c.g[27][x]='M'
    c.outline('k'); return c.rows()
frames=[("idle",blob(10,8)),("idle",blob(11,7)),
 ("windup",blob(12,6)),
 ("attack",blob(8,10,lift=4)),("attack",blob(9,9,lift=2)),
 ("hurt",blob(11,7,hurt=True)),
 ("death",splash(0)),("death",splash(1)),("death",splash(2)),("death",splash(3))]
print(json.dumps({"legend":L,"frames":frames},separators=(',',':')))
