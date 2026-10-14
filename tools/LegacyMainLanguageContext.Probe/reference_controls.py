"""Original full80 ordinary203690 and full16 collector interpreter.

Transition hooks are explicitly scripted inputs. Actual geometry/resource
integration is a separate probe, not a guessed callback in this oracle.
"""
import ast
import json
import random
import struct
import sys
from pathlib import Path

if len(sys.argv) != 2 or Path(sys.argv[1]).suffix.lower() != '.json':
    raise SystemExit('Usage: reference_controls.py controls-output.json (one output argument only)')

ROOT = Path(__file__).resolve().parents[2]
tree = ast.parse((ROOT / 'tools/LegacyWordHigherArc.Probe/reference_controls.py').read_text(encoding='utf-8-sig'))
selected = [node for node in tree.body if isinstance(node, ast.FunctionDef) and node.name == 'profile']
scope = {'ROOT': ROOT, 'struct': struct, 'u16': lambda data, at: struct.unpack_from('<H', data, at)[0]}
exec(compile(ast.Module(body=selected, type_ignores=[]), 'original-profile-reader', 'exec'), scope)
categories = scope['profile'](188222)
rng = random.Random(0x203690)
IMP = 0x4000000
def i32(data, at): return struct.unpack_from('<i', data, at)[0]
def u16(data, at): return struct.unpack_from('<H', data, at)[0]
def u64(data, at): return struct.unpack_from('<Q', data, at)[0]
def wrap(value): return (value + 2**31) % 2**32 - 2**31
def w16(data, at, value): struct.pack_into('<H', data, at, value & 65535)
def w32(data, at, value): struct.pack_into('<I', data, at, value & 0xffffffff)
def w64(data, at, value): struct.pack_into('<Q', data, at, value)

def insert(heap, capacity, cost, inc, previous, delta, current):
    if capacity == 0 or cost > IMP - 1: return False
    record = struct.pack('<iiiHH', cost, inc, delta, previous, current)
    if len(heap) == capacity:
        if i32(heap[0], 0) <= cost: return False
        hole = 0
        while len(heap) != 1:
            child = hole * 2 + 1
            if child < len(heap)-1 and i32(heap[child], 0) <= i32(heap[child+1], 0): child += 1
            if i32(heap[child], 0) <= cost: break
            heap[hole] = heap[child]
            hole = child
            if hole * 2 >= len(heap)-1: break
    else:
        hole = len(heap)
        heap.append(bytes(16))
        while hole:
            parent = (hole-1)//2
            if cost <= i32(heap[parent], 0): break
            heap[hole] = heap[parent]
            hole = parent
    heap[hole] = record
    return True

def make_case(index):
    anchor_end = [9,255,40][index%3]
    span = index%4
    prev_node = anchor_end - span - 1
    headers = [rng.getrandbits(24) for _ in range(9)]
    headers[1] = (headers[1] & ~0x1e00) | (0xc00 if index%3 else 0x600)
    rows = []
    state = rng.getrandbits(64)
    original = 2 + index%7
    for ordinal in range(9):
        row = bytearray(rng.randbytes(88))
        row[4] = ordinal
        row[7] = rng.randrange(256)
        w16(row,24,rng.randrange(65536)); w16(row,26,rng.randrange(35))
        w16(row,28,100+ordinal); w16(row,30,(ordinal+1)%9)
        w16(row,36,20+ordinal)
        row[33] = rng.randrange(64) | (128 if ordinal%5 == 0 and ordinal != original else 0)
        row[38] = rng.randrange(10)
        row[39] = rng.randrange(256)
        w32(row,16,rng.choice([rng.randrange(-30000,30000), -2**31, 2**31-1,IMP,IMP+1]))
        w32(row,20,rng.choice([rng.randrange(-2000,2000), -2**31,2**31-1]))
        w64(row,40,state if ordinal%4 or ordinal == original else state^1)
        rows.append(row)
    anchor = bytearray(rng.randbytes(88))
    anchor[4] = 1; anchor[32] = span; anchor[33] = rng.randrange(256)
    anchor[39] = rng.randrange(256) & ~14 | ((index%2)*2 << 1)
    w16(anchor,24,anchor_end); w16(anchor,26,rng.randrange(35)); w16(anchor,28,27)
    w16(anchor,30,original); w16(anchor,36,70); anchor[7]=rng.randrange(256)
    suffix = None
    if index%4:
        suffix = [0xfffb, 0xfffc, 0xfff7, 42][(index//4)%4]
        w16(anchor,24,anchor_end+1)
        w64(anchor,8,1)  #Probe binds this symbolic node to its owned context.
    current = bytearray(rng.randbytes(88))
    w32(current,56,anchor_end); w16(current,60,2)
    w32(current,16,rng.choice([50000, -2**31, 2**31-1, -15000]))
    w32(current,20,rng.choice([4000, -2**31, 2**31-1]))
    residue = bytearray(rng.randbytes(80))
    cap = index%9
    policy = dict(count=index%2==0, limit=7, flag=index%5==0, reuse=index%3!=0,
                  permission=rng.randrange(256), primary=2, end=9)
    scripts = [dict(d118=rng.choice([-30,0,170,2**31-1]), d140=rng.choice([-9,0,45]),
                    dinc=rng.choice([-11,0,13]), a118=(index+k)%11!=0, a140=(index+k)%13!=0,
                    flag=(index+k)%256, successors=(k+index)%3!=0) for k in range(9)]
    traces = []
    request = bytearray(residue)
    def update(row):
        w32(request,4,u16(row,24)); w16(request,14,(struct.unpack_from('<I',row,24)[0]>>22))
        w16(request,16,u16(row,28)); w16(request,38,u16(row,36))
        request[73]=rows[u16(row,30)][7]&1
        w32(request,68,headers[row[4]]>>9&15)
        first,geom=categories[u16(row,26)&63]
        request[19]=geom; w32(request,24,first)
    w32(request,0,anchor_end); w32(request,8,prev_node); w16(request,12,27)
    first,geom=categories[u16(anchor,26)&63]
    request[18]=geom; w32(request,20,first); request[28]=anchor[39]>>1&7
    w32(request,32,span)
    if suffix is None:
        w16(request,36,70); request[72]=anchor[7]&1
    else:
        request[72]=0
        if suffix == 0xfffb: w16(request,36,2)
        elif suffix == 0xfffc: w16(request,36,3)
        elif (suffix+9)&65535 < 3: w16(request,36,1)
    w64(request,40,1001); w32(request,52,IMP); request[56]=anchor[33]&63
    request[57]=request[59]=0; request[58]=policy['permission']; w16(request,60,0)
    w32(request,64,headers[1]>>9&15)
    update(rows[original]); w32(request,48,i32(rows[original],20))
    cell=i32(rows[original],16)
    calls=0
    def geometry(ordinal):
        nonlocal cell,calls
        script=scripts[ordinal]
        traces.append(dict(kind=118, bytes=request.hex(), cell=cell)); calls+=1
        cell=wrap(cell+script['d118']); request[59]=script['flag']
        if not script['a118']: return False
        traces.append(dict(kind=140, bytes=request.hex(), cell=cell)); calls+=1
        cell=wrap(cell+script['d140']); w32(request,48,wrap(i32(request,48)+script['dinc']))
        return script['a140']
    initial=geometry(original)
    cost_delta=wrap(i32(current,16)-cell) if initial else IMP
    inc_delta=wrap(i32(current,20)-i32(request,48)) if initial else IMP
    geom_delta=wrap(cell-i32(rows[original],16)) if initial else 0
    heap=[]; matched=0
    for ordinal in range(2,9):
        row=rows[ordinal]
        if row[33]&128 or u64(row,40)!=u64(rows[original],40) or policy['count'] and row[38]+span>7 or policy['flag'] and not row[39]&1: continue
        w16(request,16,u16(row,28))
        traces.append(dict(kind=0, bytes=request.hex(), cell=cell))
        if not scripts[ordinal]['successors']: continue
        matched+=1
        raw=i32(row,16); inc=i32(row,20)
        if policy['reuse'] and struct.unpack_from('<I',request,64)[0]==6:
            total=wrap(cost_delta+raw)
            if raw<IMP and total<IMP: insert(heap,cap,total,wrap(inc_delta+inc),ordinal,geom_delta,33)
        else:
            update(row); w64(request,40,1002); cell=raw; w32(request,48,inc)
            accepted=geometry(ordinal); total=wrap(cost_delta+cell)
            if accepted and cell<IMP and total<IMP:
                insert(heap,cap,total,wrap(inc_delta+i32(request,48)),ordinal,wrap(cell-raw-geom_delta),65535)
    return dict(current=current.hex(), anchor=anchor.hex(), suffix=suffix, rows=[r.hex() for r in rows], headers=headers,
                scripts=scripts, residue=residue.hex(), policy=policy, cap=cap,
                initial=initial, output=b''.join(heap).hex(), last=request.hex(), matched=matched, calls=calls, traces=traces)

cases=[make_case(i) for i in range(512)]
streams=[]
for index in range(1024):
    cap=index%17; heap=[]; controls=[]
    for ordinal in range(30):
        cost=rng.choice([0,7,7,-7,-2**31,2**31-1,IMP-1,IMP, rng.randrange(-100,100)])
        inc=rng.randrange(-2**31,2**31); delta=rng.randrange(-2**31,2**31)
        prev=rng.randrange(65536); cur=rng.randrange(65536)
        accepted=insert(heap,cap,cost,inc,prev,delta,cur)
        controls.append(dict(cost=cost, inc=inc, delta=delta, previous=prev, current=cur, accepted=accepted, output=b''.join(heap).hex()))
    streams.append(dict(cap=cap, steps=controls))
output=Path(sys.argv[1]); output.parent.mkdir(parents=True,exist_ok=True)
output.write_text(json.dumps(dict(cases=cases,streams=streams),separators=(',',':')))
print('Generated512 full80 context cases and1024 independent16-byte heap streams.')
