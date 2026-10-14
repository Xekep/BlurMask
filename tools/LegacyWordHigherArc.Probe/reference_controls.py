"""Independent byte-level 20e6a0 request/controller interpreter.

Full graph transition callbacks are deliberately scripted, not a guessed port.
The real AK category profile is independently decoded. Portable symbolic IDs
are canonical paths and are rebound to the managed context container by probe.
"""
import ast
import json
import random
import struct
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
RNG = random.Random(0x20e6a0)
IMP = 0x4000000

def functions_from(relative, names):
    tree = ast.parse((ROOT / relative).read_text(encoding="utf-8-sig"))
    selected = []
    for statement in tree.body:
        if isinstance(statement, ast.FunctionDef) and statement.name in names:
            selected.append(statement)
        if isinstance(statement, ast.Assign) and any(isinstance(t, ast.Name) and t.id in names for t in statement.targets):
            selected.append(statement)
    scope = {"struct": struct, "IMP": IMP}
    exec(compile(ast.Module(body=selected, type_ignores=[]), relative, "exec"), scope)
    return scope

compose = functions_from("tools/LegacyWordContextAlternatives.Probe/reference_controls.py", {"compose", "actions"})["compose"]
merge = functions_from("tools/LegacyWordSecondaryRecombination.Probe/reference_controls.py", {"merge", "cost", "key"})["merge"]

def u16(data, at): return struct.unpack_from("<H", data, at)[0]
def u32(data, at): return struct.unpack_from("<I", data, at)[0]
def i32(data, at): return struct.unpack_from("<i", data, at)[0]
def u64(data, at): return struct.unpack_from("<Q", data, at)[0]
def wrap(value): return (value + 0x80000000) % 0x100000000 - 0x80000000
def w16(data, at, value): struct.pack_into("<H", data, at, value & 0xffff)
def w32(data, at, value): struct.pack_into("<I", data, at, value & 0xffffffff)
def w64(data, at, value): struct.pack_into("<Q", data, at, value)
def s8(value): return value - 256 if value >= 128 else value

def profile(offset):
    data = (ROOT / "models/ru-RU/ru_RU-ak-cur.lite.res").read_bytes()
    assert u16(data, offset) == 0x201
    count = u16(data, offset+2)+3
    pos = offset+4
    def ids():
        nonlocal pos
        n = data[pos]; pos = (pos+4) & ~3
        values = list(struct.unpack_from("<"+"i"*n, data, pos)); pos += 4*n
        return values
    first = ids()
    fm = list(struct.unpack_from("<"+"b"*count, data, pos)); pos += count
    geom = ids()
    start = list(struct.unpack_from("<"+"b"*count, data, pos)); pos += count
    end = list(struct.unpack_from("<"+"b"*count, data, pos))
    categories = []
    for a, b, c in zip(fm, start, end):
        for x, y in [(a,b), (a,c), (first.index(0),b), (first.index(0),c)]:
            if x >= 0 and y >= 0 and (first[x],y) not in categories:
                categories.append((first[x],y))
    assert len(categories) == 35
    return categories

def generate(index):
    categories = profile([11571,188222][index%2])
    pc = [1,2,3,20][index%4]; sc = [2,3,4,20][index%4]
    limit = 2; n = 7
    paths = [[]]
    def ident(path):
        path = list(path)
        if path not in paths: paths.append(path)
        return paths.index(path)
    suffix = [[7],[22],[70],[0xfff7],[0xfff8],[0xfff9],[0xfffb],[0xfffc],[0xfffd]][index%9]
    suffix_id = ident(suffix)
    headers = [((k%10+1)<<9) | (RNG.randrange(1<<20) & ~0x1e00) for k in range(8)]
    slot = bytearray(RNG.randbytes(128))
    node = [100,0,7,255][index%4] if index%8 else 0
    if index%19==0: node=70000
    span = [-1,0,1,3][index%4]
    if node < span+1: node = span+5
    previous = node-span-1
    original_arc = 2+index%5
    slot[4] = 0; slot[26] = RNG.randrange(35); slot[32] = span&255
    slot[33] = RNG.randrange(64); slot[39] = RNG.randrange(256)
    w64(slot,8,suffix_id); w16(slot,28,23); w16(slot,30,original_arc)
    w16(slot,24,node if index%3 else node+1); w16(slot,36,190)
    w32(slot,16,RNG.choice([5000,IMP-1,-100,2147483647,-2147483648]))
    w32(slot,20,RNG.randrange(-200,200)); slot[38] = RNG.randrange(255)
    records = []; pools = []; scripts = []
    state = RNG.randrange(64); identity = RNG.getrandbits(64)
    for arc in range(n):
        record = bytearray(RNG.randbytes(128))
        record[4] = arc; record[26] = RNG.randrange(35); record[33] = state
        record[7] = RNG.randrange(256); record[38] = RNG.randrange(12)
        w16(record,24,RNG.randrange(65536)); w16(record,28,100+arc); w16(record,30,(arc+1)%n)
        w32(record,16,RNG.randrange(-10000,10000)); w32(record,20,RNG.randrange(-200,200))
        w32(record,56,previous-RNG.randrange(30)); w32(record,88,0); w64(record,40,identity)
        w16(record,36,188 if index%13==0 and arc==original_arc else 30+arc)
        if arc!=original_arc and arc%4==0: record[33] ^= 1
        if arc!=original_arc and arc%5==0: w64(record,40,identity^1)
        pool = bytearray(RNG.randbytes(sc*40))
        for k in range(sc):
            w32(pool,k*40,RNG.randrange(-1000,10000) if k<sc-1 or index%3 else IMP)
            w32(pool,k*40+4,RNG.randrange(-1000,1000))
            w64(pool,k*40+8,ident([10+(arc+k)%4]))
            w64(pool,k*40+16,ident([40+arc,k]))
        records.append(record); pools.append(pool)
        scripts.append(dict(d118=RNG.choice([-200,0,500,2147483647]),d140=RNG.choice([-5,0,17]),
                            a118=arc==original_arc or arc%3!=0,a140=arc==original_arc or arc%4!=1,
                            successors=[99,23,23] if arc%2==0 else [98,97]))
    if index%11==0: scripts[original_arc]["a118"] = False
    if index%17==0: scripts[original_arc]["a140"] = False
    primary = bytearray(RNG.randbytes((pc+1)*40)); secondary = bytearray(RNG.randbytes((sc+2)*40))
    for data in [primary,secondary]:
        for k in range(len(data)//40):
            w64(data,k*40+8,0); w64(data,k*40+16,0)
    residue = bytearray(RNG.randbytes(80)); request = bytearray(residue)
    cell_id = 0x1122334455667788
    symbols = [185,186,187,188]; permission = index%256
    cap_enabled = index%3==0; cap = index%12; threshold = index%30
    original = records[original_arc]; callbacks = []; mapping = []; lookups = []
    inp = dict(pc=pc,sc=sc,node=node,limit=limit,slot=slot.hex(),primary=primary.hex(),secondary=secondary.hex(),
               records=[x.hex() for x in records],pools=[x.hex() for x in pools],headers=headers,scripts=scripts,
               residue=residue.hex(),symbols=symbols,permission=permission,capEnabled=cap_enabled,cap=cap,
               threshold=threshold,profileOffset=[11571,188222][index%2],costCell=cell_id)

    def previous_fields(record):
        w32(request,4,u16(record,24)); w16(request,14,u32(record,24)>>22)
        w16(request,16,u16(record,28)); w16(request,38,u16(record,36))
        related = u16(record,30); lookups.append([previous,related])
        request[73] = records[related][7]&1
        w32(request,68,(headers[record[4]]>>9)&15)
        cat = categories[record[26]&63]; request[19] = cat[1]; w32(request,24,cat[0])
    def composed(prefix,suffix,mapped):
        path,calls = compose(paths[prefix], paths[suffix], mapped); mapping.extend(calls)
        return ident(path)
    def meta(target):
        w16(target,24,original_arc); target[26] = slot[32]
        target[31] = (target[31]&0xe0)|(slot[33]&31)
    if pc>1:
        first = primary[:40]; meta(first); first[27] &= 128
        w64(first,8,composed(0,suffix_id,True)); w64(first,16,composed(0,suffix_id,False))
        w32(first,0,i32(slot,16)-i32(original,16)); w32(first,4,i32(slot,20)-i32(original,20))
        first[30] = slot[38]; first[31] |= 32; primary[:40] = first
        for k in range(1,pc): w32(primary,k*40,IMP); w32(primary,k*40+4,IMP); primary[k*40+30] = 0
    first = secondary[:40]; meta(first); first[27]&=128
    w32(first,0,i32(slot,16)); w32(first,4,i32(slot,20)); first[30]=slot[38]; secondary[:40]=first
    w32(request,0,node); w32(request,8,previous); w16(request,12,u16(slot,28))
    request[18]=categories[slot[26]&63][1]; w32(request,20,categories[slot[26]&63][0])
    request[28]=(slot[39]>>1)&7; w32(request,32,span)
    if u16(slot,24)==node: w16(request,36,u16(slot,36)); request[72]=slot[7]&1
    else:
        symbol = suffix[-1]; request[72]=0
        if symbol==0xfffb: w16(request,36,symbols[1])
        elif symbol==0xfffc: w16(request,36,symbols[2])
        elif (symbol+9)&65535<3: w16(request,36,symbols[0])
    w64(request,40,cell_id); w32(request,52,IMP)
    request[56]=slot[33]&63; request[57]=0; request[58]=permission
    w32(request,64,(headers[slot[4]]>>9)&15); previous_fields(original)
    cell = i32(original,16)
    inp["expectedInitialRequest"]=request.hex(); inp["expectedInitialCost"]=cell
    def invoke(kind,arc):
        nonlocal cell
        callbacks.append(dict(kind=kind,request=request.hex(),cost=cell))
        script=scripts[arc]
        if kind==0: cell=wrap(cell+script["d118"]); request[60]^=0x11; return script["a118"]
        if kind==1: cell=wrap(cell+script["d140"]); request[61]=(request[61]+3)&255; return script["a140"]
        return script["successors"]
    parent_kind=headers[original[4]]&0x1e00
    bypass=u16(original,36)==symbols[3] or wrap(previous-i32(original,56))>=threshold and 0x200<=parent_kind<=0xa00
    accepted=(bypass or invoke(0,original_arc)) and invoke(1,original_arc)
    filtered=merged=0
    if accepted:
        adjusted=wrap(i32(slot,16)-cell)
        physical=wrap(i32(slot,16)-i32(original,16)); increment=wrap(i32(slot,20)-i32(original,20))
        cell=physical
        w64(secondary,8,u64(pools[original_arc],8))
        for k in range(1,sc):
            secondary[k*40+30]=0
            if i32(pools[original_arc],k*40)<IMP:
                w32(secondary,k*40,physical+i32(pools[original_arc],k*40)); w32(secondary,k*40+4,increment+i32(pools[original_arc],k*40+4))
                w16(secondary,k*40+24,original_arc); secondary[k*40+27]=(secondary[k*40+27]&128)|k
                w64(secondary,k*40+8,u64(pools[original_arc],k*40+8))
            else: w32(secondary,k*40,IMP); w32(secondary,k*40+4,IMP)
        work=bytearray(800)
        for arc in range(limit,n):
            candidate=records[arc]
            if arc==original_arc or (candidate[33]^original[33])&63 or u64(candidate,40)!=u64(original,40): continue
            if cap_enabled and candidate[38]+span>cap: continue
            w16(request,16,u16(candidate,28))
            if u16(slot,28) not in invoke(2,arc): continue
            filtered+=1; cell=adjusted; previous_fields(candidate)
            if not invoke(0,arc) or not invoke(1,arc): continue
            for k in range(sc):
                source=pools[arc]
                if i32(source,k*40)<IMP:
                    w32(work,k*40,cell+i32(source,k*40)); w32(work,k*40+4,increment+i32(source,k*40+4))
                    work[k*40+30]=0; w16(work,k*40+24,arc); work[k*40+27]=(work[k*40+27]&128)|k
                    w64(work,k*40+8,u64(source,k*40+8))
                else: w32(work,k*40,IMP)
            secondary=bytearray(merge(secondary,work,sc)[0])
            w32(slot,16,i32(secondary,0)); w32(slot,20,i32(secondary,4)); slot[38]=secondary[30]; w16(slot,30,u16(secondary,24))
            if pc>1: primary[27]=(primary[27]&128)|(secondary[27]&127)
            merged+=1
        w32(slot,16,i32(secondary,0)); w32(slot,20,i32(secondary,4)); slot[38]=secondary[30]; w16(slot,30,u16(secondary,24))
        if pc>1: primary[27]=(primary[27]&128)|(secondary[27]&127)
        for k in range(sc):
            if i32(secondary,k*40)>=IMP: continue
            arc=u16(secondary,k*40+24); choice=secondary[k*40+27]&127
            lookups.append([previous,arc]); source=pools[arc]
            w64(secondary,k*40+8,composed(u64(source,choice*40+8),suffix_id,True))
            w64(secondary,k*40+16,composed(u64(source,choice*40+16),suffix_id,False))
            secondary[k*40+31]=(secondary[k*40+31]&0xe0)|(slot[33]&31); secondary[k*40+26]=slot[32]
    inp.update(paths=paths,expectedSlot=slot.hex(),expectedPrimary=primary.hex(),expectedSecondary=secondary.hex(),
               expectedRequest=request.hex(),callbacks=callbacks,mapping=mapping,lookups=lookups,
               accepted=accepted,filtered=filtered,merged=merged)
    return inp

cases=[generate(index) for index in range(512)]
assert sum(t["merged"] for t in cases)>50
Path(sys.argv[1]).write_text(json.dumps(dict(cases=cases),separators=(",",":")),encoding="utf-8")
print(f"generated {len(cases)} full controller streams / {sum(t['merged'] for t in cases)} accepted incoming merges / actual35 category tables")
