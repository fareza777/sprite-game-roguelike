import json,sys
sys.path.insert(0,r"D:/Isprite Game Pixel/ArtSource/gen")
from compose import Canvas
exec(open(r"D:/Isprite Game Pixel/ArtSource/gen/skeleton.py",encoding="utf-8").read().split("def frame(")[0])  # reuse parts & legend
L.update({"G":"#25562e","g":"#19332d","v":"#468232","N":"#7a4841","n":"#4d2b32","T":"#c7cfcc","A":"#a8b5b2","F":"#cf573c"})
hood=["...gggg.....",".gGGGGGgg...","gGGvGggGGg..","gGg.........","gGg.........","gGg.........","gGg.........",".gGg........","..gGGGGg....",".gGGGGGGGg.."]
cape=["gG.","gG.","gGg","gGg","gGg","ggG","g.g"]
def bow(c,x,y,drawn=False,arrow=False):
    pts=[(0,0),(1,1),(1,2),(2,3),(2,4),(2,5),(2,6),(2,7),(2,8),(1,9),(1,10),(0,11)]
    for dx,dy in pts: c.g[y+dy][x+dx]='N'
    c.g[y+5][x+2]='n'; c.g[y+6][x+2]='n'
    if drawn:
        c.line(x,y,x-3,y+6,'T'); c.line(x-3,y+6,x,y+11,'T')
        if arrow:
            c.line(x-4,y+6,x+6,y+6,'N'); c.g[y+6][x+7]='A'; c.g[y+6][x+8]='T'; c.g[y+5][x-4]='F'; c.g[y+7][x-4]='F'
    else:
        c.line(x,y,x,y+11,'T')
def frame(sx=0,sy=0,legs=('stand','stand'),pose='hold',hurt=False):
    c=Canvas(32,32); by=sy
    lgs={'stand':leg_stand,'fwd':leg_fwd,'back':leg_back}
    c.paste(cape,10+sx,15+by)
    c.paste(lgs[legs[0]],12+sx,23); c.paste(lgs[legs[1]],16+sx,23)
    c.paste(rib,12+sx,14+by)
    c.paste(skull_hurt if hurt else skull,12+sx-(1 if hurt else 0),6+by)
    c.paste(hood,11+sx-(1 if hurt else 0),5+by)
    if pose=='hold':
        c.paste(arm,18+sx,15+by); bow(c,21+sx,13+by)
    elif pose=='draw':
        c.paste(["BBW","bbW"],18+sx,18+by); bow(c,22+sx,13+by,drawn=True,arrow=True)
        c.paste(["W","B"],17+sx,19+by)
    elif pose=='release':
        c.paste(["BBBW","bbbW"],18+sx,18+by); bow(c,22+sx,13+by)
    c.outline('k'); return c.rows()
def pile(stage):
    c=Canvas(32,32)
    if stage==0:
        c.paste(skull,13,9); c.paste(hood,12,8); c.paste(rib,12,17); c.paste(leg_stand,12,24); c.paste(leg_back,16,24)
    elif stage==1:
        c.paste(skull,11,14); c.paste(hood,10,13); c.paste(rib,13,20); c.paste(["BBBW","bbbW"],19,27)
    elif stage==2:
        c.paste(skull,9,21); c.paste(hood,8,20); c.paste(["bWWWWBb","bddWddb","bWWWWBb"],15,25); c.line(22,28,29,26,'N')
    else:
        c.paste(["gGGGGg","GGggGG"],8,26); c.paste(skull,9,22); c.paste(["bWdWdBb","bddWddb"],15,27); c.line(22,29,29,28,'N')
    c.outline('k'); return c.rows()
frames=[("idle",frame()),("idle",frame(sy=1)),
 ("run",frame(legs=('fwd','back'),sy=1)),("run",frame()),("run",frame(legs=('back','fwd'),sy=1)),("run",frame()),
 ("attack",frame(sx=-1,pose='draw')),("attack",frame(sx=-1,pose='release')),
 ("hurt",frame(sx=-1,hurt=True)),
 ("death",pile(0)),("death",pile(1)),("death",pile(2)),("death",pile(3))]
print(json.dumps({"legend":L,"frames":frames},separators=(',',':')))
