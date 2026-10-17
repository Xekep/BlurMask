"""Independent final2410 selector, raw WORD streams and whole273 allocation.

No expected transcription, NN label alias or vendor execution is used.
"""
import base64, importlib.util, json, math, struct, sys
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('actual_alphabet', ROOT/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
alphabet = module.read_alphabet(Path(sys.argv[1]).read_bytes())
data = json.loads(Path(sys.argv[2]).read_text())
b64 = base64.b64decode
u16 = lambda b,o: struct.unpack_from('<H',b,o)[0]
i32 = lambda b,o: struct.unpack_from('<i',b,o)[0]
u64 = lambda b,o: struct.unpack_from('<Q',b,o)[0]
f32 = lambda x: struct.unpack('<f',struct.pack('<f',x))[0]
rf = lambda b,o: struct.unpack_from('<f',b,o)[0]
wrap = lambda n: (n+2**31)%2**32-2**31
imp = 0x4000000

def reset(b):
    b = bytearray(b)
    for off,value,fmt in [(8,2**64-1,'Q'),(16,0x3f800000,'I'),(20,0x3f800000,'I'),(24,0,'I'),(28,0,'I'),
        (40,0,'I'),(48,65534,'I'),(53,0,'H'),(55,0,'B'),(60,2**32-1,'I'),(84,2**32-1,'I'),(96,0,'I'),(112,0,'I')]:
        struct.pack_into('<'+fmt,b,off,value)
    return b

def compare(a,b,pool):
    #All emitted actual words in this capture are ordinary scalar glyphs.
    #Retain stream order and publication bit6, not decoded-string equality.
    aa = [(u16(pool[r],8),pool[r][6]&64) for w in a for r in w['references']]
    bb = [(u16(pool[r],8),pool[r][6]&64) for w in b for r in w['references']]
    assert aa and bb
    return aa == bb

total_bytes = total_words = total_packets = 0
for run in data['runs']:
    records, matrix = b64(run['records']), b64(run['secondary'])
    contexts = {i:(parent,symbol) for i,parent,symbol in run['contexts']}
    excluded, expected_choices = [], []
    output_index = 0
    pool = {}
    for node in run['pool']:
        raw = b64(node['records']); assert len(raw)%40 == 0
        for i in range(len(raw)//40): pool[node['key']|(i<<40)] = raw[i*40:(i+1)*40]
    active = []
    seed = run['seed']
    word = bytearray((i*37+seed)&255 for i in range(120))
    for at,n in [(32,8),(88,12),(104,12)]: word[at:at+n] = bytes(n)
    word = reset(word)
    for attempt in run['attempts']:
        assert attempt['outputIndex'] == output_index
        best, chosen = imp, None
        for ordinal in range(run['sourceEnd']):
            row = records[ordinal*88:(ordinal+1)*88]
            if i32(row,16) >= imp: continue
            inspect = ordinal < run['primary'] or bool(row[5]&4)
            context = u64(row,8)
            if inspect and context:
                parent,symbol = contexts[context]
                if not parent and (symbol==65529 or ((symbol+5)&65535)<=1): inspect = False
            for k in range(2):
                alt = matrix[(ordinal*2+k)*40:(ordinal*2+k+1)*40]
                cost = i32(alt,0)
                if cost >= imp: break
                if inspect: cost = imp  #Genuine source policy Filter4=false.
                identity = u64(alt,8)
                if cost < best and identity not in excluded:
                    best = cost; chosen = (ordinal,k,identity,wrap(cost+run['cumulative']),i32(alt,4))
        if chosen is None:
            assert 'identity' not in attempt; output_index += 1; continue
        ordinal,k,identity,total,row14 = chosen
        excluded.append(identity)
        assert (attempt['ordinal'],attempt['secondaryOrdinal'],attempt['identity'],attempt['total'],attempt['row14']) == chosen
        assert attempt['exclusionCount'] == len(excluded)
        assert b64(attempt['initial']) == word
        for packet64 in attempt['packets']:
            packet = b64(packet64)
            for a,b,n in [(0,16,4),(4,24,4),(8,28,4),(12,48,2),(14,50,2),(16,53,1),(17,54,1),
                (20,60,4),(28,68,4),(32,72,4),(36,76,4),(40,80,4),(44,55,1)]:
                word[b:b+n] = packet[a:a+n]
            word = reset(word); total_packets += 1
        words = attempt['words']
        assert words
        raw_texts = []
        for w in words:
            raw = b64(w['storage']); refs = w['references']; assert u64(raw,8)==refs[-1]
            text = b''
            for reference in refs:
                glyph = pool[reference]; assert u16(glyph,4)&7 in (0,2)
                text += alphabet[u16(glyph,8)].encode('utf8')
            raw_texts.append(text); total_words += 1
        assert raw_texts == [b64(s) for s in attempt['rawUtf8']]
        duplicate = next((i for i,a in enumerate(active) if compare(a['words'],words,pool)), -1)
        assert attempt['duplicate'] == (duplicate>=0) and attempt['duplicateOf'] == duplicate
        assert attempt['released'] == (duplicate>=0)
        if duplicate < 0: active.append(attempt); output_index += 1
    assert output_index == run['outputIndex'] == 2 and excluded == run['excluded']
    assert len(active) == 2 and run['reference'] == active[0]['total']
    #One400 allocation, ctor only on the two occupied rows. Pointer values
    #are zero placeholders; actual WORD/selected40 owners are typed sidecars.
    expected = bytearray((i*23+seed)&255 for i in range(400))
    raw_masses, row_costs = [], []
    for i,a in enumerate(active):
        start = i*40; expected[start:start+40] = bytes(40)
        norm = f32(f32(wrap(a['total']-run['reference']))/f32(wrap(run['sourceCount']<<13)))
        row_cost = f32(f32(a['row14'])*f32(1/8192))
        exponent = f32(-f32(f32(5)*norm))
        mass = 0.0 if exponent < -75 else f32(3.73e32) if exponent > 75 else f32(math.exp(exponent))
        struct.pack_into('<fff',expected,start+8,mass,0.0,norm)
        struct.pack_into('<f',expected,start+20,row_cost)
        raw_masses.append(mass); row_costs.append(row_cost)
    denominator = f32(raw_masses[0]+raw_masses[1])
    for i in range(2):
        struct.pack_into('<f',expected,i*40+8,f32(raw_masses[i]/denominator))
        struct.pack_into('<f',expected,i*40+12,f32(math.exp(-f32(row_costs[i]-min(row_costs)))))
    actual = b64(run['storage'])
    assert actual == expected, [(i,a,b) for i,(a,b) in enumerate(zip(actual,expected)) if a!=b][:10]
    assert run['top80bits'] == 0xbf800000
    total_bytes += len(expected)
assert len(data['runs'])==4 and data['guards']==27
print(f'PASS independent final2410 selectors/exclusions, eight real WORD streams/raw AK UTF8, {total_packets} seed-reset packets and {total_bytes} complete273 allocation bytes; no accepted text.')
