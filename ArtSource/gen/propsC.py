import json, math
ops=[]
def grid(x,y,legend,rows,w):
    assert all(len(r)==w for r in rows),[len(r) for r in rows]
    ops.append({"kind":"grid","x":x,"y":y,"legend":legend,"rows":rows})
stone={"k":"#10141f","d":"#202e37","s":"#394a50","S":"#577277","L":"#819796","g":"#25562e","G":"#468232","E":"#3c5e8b"}
statue=["................",
"......kkkk......",
".....kSLLSk.....",
"....kSLLLLSk....",
"....kSkkkkSk....",
"....kSkEEkSk....",
"....ksSSSSsk....",
"...kkssSSssk....",
"..kSLsSSSSsLSk..",
"..kSLsSLLSsLSk..",
"..ksSsSLLSssSk..",
"..ksdsSSSSssdk..",
"...kdsSSSSssk...",
"....ksSLLSsk....",
"....ksSLLSsk....",
"....kgSLLSsk....",
"....ksSLLSsk....",
"....ksdLLdsk....",
"....ksd..dsk....",
"....ksd..dsk....",
"....ksd..dsk....",
"...kssd..dssk...",
"...kkkk..kkkk...",
"..kSSSSSSSSSSk..",
"..kSLLLLLLLLSk..",
".kSSSSSSSSSSSSk.",
".ksgsssssssssdk.",
".ksssssssssGssk.",
".kddddddddddddk.",
".kkkkkkkkkkkkkk.",
"................",
"................"]
grid(0,48,stone,statue,16)
gold={"k":"#10141f","N":"#7a4841","T":"#ad7757","H":"#c09473","D":"#4d2b32","G":"#de9e41","Y":"#e8c170","O":"#be772b","R":"#752438","r":"#a53030","m":"#151d28","W":"#ebede9","L":"#c7cfcc","s":"#394a50"}
bigC=["................................",
"................................",
"....kkkkkkkkkkkkkkkkkkkkkkkk....",
"...kGGGGGGGGGGGGGGGGGGGGGGGGk...",
"..kGNTTTTTTTTTTTTTTTTTTTTTTNGk..",
"..kGNTHHHHHHHHHHHHHHHHHHHHTNGk..",
"..kGGGGGGGGGGGGGGGGGGGGGGGGGGk..",
"..kGRRRRRRRRRRkLLkRRRRRRRRRRGk..",
"..kGRrrrrrrrrkLkkLkrrrrrrrrRGk..",
"..kGRrrrrrrrrkLLLLkrrrrrrrrRGk..",
"..kGRrrrrrrrrrkLLkrrrrrrrrrRGk..",
"..kGRRRRRRRRRRRkkRRRRRRRRRRRGk..",
"..kGGGGGGGGGGGGGGGGGGGGGGGGGGk..",
"..kOOOOOOOOOOOOOOOOOOOOOOOOOOk..",
"...kkkkkkkkkkkkkkkkkkkkkkkkkk...",
"................................"]
bigO=["....kkkkkkkkkkkkkkkkkkkkkkkk....",
"...kGNNNNNNNNNNNNNNNNNNNNNNGk...",
"...kGDDDDDDDDDDDDDDDDDDDDDDGk...",
"...kGGGGGGGGGGGGGGGGGGGGGGGGk...",
"..kmYmOYmYOmmYOWYmOYmmYOmOYmmk..",
"..kYOYYOYYOYOYYOYYOYYOYOYYOYYk..",
"..kGGGGGGGGGGGGGGGGGGGGGGGGGGk..",
"..kGRRRRRRRRRRkLLkRRRRRRRRRRGk..",
"..kGRrrrrrrrrkLkkLkrrrrrrrrRGk..",
"..kGRrrrrrrrrkLLLLkrrrrrrrrRGk..",
"..kGRrrrrrrrrrkLLkrrrrrrrrrRGk..",
"..kGRRRRRRRRRRRkkRRRRRRRRRRRGk..",
"..kGGGGGGGGGGGGGGGGGGGGGGGGGGk..",
"..kOOOOOOOOOOOOOOOOOOOOOOOOOOk..",
"...kkkkkkkkkkkkkkkkkkkkkkkkkk...",
"................................"]
grid(16,48,gold,bigC,32)
grid(48,48,gold,bigO,32)
# exit portal: 4 frames 32x32 at cols 5..12 row 3 (x=80..), drawn with ellipses
for f in range(4):
    x0=80+f*32; y0=48
    ops.append({"kind":"ellipse","rect":{"x":x0+3,"y":y0+8,"width":26,"height":18},"color":"#10141f","fill":"#090a14"})
    ops.append({"kind":"ellipse","rect":{"x":x0+4,"y":y0+9,"width":24,"height":16},"color":"#253a5e"})
    ops.append({"kind":"ellipse","rect":{"x":x0+8,"y":y0+12,"width":16,"height":10},"color":"#3c5e8b"})
    ops.append({"kind":"ellipse","rect":{"x":x0+12,"y":y0+14,"width":8,"height":6},"color":"#4f8fba"})
    # rotating sparks along the rim
    pts=[]
    for i in range(6):
        a=(i/6.0)*2*math.pi + f*(math.pi/12)
        pts.append({"x":int(round(x0+16+11*math.cos(a))),"y":int(round(y0+17+7*math.sin(a)))})
    ops.append({"kind":"pixels","color":"#a4dddb","points":pts})
    inner=[]
    for i in range(3):
        a=(i/3.0)*2*math.pi - f*(math.pi/6)
        inner.append({"x":int(round(x0+16+5*math.cos(a))),"y":int(round(y0+17+3*math.sin(a)))})
    ops.append({"kind":"pixels","color":"#73bed3","points":inner})
    ops.append({"kind":"pixels","color":"#ebede9","points":[{"x":x0+16,"y":y0+17}]})
    # rising motes
    for i,(dx,dy) in enumerate([(6,4),(24,2),(14,0),(20,6)]):
        yy=y0+dy+((f*2+i*3)%8)
        ops.append({"kind":"pixels","color":"#73bed3" if i%2 else "#a4dddb","points":[{"x":x0+dx,"y":yy}]})
# arrow (col 13 row 3) pointing right, centred
arrow=["................","................","................","................","................","................","..k.k...........",".kNkNNNNNNNNkLk.","..k.k.......kWk.","...........k....","................","................","................","................","................","................"]
arrow=["................","................","................","................","................","................",
"................",
".kk.........k...",
"kTNNNNNNNNNNkLk.",
".kk.........kk..",
"................","................","................","................","................","................"]
grid(208,48,{"k":"#10141f","N":"#7a4841","T":"#c7cfcc","L":"#a8b5b2","W":"#ebede9"},arrow,16)
shard=["................","................","................","................","................",".......kk.......","......kLLk......",".....kLLlLk.....","......kLlk......",".......kk.......","................","................","................","................","................","................"]
grid(224,48,{"k":"#202e37","L":"#c7cfcc","l":"#a8b5b2"},shard,16)
# elite aura f0 (col15 row3) and f1 (col13 row4): flickering crimson ground ring
for i,(x0,y0) in enumerate([(240,48),(208,64)]):
    ops.append({"kind":"ellipse","rect":{"x":x0+1,"y":y0+10,"width":14,"height":5},"color":"#a53030" if i==0 else "#cf573c"})
    ops.append({"kind":"pixels","color":"#cf573c" if i==0 else "#e8c170","points":[{"x":x0+3,"y":y0+8-i},{"x":x0+12,"y":y0+7+i},{"x":x0+8,"y":y0+6}]})
# death poof 4 frames (row 5 cols 0..3)
poof=[["................","................","................","................","................","................",".......LL.......","......LWWL......","......LWWL......",".......LL.......","................","................","................","................","................","................"],
["................","................","................","................",".....dd.dd......","....dDDdDDd.....","...dDLLLLLDd....","...dDLWWWLDd....","...dDLWWWLDd....","...dDLLLLLDd....","....dDDdDDd.....",".....dd.dd......","................","................","................","................"],
["................","................","...d......d.....","..dDd....dDd....","..dLd.dd.dLd....","....dDDDDd......","...dDL..LDd.....","...dL....Ld.....","...dL....Ld.....","...dDL..LDd.....","....dDDDDd......","..dLd.dd.dLd....","..dDd....dDd....","...d......d.....","................","................"],
["................","..d........d....",".dD........Dd...","................","................","....d.....d.....","................","................","................","................","....d.....d.....","................","................",".dD........Dd...","..d........d....","................"]]
for i,p in enumerate(poof): grid(i*16,80,{"d":"#394a50","D":"#577277","L":"#819796","W":"#c7cfcc"},p,16)
print(json.dumps(ops,separators=(',',':')))
