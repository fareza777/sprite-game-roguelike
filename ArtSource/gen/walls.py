import json
W = {}
W[0] = """cccccccccccccccc
LLLLLLLLLLLLLLLL
mmmmmmmmmmmmmmmm
LBBBBBBmLBBBBBBm
BBBBBBBmBBBBBBBm
BBBBBBDmBBBBBBDm
DDDDDDDmDDDDDDDm
mmmmmmmmmmmmmmmm
BBBmLBBBBBBmLBBB
BBBmBBBBBBBmBBBB
BBDmBBBBBBDmBBBB
DDDmDDDDDDDmDDDD
mmmmmmmmmmmmmmmm
DDDDDDDkDDDDDDDk
kDDDDDDkkDDDDDDk
kkkkkkkkkkkkkkkk"""
W[1] = """cccccccccccccccc
LLLLLLLLLLLLLLLL
mmmmmmmmmmmmmmmm
LBBBBBBmLBBmBBBm
BBBBBBBmBBBBmBBm
BBBBBBDmBBBmBBDm
DDDDDDDmDDDDmDDm
mmmmmmmmmmmmmmmm
BBBmLBBmBBBmLBBB
BBBmBBBBmBBmBBBB
BBDmBBBmBBDmBBBB
DDDmDDDDDDDmDDDD
mmmmmmmmmmmmmmmm
DDDDDDDkDDDDDDDk
kDDDDDDkkDDDDDDk
kkkkkkkkkkkkkkkk"""
W[2] = """cccccccccccccccc
LGvGLLLLLLLLGvLL
mgGgmmmmmmmmgGmm
LgGgBBBmLBBBBgBm
BBgBBBBmBBBBBgBm
BBgBBBDmBBBBBBDm
DDgDDDDmDDDDDDDm
mmmmmmmmmmmmmmmm
BBBmLBBBBBBmLBBB
BBBmBBBBBBBmBBBB
BBDmBBBBBBDmBBBB
DDDmDDDDDDDmDDDD
mmmmmmmmmmmmmmmm
DDDDDDDkDDDDDDDk
kDDDDDDkkDDDDDDk
kkkkkkkkkkkkkkkk"""
W[3] = """cccccccccccccccc
LLLLLLLLLLLLLLLL
mmmmmmmmmmmmmmmm
LBBBBBBmLBBBBBBm
BBBBBBBmBBBBBBBm
BBBBBBDmBBBBBBDm
DDDDDDDmDDDDDDDm
mmmmmmmmmmmmmmmm
BBBmkkkkkkkmLBBB
BBBmkkkkkkkmBBBB
BBDmkkkLkkkmBBBB
DDDmkkkkkkkmDDDD
mmmmmmmmmmmmmmmm
DDDDDDDkDDDDDDDk
kDDDDDDkkDDDDDDk
kkkkkkkkkkkkkkkk"""
legend = {"c":"#819796","L":"#577277","B":"#394a50","D":"#202e37","m":"#151d28","k":"#10141f","g":"#19332d","G":"#25562e","v":"#468232"}
ops=[]
for i,t in W.items():
    rows=t.split("\n"); assert len(rows)==16 and all(len(r)==16 for r in rows),(i,[len(r) for r in rows])
    ops.append({"kind":"grid","x":i*16,"y":16,"legend":legend,"rows":rows})

# Wall caps: dark stone tops; rims on sides facing open space. Mask bits N=1,E=2,S=4,W=8
base = """kkknkkkkkkkknkkk
kkkkkkknkkkkkkkk
knkkkkkkkkkkkkkn
kkkkkkkkkknkkkkk
kkkkknkkkkkkkkkk
kkkkkkkkkkkkknkk
kknkkkkkkkkkkkkk
kkkkkkkknkkkkkkk
kkkkkkkkkkkkkkkk
knkkkkkkkkknkkkk
kkkkkknkkkkkkkkk
kkkkkkkkkkkkkkkn
kkknkkkkkkkkkkkk
kkkkkkkkknkkkkkk
kkkkkkkkkkkkkkkk
kknkkkkkkkkknkkk""".split("\n")
cap_ops=[]
for mask in range(16):
    g=[list(r) for r in base]
    if mask & 1:  # floor to the north: lit rim
        for x in range(16): g[0][x]='h'; g[1][x]='D'
    if mask & 8:  # floor to the west: lit rim
        for y in range(16): g[y][0]='h'; g[y][1]= 'D' if g[y][1] not in 'h' else g[y][1]
    if mask & 2:  # floor to the east: shadow rim
        for y in range(16): g[y][15]='D'; g[y][14]= 'n' if g[y][14] not in 'hD' else g[y][14]
    if mask & 4:  # face below: top lip
        for x in range(16): g[15][x]='D' if g[15][x]!='h' else 'h'; g[14][x]= 'n' if g[14][x] not in 'hD' else g[14][x]
    rows=["".join(r) for r in g]
    col, row = mask % 8, 2 + mask // 8
    cap_ops.append({"kind":"grid","x":col*16,"y":row*16,"legend":{"k":"#10141f","n":"#151d28","h":"#394a50","D":"#202e37"},"rows":rows})
print(json.dumps(ops+cap_ops))
