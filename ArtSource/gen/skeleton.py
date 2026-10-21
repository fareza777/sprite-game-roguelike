import json,sys
sys.path.insert(0,r"D:/Isprite Game Pixel/ArtSource/gen")
from compose import Canvas, op
L={"k":"#10141f","W":"#c7cfcc","B":"#a8b5b2","b":"#819796","d":"#577277","E":"#cf573c","e":"#a53030","R":"#884b2b","r":"#be772b","s":"#577277","S":"#a8b5b2","C":"#4d2b32","c":"#7a4841"}
skull=["..BBBBB..",".BWWWWBb.","BWWWWWWBb","BWkkWWkkb","BWkEWWkEb","BWWWbWWWb",".BWBWBWb.","..bbbbb.."]
skull_hurt=["..BBBBB..","BWWWWWBb.","BWWWWWWBb","BkkWWkkWb","BkEWWkEWb","BWWbWWWWb",".BWBWBWb.","..bbbbb.."]
rib=["...bb...",".bWWWBb.","bWWWWWBb","bddWddBb","bWWWWWBb","bddWddbb",".bWWWBb.","..bbbb..",".cCCCCc.",".Cc..cC."]
leg_stand=[".B.",".B.",".b.",".W.",".B.",".b.",".WW"]
leg_fwd  =["..B.","..B.","..b.","...W","...B","...b","...WW"]
leg_back =[".B..",".B..",".b..","W...","B...","b...","WW.."]
arm=[".B.",".b.",".B.",".W.",".B.","WWB"]
def blade_down(c,hx,hy):   # rusty sword held low pointing forward-down
    c.line(hx+1,hy+1,hx+6,hy+5,'S'); c.line(hx+1,hy+2,hx+5,hy+5,'s'); c.g[hy+6][hx+7]='r' if hy+6<c.h and hx+7<c.w else None
    c.paste(["R","r","R"],hx,hy-1)
def frame(sx=0,sy=0,legs=('stand','stand'),arm_pose='down',hurt=False,blade=None):
    c=Canvas(32,32)
    by=sy
    # far leg, far arm first
    lgs={'stand':leg_stand,'fwd':leg_fwd,'back':leg_back}
    c.paste(lgs[legs[0]],12+sx,23,)
    c.paste(lgs[legs[1]],16+sx,23)
    if arm_pose=='up':   # windup: blade raised behind the head, drawn before the skull
        c.line(19+sx,11+by,12+sx,3+by,'S'); c.line(20+sx,11+by,13+sx,3+by,'s')
        c.paste(["Rr","rR"],18+sx,11+by)
    c.paste(rib,12+sx,14+by)
    c.paste(skull_hurt if hurt else skull,12+sx-(1 if hurt else 0),6+by)
    if arm_pose=='up':
        c.paste([".W.",".B.",".b.",".B.",".W."],18+sx,12+by)
    # sword arm (front)
    if arm_pose=='down':
        c.paste(arm,18+sx,15+by)
        c.line(20+sx,21+by,26+sx,26+by,'S'); c.line(20+sx,22+by,25+sx,26+by,'s')
        c.paste(["Rr","rR"],19+sx,20+by)
    elif arm_pose=='fwd':  # strike: arm and blade extended forward
        c.paste(["BBBW","bbbW"],18+sx,17+by)
        c.line(22+sx,17+by,30+sx,17+by,'S'); c.line(22+sx,18+by,29+sx,18+by,'s')
        c.paste(["R","r"],21+sx,17+by)
    elif arm_pose=='follow':
        c.paste(arm,18+sx,15+by)
        c.line(20+sx,21+by,27+sx,24+by,'S'); c.line(20+sx,22+by,26+sx,24+by,'s')
        c.paste(["Rr","rR"],19+sx,20+by)
    c.outline('k')
    return c.rows()
def pile(stage):
    c=Canvas(32,32)
    if stage==0:
        c.paste(skull,13,9); c.paste(rib,12,17); c.paste(leg_stand,12,24); c.paste(leg_back,16,24)
    elif stage==1:
        c.paste(skull,11,14); c.paste(rib,13,20); c.paste(["BBBW","bbbW"],19,27); c.paste(["WbbB"],10,28)
    elif stage==2:
        c.paste(skull,9,21); c.paste(["bWWWWBb","bddWddb","bWWWWBb"],15,25); c.paste(["WbbB","BbbW"],22,27); c.line(23,29,29,27,'S')
    else:
        c.paste(skull,8,22); c.paste(["bWdWdBb","bddWddb"],15,27); c.paste(["WbbB","BbbW"],22,28); c.line(23,29,29,28,'S'); c.paste(["W.B"],13,29)
    c.outline('k'); return c.rows()
frames=[
 ("idle",frame()),("idle",frame(sy=1)),
 ("run",frame(legs=('fwd','back'),sy=1)),("run",frame()),("run",frame(legs=('back','fwd'),sy=1)),("run",frame()),
 ("windup",frame(sx=-1,arm_pose='up')),
 ("attack",frame(sx=2,legs=('back','fwd'),arm_pose='fwd')),("attack",frame(sx=1,legs=('back','fwd'),arm_pose='follow')),
 ("hurt",frame(sx=-1,hurt=True)),
 ("death",pile(0)),("death",pile(1)),("death",pile(2)),("death",pile(3)),
]
out={"legend":L,"frames":frames}
print(json.dumps(out,separators=(',',':')))
