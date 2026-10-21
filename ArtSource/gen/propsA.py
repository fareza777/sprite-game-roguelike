import json
def base_torch(flame):
    tail=["......nNNn......",".......NN.......",".....kiNNik.....",".....kIiiIk.....","......kiik......",".......kk.......","................"]
    rows=flame+tail
    return rows
T=[]
T.append(["................","........r.......",".......cr.......","......ccO.......","......cOOc......",".....cOYOc......",".....cOYYOc.....",".....cOYYOc.....","......cOOc......"])
T.append(["........cr......","........cr......",".......cOc......","......cOOc......","......cOYOc.....",".....cOYYOc.....",".....cOYYOc.....",".....cOYYOc.....","......cOOc......"])
T.append(["................","................",".......r........","......cOc.r.....",".....cOYOcc.....",".....cOYYOc.....","....cOYYYOc.....","....cOYYYOc.....","......cOOc......"])
T.append(["......rc........","......rc........","......cOc.......","......cOOc......",".....cOYOc......",".....cOYYOc.....",".....cOYYOc.....",".....cOYYOc.....","......cOOc......"])
torch=[base_torch(f) for f in T]
bowl=["..kkkkkkkkkkk...","..kIiiiiiiiik...","...kiiiiiiik....","....kkkkkkk.....",".....k...k......","....k.....k.....","....k.....k.....","...kk.....kk...."]
B=[]
B.append(["................",".......r........","......cr...r....","....r.cOc..c....","....c.cOOc.c....","...cOcOYOcOc....","...cOOYYYOOc....","...cOYYWYYOc...."])
B.append(["......r.........","......cr..r.....","....r.cOc.c.....","....c.cOOcOc....","...cOcOYYOOc....","...cOOYYYYOc....","...cOYYWWYOc....","...cOYYWYYOc...."])
B.append(["................","........r.......","...r...cr.......","...c..cOOc.r....","...cOcOYOccOc...","...cOOYYYOOc....","...cOYYWYYOc....","...cOYYWYYOc...."])
B.append(["....r...........","....cr.....r....","....cOc...cr....","...cOOc.cOOc....","...cOYOcOYOc....","...cOYYYYYOc....","...cOYWWYYOc....","...cOYYWYYOc...."])
brazier=[b+bowl for b in B]
candle_body=["...kLk..kLk.kLk.","...kLk..kLk.kLk.","...kLlk.kLk.kLk.","..kLLLk.kLlkkLk.","..kkkkk.kkkkkkk.","................","................"]
C=[["................","................","................","................","................","................","....Y....Y......","....O...YO...Y..","...kLk..kOk..O.."],
   ["................","................","................","................","................","................","....Y.....Y.....","...YO....YO..Y..","...kOk...kOk.O.."],
   ["................","................","................","................","................","................",".....Y...Y......","....OY...OY..Y..","...kLk..kOk..YO."]]
cand=[]
for c in C:
    rows=c+candle_body
    rows=[r if r else "." for r in rows]
    cand.append(rows)
# fix candle row 9 (index 8) shared: ok
banner=["................","...kiiiiiiiik...","....kRrrrrRk....","....kRrrrrRk....","....kRrGGrRk....","....kRGrrGRk....","....kRrGGrRk....","....kRrrrrRk....","....kRrrrrRk....","....kRRrrRRk....","....kRRrRRRk....","....kRk.kRRk....",".....k...kk.....","................","................","................"]
chains=["....I......I....","...k.k....k.k...","....I......I....","...k.k....k.k...","....I......I....","...k.k....k.k...","....I......I....","...k.k....k.k...","....I......I....","...k.k....k.k...","....I......I....","...kIk....kIk...","...kik....kik...","....k......k....","................","................"]
niche=["................","................","....kkkkkkk.....","...kmmmmmmmk....","...kmOOOOOmk....","...kmOLLLOmk....","...kmLOLOLmk....","...kmOLLLOmk....","...kmmLlLmmk....","...kkkkkkkkk....","................","................","................","................","................","................"]
pot=["................","................","................",".....kkkkkk.....",".....kTTTTk.....","......kNNk......",".....kNTTNk.....","....kNTHTTNk....","...kNTHTTTTNk...","...kNTTTTTTNk...","...kNNTTTTNNk...","...kDNNNNNNDk...","....kDDNNDDk....",".....kkkkkk.....","................","................"]
barrel=["................","................","....kkkkkkkk....","...kTHHHHHHTk...","...kiiiiiiiik...","...kNTHTTTTNk...","...kNTHTTTTNk...","...kNTTTTTTNk...","...kiiiiiiiik...","...kNTTTTTTNk...","...kNTTTTTTNk...","...kDNNNNNNDk...","...kiiiiiiiik...","....kkkkkkkk....","................","................"]
crate=["................","................","...kkkkkkkkkk...","...kTTTTTTTTk...","...kHTTTTTTTk...","...kkkkkkkkkk...","...kNNNNNNNNk...","...kNDNNNNDNk...","...kNNDNNDNNk...","...kNNNDDNNNk...","...kNNDNNDNNk...","...kNDNNNNDNk...","...kDDDDDDDDk...","...kkkkkkkkkk...","................","................"]
legend={"k":"#10141f","i":"#394a50","I":"#577277","n":"#4d2b32","N":"#7a4841","r":"#a53030","c":"#cf573c","O":"#de9e41","Y":"#e8c170","W":"#ebede9",
        "L":"#c7cfcc","l":"#a8b5b2","R":"#752438","G":"#de9e41","m":"#151d28","T":"#ad7757","H":"#c09473","D":"#4d2b32"}
# chains use I/i/k; candles: L wax
cells=[]
for i,f in enumerate(torch): cells.append((i,0,f))
for i,f in enumerate(brazier): cells.append((4+i,0,f))
for i,f in enumerate(cand): cells.append((8+i,0,f))
cells += [(11,0,banner),(12,0,chains),(13,0,niche),(14,0,pot),(15,0,barrel),(0,1,crate)]
ops=[]
for x,y,rows in cells:
    assert len(rows)==16 and all(len(r)==16 for r in rows),(x,y,len(rows),[len(r) for r in rows])
    ops.append({"kind":"grid","x":x*16,"y":y*16,"legend":legend,"rows":rows})
print(json.dumps(ops,separators=(',',':')))
