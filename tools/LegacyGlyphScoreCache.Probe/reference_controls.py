"""Independent pinned ordinary21d scalar callback oracle; no library imports."""
import json, random, struct, sys
from pathlib import Path

RNG = random.Random(0x1c1300)
IMP = 0x4000000
def f(value): return struct.unpack('<f', struct.pack('<f', value))[0]
def bits(value): return struct.unpack('<i', struct.pack('<f', value))[0]
def wrap(value): return (value + 0x80000000) % 0x100000000 - 0x80000000
cases = []
for i in range(1024):
    n = i % 151
    rows = bytearray(RNG.randbytes(n * 24))
    labels = [(j * 7) % 31 for j in range(n)]
    costs = [RNG.choice([-2147483648, -100, 0, 30, IMP, IMP+1, 2147483647]) for _ in labels]
    vectors = [[bits(f(RNG.uniform(-2, 2))) for _ in range(3)] for _ in labels]
    if n:
        # Raw NaN/sign/payload and field0 independence are deliberate.
        vectors[0][0] = RNG.choice([0x3f400000, -2147483648, 0x7fc00123])
        for j, (label, cost) in enumerate(zip(labels, costs)):
            struct.pack_into('<iH', rows, j*24+4, cost, label)
    symbol = labels[i % n] if n and i % 3 else 250
    channel = i % 3
    expected_score = next((v[channel] for label,v in zip(labels,vectors) if label==symbol), 0)
    bypass = [200,201,202,203,204,205,206,207]
    blank = 199
    blank_cost = RNG.choice([-2147483648, 0, 19, IMP, IMP+1])
    geometry = [RNG.choice([-2147483648, -3, 0, 4, 2147483647]) for _ in range(8)]
    scale = [1., .25, 32768., 65536., 1e20][i % 5]
    ordinal = [65535, i % max(n,1), 20, 150][i % 4]
    requested_symbol = [symbol, blank, bypass[i%8], 249][(i//4)%4]
    top20 = (i//16) % 2
    fallback = (i//32) % 2
    geom = i%8
    request = bytearray(RNG.randbytes(16)); struct.pack_into('<HH',request,6,ordinal,requested_symbol)
    request[10:13] = bytes([geom,top20,fallback])
    if fallback:
        value = f(f(scale)*40000.)
        cost = -2147483648 if value >= 2147483648 or value < -2147483648 else int(value)
    elif ordinal==65535 and requested_symbol==blank:
        cost = min(blank_cost, IMP)
    else:
        index = ordinal if ordinal!=65535 else next((j for j,label in enumerate(labels) if label==requested_symbol),n)
        cost = costs[index] if index<n else IMP
        if top20 and index>19: cost=IMP
    if cost<IMP and requested_symbol not in bypass: cost=wrap(cost+geometry[geom])
    cases.append(dict(rows=rows.hex(),vectors=vectors,geometry=geometry,bypass=bypass,blank=blank,
        blankCost=blank_cost,scale=scale,symbol=symbol,channel=channel,request=request.hex(),
        expectedScore=expected_score,expectedFirst=vectors[0][0] if n else 0,expectedCost=cost))
Path(sys.argv[1]).write_text(json.dumps(dict(cases=cases)),encoding='utf-8')
print('generated1024 independent native glyph score/cache controls')
