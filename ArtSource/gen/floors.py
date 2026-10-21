import json
F = {}
F[0] = """mmmmmmmmmmmmmmmm
mhhhhhhhhhhhhhhb
mhbbbbbbbbbbbbbb
mhbbbbtbbbbbbbbb
mhbbbbbbbbbbLbbb
mhbbbbbbbbbbbbbb
mhbbbbbbbbbbbbbb
mhbbbbbbbbtbbbbb
mhbLbbbbbbbbbbbb
mhbbbbbbbbbbbbbb
mhbbbbbbbbbbbtbb
mhbbbbtbbbbbbbbb
mhbbbbbbbbbbbbbb
mhbbbbbbbbbLbbbb
mhbbbbbbbbbbbbbb
mbbbbbbbbbbbbbbb"""
F[1] = """mmmmmmmmmmmmmmmm
mhhhhhhhhhhhhhhh
mhbbbbbbbbbbbbbb
mhbbbbbbbLbbbbbb
mhbbtbbbbbbbbbbb
mhbbbbbbbbbbbbbb
mhbbbbbbbbbbbtbb
mbbbbbbbbbbbbbbb
mmmmmmmmmmmmmmmm
mhhhhhhhhhmhhhhh
mhbbbbbbbbmhbbbb
mhbbbbLbbbmhbbbb
mhbbbbbbbbmhbbtb
mhbbtbbbbbmhbbbb
mhbbbbbbbbmhbbbb
mbbbbbbbbbmbbbbb"""
F[2] = """mmmmmmmmmmmmmmmm
mhhhhhhmhhhhhhhh
mhbbbbbmhbbbbbbb
mhbbLbbmhbbbbtbb
mhbbbbbmhbbbbbbb
mhbbbtbmhbbbbbbb
mbbbbbbmbbbbbbbb
mmmmmmmmmmmmmmmm
mhhhhhhhhhhmhhhh
mhbbbbbbbbbmhbbb
mhbbbbbtbbbmhbbb
mhbbbbbbbbbmhbLb
mhbLbbbbbbbmhbbb
mhbbbbbbbbbmhbbb
mhbbbbbbbtbmhbbb
mbbbbbbbbbbmbbbb"""
F[3] = """mmmmmmmmmmmmmmmm
mhhhhhhhhhhhhhhb
mhbbcbbbbbbbbbbb
mhbbbcbbbbbbbbbb
mhbbbcbbbbbLbbbb
mhbbbbccbbbbbbbb
mhbbbbbbcbbbbbbb
mhbbbbbbcbbbbbbb
mhbLbbbbbccbbbbb
mhbbbbbbbbbcbbbb
mhbbbbbbbbbcbbbb
mhbbbbtbbbbbccbb
mhbbbbbbbbbbbbcb
mhbbbbbbbbbbbbcb
mhbbbbbbbbbbbbbc
mbbbbbbbbbbbbbbb"""
F[4] = """mmgmmmmmmmmmmmmm
mGvGhhhhhhhhhhhb
gGGgbbbbbbbbbbbb
mggbbbbbbbbbbbbb
mhgbbbbbbbbbLbbb
mhbbbbbbbbbbbbbb
mhbbbbbbbbbbbbbb
mhbbbbbbbbbbbbbb
mhbLbbbbbbbbgbbb
mhbbbbbbbbbgGgbb
mhbbbbbbbbbbgbbb
mhbbbbtbbbbbbbbb
mhbbbbbbbbbbbbbb
mhbbbbbbbbbLbbbb
mhbbbbbbbbbbbbbb
mbbbbbbbbbbbbbbb"""
F[5] = """mmmmmmmmmmmmmmmm
mhhhhhhhhhhhhhhb
mhbbbbbbbbbbbbbb
mhbbbbbbbbbbbbbb
mhbbbbmmmbbbbbbb
mhbbbmkkkmbbbbbb
mhbbmkkLkkmbbbbb
mhbbmkhkkkmbbbbb
mhbbbmkkkmbbbbbb
mhbbbbmmmbbbbbbb
mhbbbbbbbbbbbtbb
mhbbtbbbbbbbbbbb
mhbbbbbbbbLbbbbb
mhbbbbbbbbbbbbbb
mhbbbbbbbbbbbbbb
mbbbbbbbbbbbbbbb"""
F[6] = """mmmmmmmmmmmmmmmm
mhhhhhhhhhhhhhhb
mhbbbbbbbbbbbbbb
mhbbbbbbbbbbbbbb
mhbbmmmmmmmmbbbb
mhbbmLkLkLkmbbbb
mhbbmLkLkLkmbbbb
mhbbmLkLkLkmbbbb
mhbbmLkLkLkmbbbb
mhbbmLkLkLkmbbbb
mhbbmhhhhhhmbbbb
mhbbmmmmmmmmbbbb
mhbbbbbbbbbbbbbb
mhbbbbbbbbbbLbbb
mhbbbbbbbbbbbbbb
mbbbbbbbbbbbbbbb"""
F[7] = """mmmmmmmmmmmmmmmm
mhhhhhhhhhhhhhhb
mhbbbbbbbbbbbbbb
mhbbbbbrrrbbbbbb
mhbbbbrbbbrbbbbb
mhbbbrbbRbbrbbbb
mhbbbrbRRRbrbbbb
mhbbbrbbRbbrbbbb
mhbbbbrbbbrbbbbb
mhbbbbbrrrbbbbbb
mhbbbbbbbbbbbbbb
mhbbbbtbbbbbbbbb
mhbbbbbbbbbbbbbb
mhbbbbbbbbbLbbbb
mhbbbbbbbbbbbbbb
mbbbbbbbbbbbbbbb"""
legend = {"m":"#151d28","b":"#202e37","h":"#394a50","t":"#151d28","L":"#577277","c":"#10141f","g":"#19332d","G":"#25562e","v":"#468232","k":"#090a14","r":"#253a5e","R":"#3c5e8b"}
ops=[]
for i,txt in F.items():
    rows=txt.split("\n")
    assert len(rows)==16 and all(len(r)==16 for r in rows),(i,[len(r) for r in rows])
    ops.append({"kind":"grid","x":i*16,"y":0,"legend":legend,"rows":rows})
print(json.dumps(ops))
