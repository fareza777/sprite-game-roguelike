import json
E="."*16
def pad(rows):
    rows=list(rows)+[E]*(16-len(rows)); return rows
def at(top, rows): return pad([E]*top+rows)
cells=[]
shards=[at(5,[".....T..H.......","...T..kNT..T....","....NTHNTk......","..H..TNNT..N....","....kNTNTk......","...N..TkT..H....","......N........."]),
        pad([E,E,E,"..T.........H...",E,".....H....N.....",E,"N..............T",E,"....T......N....",E,".H.......T......",E,"......N....H...."]),
        at(12,["...Nk.....T.....","..TN..Hk...NT...","...........k...."])]
for i,s in enumerate(shards): cells.append((1+i,1,s))
chestC=at(4,["...kkkkkkkkkk...","..kNTTTTTTTTNk..","..kNTHHHHHHTNk..","..kGGGGGGGGGGk..","..kNNNNGYGNNNk..","..kNTTTGGGTTNk..","..kNTTTTTTTTNk..","..kGGGGGGGGGGk..","..kDNNNNNNNNDk..","...kkkkkkkkkk..."])
chestO=at(2,["...kkkkkkkkkk...","..kNNNNNNNNNNk..","..kDDDDDDDDDDk..","..kGGGGGGGGGGk..","..kmYmOYmYOmmk..","..kYOYYOYYOYOk..","..kNNNNGYGNNNk..","..kNTTTGGGTTNk..","..kNTTTTTTTTNk..","..kGGGGGGGGGGk..","..kDNNNNNNNNDk..","...kkkkkkkkkk..."])
cells += [(4,1,chestC),(5,1,chestO)]
plate=["..kkkkkkkkkkkk..","..kIiiiiiiiiik..","..kiOiiOiiOiik..","..kiiiiiiiiiik..","..kiiiiiiiiiik..","..kiOiiOiiOiik..","..kiiiiiiiiiik..","..kiiiiiiiiiik..","..kiOiiOiiOiik..","..kiiiiiiiiiik..","..kDDDDDDDDDDk..","..kkkkkkkkkkkk.."]
hidden=at(2,plate)
holes=[(x,y) for y in (4,7,10) for x in (4,7,10)]
warn=[list(r) for r in hidden]
for x,y in holes: warn[y][x]='l'
up=[list(r) for r in hidden]
for x,y in holes:
    up[y][x]='l'; up[y-1][x]='L'; up[y-2][x]='W'
cells += [(6,1,hidden),(7,1,["".join(r) for r in warn]),(8,1,["".join(r) for r in up])]
bars=[]
for y in range(16):
    if y in (3,11): bars.append("IIIIIIIIIIIIIIII")
    elif y in (4,12): bars.append("kiikkiikkiikkiik")
    elif y==15: bars.append(".k...k...k...k..")
    else: bars.append(".Ii..Ii..Ii..Ii.")
cells.append((9,1,bars))
potion=at(2,["......kkkk......","......kNNk......","......kLlk......",".....kLlllk.....","....kLrrrrlk....","...kLrcWrrrlk...","...krcWrrrrRk...","...krrrrrrrRk...","...kRrrrrrRRk...","....kRRRRRRk....",".....kkkkkk....."])
cells.append((14,1,potion))
coin0=at(4,["......kkkk......",".....kYYYOk.....",".....kYOOGk.....",".....kYOOGk.....",".....kOGGGk.....","......kkkk......"])
coin1=at(4,[".......kk.......","......kYOk......","......kYGk......","......kYGk......","......kOGk......",".......kk......."])
coin2=at(4,[".......k........",".......Y........",".......O........",".......O........",".......G........",".......k........"])
coin3=[r[::-1] for r in coin1]
cells += [(15,1,coin0),(0,2,coin1),(1,2,coin2),(2,2,coin3)]
sword=pad([E,"............kkk.","...........kWLk.","..........kWLk..",".........kWLk...","........kWLk....",".......kWLk.....","......kWLk......","..k..kWLk.......","..kGkWLk........","...kGLk.........","...kNGGk........","..kNk.kGk.......",".kYk...k........","..k.............",E])
great=pad(["...........kkkk.","..........kWWLk.",".........kWWLLk.","........kWWLLk..",".......kWWLLk...","......kWWLLk....",".....kWWLLk.....","..kk.kWWLLk.....","..kGkWWLLk......","...kGrLLk.......","...kkGGGk.......","..kNkkGGk.......",".kNk..kGk.......","kYk....k........",".k..............",E])
armor=at(2,["....kk....kk....","...kLLk..kLLk...","..kLlLLkkLLlLk..","..kLlLLLLLLlMk..","..kkLlLLLLlMkk..","....kLLGGLMk....","....kLlLLlMk....","....kLlLLlMk....","....kMLLLMMk....",".....kMMMMk.....","......kkkk......"])
tabard=at(2,["....kk....kk....","...kLLk..kLLk...","..kLlLrrrrlLMk..","..kLlrrcrrrlMk..","..kkLrrrrrrMkk..","....krGGGGrk....","....krrrrrrk....","....kRrrrrRk....","....kRRrrRRk....",".....kRkkRk.....","......k..k......"])
ring=at(3,["......kkkk......",".....kEeeEk.....","......kEEk......",".....kGYYGk.....","....kYk..kGk....","....kGk..kGk....","....kGk..kOk....",".....kGOOOk.....","......kkkk......"])
amulet=at(1,["...I........I...","....I......I....",".....I....I.....","......I..I......",".......II.......","......kGGk......",".....kGrrGk.....",".....kGrcGk.....",".....kGrrGk.....","......kGGk......",".......kk......."])
helm=at(3,[".....kkkkk......","....kLLLLLk.....","...kLLLLLlMk....","...kLLLlMMMk....","...kLlkkkkkk....","...kLMkEEkkk....","...kMMMMMMMk....","...kMmMmMmMk....","....kkkkkkk....."])
for i,icon in enumerate([sword,great,armor,tabard,ring,amulet,helm]): cells.append((3+i,2,icon))
sp0=at(4,["........Y.......","...Y....W....Y..","....W...W...W...",".....WWWWWWW....","..YWWWWWWWWWWY..",".....WWWWWWW....","....W...W...W...","...Y....W....Y..","........Y......."])
sp1=at(2,["........W.......","....Y.......Y...",E,"..W...........W.",E,".Y.............Y",E,"..W...........W.",E,"....Y.......Y...","........W......."])
sp2=at(3,["........Y.......",E,"...Y.........Y..",E,E,"Y...............",E,E,"...Y.........Y..",E,"........Y......."])
for i,s in enumerate([sp0,sp1,sp2]): cells.append((10+i,2,s))
d0=at(9,["......dd........",".....dDDd.......","....dDLLDd......",".....dDDd.......","......dd........"])
d1=at(7,["....dd..dd......","...dDDddDDd.....","..dDLLDDLLDd....","...dDDDDDDd.....","....dd.ddd......"])
d2=at(6,["...d.....d......","..d.D...d.D.....","....d.....d.....","........d.......","..d.............","......d...d....."])
for i,s in enumerate([d0,d1,d2]): cells.append((13+i,2,s))
legend={"k":"#10141f","i":"#394a50","I":"#577277","N":"#7a4841","T":"#ad7757","H":"#c09473","D":"#4d2b32","G":"#de9e41","Y":"#e8c170","O":"#be772b","m":"#151d28","l":"#a8b5b2","L":"#c7cfcc","W":"#ebede9","r":"#a53030","c":"#cf573c","R":"#752438","E":"#73bed3","e":"#a4dddb","M":"#819796"}
# per-cell legend tweaks
spikeleg=dict(legend); spikeleg["O"]="#090a14"; spikeleg["D"]="#202e37"
coinleg=dict(legend); coinleg["k"]="#884b2b"; coinleg["G"]="#be772b"; coinleg["O"]="#de9e41"
shardleg=dict(legend); shardleg["k"]="#4d2b32"
dustleg={"d":"#394a50","D":"#577277","L":"#819796"}
armleg=dict(legend); armleg["M"]="#577277"; armleg["l"]="#a8b5b2"; armleg["L"]="#c7cfcc"
helmleg=dict(legend); helmleg["M"]="#577277"; helmleg["m"]="#394a50"
ops=[]
for x,y,rows in cells:
    assert len(rows)==16 and all(len(r)==16 for r in rows),(x,y,[len(r) for r in rows])
    lg=legend
    if y==1 and x in (6,7,8): lg=spikeleg
    if (y==1 and x==15) or (y==2 and x in (0,1,2)): lg=coinleg
    if y==1 and x in (1,2,3): lg=shardleg
    if y==2 and x in (13,14,15): lg=dustleg
    if y==2 and x in (5,6): lg=armleg
    if y==2 and x==9: lg=helmleg
    ops.append({"kind":"grid","x":x*16,"y":y*16,"legend":lg,"rows":rows})
# spawn marker: 4 frames of a growing summoning circle at cells (10..13,1)
for f in range(4):
    cx=(10+f)*16
    r=[2,4,6,6][f]
    ops.append({"kind":"ellipse","rect":{"x":cx+8-r,"y":16+8-r//2-1,"width":r*2,"height":max(2,r)+1},"color":"#a23e8c"})
    if f>=2: ops.append({"kind":"ellipse","rect":{"x":cx+8-r+2,"y":16+8-r//2,"width":r*2-4,"height":max(2,r)-1},"color":"#7a367b"})
    if f==3:
        ops.append({"kind":"pixels","color":"#df84a5","points":[{"x":cx+8,"y":16+4},{"x":cx+8,"y":16+5},{"x":cx+7,"y":16+8},{"x":cx+8,"y":16+8},{"x":cx+9,"y":16+8},{"x":cx+8,"y":16+9}]})
print(json.dumps(ops,separators=(',',':')))
