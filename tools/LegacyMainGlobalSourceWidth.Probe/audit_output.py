"""Independent2188F0 width accumulator and actual protocol-packet binding.

Inputs are recorded raw rows/compact bytes and live source bounds, not expected
text. This audits a consumer of the current partial controller, not its winner.
"""
import ast, base64, copy, hashlib, json, struct, sys
from pathlib import Path

if len(sys.argv) != 2:
    raise SystemExit('capture.json')
u16 = lambda b, o: struct.unpack_from('<H', b, o)[0]
i32 = lambda b, o: struct.unpack_from('<i', b, o)[0]
u32 = lambda b, o: struct.unpack_from('<I', b, o)[0]
f32 = lambda x: struct.unpack('<f', struct.pack('<f', x))[0]
fbits = lambda x: struct.unpack('<I', struct.pack('<f', x))[0]
dbits = lambda x: struct.unpack('<Q', struct.pack('<d', x))[0]
decode = base64.b64decode

# Parse only authenticated raw v103 AL input; no captured labels become expected
# labels. The independently authored parser predates this width consumer.
parser = Path(__file__).resolve().parents[1] / 'LegacyFst28CPrimaryMap.Probe/audit_output.py'
tree = ast.parse(parser.read_text(encoding='utf8')); namespace = {'struct': struct}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name == 'alphabet'],type_ignores=[]),'raw-alphabet-v103','exec'),namespace)
def expected_compact_rows(c, compact):
    raw_al = decode(c['acousticAlphabetPayload'])
    assert len(raw_al) == 5134 and hashlib.sha256(raw_al).hexdigest() == '2997884bbaaa094929249d1af7e88f285f16ee0e5345ab9013455b66a18eb6a8'
    label_count,end,tokens,_ = namespace['alphabet'](raw_al,0)
    assert label_count == 192 and end == len(raw_al)
    labels = {token: label for label,token in tokens.items()}
    marker = labels.get(b'\x03',65535); joiner = labels.get('\u200d'.encode(),65535)
    unit = u16(raw_al,6); soft_hyphen = labels.get('\u00ad'.encode(),65535); apostrophe = labels.get(b"'",65535)
    starts = c['nodeStarts2478']; record_count = c['recordCount']; selected = c['selectedRecordIndex']
    assert len(starts) >= 2 and starts[0] == 0 and starts[-1] == record_count and all(0 <= a <= b <= record_count for a,b in zip(starts,starts[1:]))
    assert starts[-2] <= selected < starts[-1]
    def row(index):
        assert 0 <= index < record_count, 'compact index range'
        return compact[index*40:(index+1)*40]
    def reference(r):
        value = u32(r,8) & 0xffffff
        node = value if value < 0x800000 else value - 0x1000000
        return node,u16(r,11)
    index = selected; node = len(starts)-2; arc = reference(row(index))[1]; alternative = u16(row(index),30)
    seen = set(); expected = []; regular = 65535
    while node != -1:
        state = (index,node,arc,alternative)
        assert state not in seen and len(seen) < record_count, 'compact cycle'
        seen.add(state); r = row(index); symbol = u16(r,28); subtype = r[32] & 7
        if symbol not in (marker,joiner,65535,65533,unit):
            assert subtype in (0,3), 'unsupported source-width producer'
            if subtype < 3: regular = symbol
            normalized = soft_hyphen if symbol == 0xfff4 else 0xfff9 if symbol in (0xfff5,0xfff7,0xfff8) else symbol
            if normalized == label_count+1: normalized = apostrophe
            expected.append((index,node,arc,subtype,normalized,regular,r[33]))
        previous = i32(r,16)
        if previous == -1: break
        predecessor = row(previous); predecessor_node,_ = reference(predecessor)
        if u16(predecessor,30) != 65535 or (symbol == marker and predecessor_node < node):
            if alternative < 65534:
                assert 0 <= predecessor_node < len(starts)-1
                previous = starts[predecessor_node] + alternative; predecessor = row(previous)
            alternative = u16(predecessor,30)
        index = previous; node,arc = reference(predecessor)
    return expected

def audit(data):
    assert len(data) == 8
    ordinary = protocol = contributions = 0
    different_means = set()
    for owner, c in enumerate(data):
        assert c['owner'] == owner and c['topAC'] == 2 and c['metadataEvents'] == 0
        compact = decode(c['compact'])
        assert c['recordCount'] <= len(compact) // 40
        expected_rows = expected_compact_rows(c, compact)
        actual_rows = [decode(flush['rows72'])[i:i+72] for flush in c['flushes'] for i in range(0,len(decode(flush['rows72'])),72)]
        assert len(actual_rows) == len(expected_rows), 'compact row membership'
        for actual, expected_row in zip(actual_rows, expected_rows):
            index,node,arc,subtype,label,regular,score = expected_row
            assert (i32(actual,8),i32(actual,0),u16(actual,4),actual[60],u16(actual,58),u16(actual,56),actual[62]) == (index,node,arc,subtype,label,regular,score), 'compact row chronology/normalization'
        expected_ordinary = sum(row[3] == 0 for row in expected_rows)
        expected_protocol = sum(row[3] == 3 for row in expected_rows)
        assert expected_ordinary + expected_protocol == len(expected_rows)
        owner_ordinary = owner_protocol = 0
        count = 0
        value = 0.0
        assert len(c['flushes']) == 2
        for flush in c['flushes']:
            rows = decode(flush['rows72'])
            assert len(rows) % 72 == 0 and len(flush['bounds']) == len(rows) // 72
            assert [p['ordinal'] for p in flush['producers']] == list(reversed(range(len(rows) // 72)))
            for p in flush['producers']:
                ordinal = p['ordinal']
                row = rows[ordinal * 72:(ordinal + 1) * 72]
                index = i32(row, 8)
                assert index >= 0
                if index < c['recordCount']:
                    subtype = compact[index * 40 + 32] & 7
                    excluded = subtype == 2 or (subtype not in (4, 6) and u16(row, 56) != 65535 and ((u16(row, 58) + 6) & 65535) <= 2)
                    if not excluded:
                        lo, hi = map(f32, flush['bounds'][ordinal])
                        value += f32(hi - lo)
                        count = ((count + 1 + 2**31) % 2**32) - 2**31
                        contributions += 1
                assert p['subtype'] == row[60] and p['label'] == u16(row, 58)
                if row[60] == 0:
                    assert p['remaining'] == 255 and p['emitted'] == u16(row, 58)
                    assert p['score0C'] == fbits(f32(row[62] / 255.0))
                    assert p['request64'] == ''
                    ordinary += 1
                    owner_ordinary += 1
                else:
                    assert row[60] == 3 and u16(row, 58) == 65530
                    assert p['remaining'] == 256 and p['emitted'] == 65535
                    assert p['score0C'] == p['score10'] == fbits(1.0) and p['cost14'] == 0
                    request = decode(p['request64'])
                    assert len(request) == 64
                    assert u32(request, 44) == c['topAC'] and u32(request, 48) == c['topB0Bits']
                    protocol += 1
                    owner_protocol += 1
        assert (owner_ordinary,owner_protocol) == (expected_ordinary,expected_protocol), 'compact producer accounting'
        assert c['count'] == count and dbits(c['sum']) == dbits(value)
        expected = fbits(value / count if count > 0 else 0)
        assert c['topB0Bits'] == expected
        different_means.add(expected)
    assert len(different_means) >= 5
    return ordinary,protocol,contributions

data = json.loads(Path(sys.argv[1]).read_bytes())
ordinary,protocol,contributions = audit(data)
# Corrupt complete producer groups, not only counters. The first three are
# rejected by independently reconstructed membership/chronology before scalar
# checks, even when row/producers/bounds are changed together.
negative = []
def fails(name,changed,reason=None):
    try: audit(changed)
    except AssertionError as error:
        if reason is not None: assert reason in str(error), (name,str(error))
        negative.append(name)
    else: raise AssertionError(('negative capture accepted',name))
c = copy.deepcopy(data)
for owner in c:
    candidates = [f for f in owner['flushes'] if len(decode(f['rows72'])) >= 144]
    if candidates:
        f = candidates[0]; rows = decode(f['rows72']); f['rows72'] = base64.b64encode(rows[72:]).decode();f['bounds'].pop(0)
        for p in f['producers']:p['ordinal'] -= 1
        f['producers'] = [p for p in f['producers'] if p['ordinal'] >= 0]
        break
fails('delete-row-and-producer-and-bounds',c,'compact row membership')
c = copy.deepcopy(data)
for owner in c:
    candidates = [f for f in owner['flushes'] if len(decode(f['rows72'])) >= 144]
    if candidates:
        f = candidates[0]; rows=decode(f['rows72']);f['rows72']=base64.b64encode(rows[72:144]+rows[:72]+rows[144:]).decode();f['bounds'][0],f['bounds'][1]=f['bounds'][1],f['bounds'][0]
        for p in f['producers']:
            if p['ordinal'] in(0,1):p['ordinal']=1-p['ordinal']
        f['producers'].sort(key=lambda p:-p['ordinal']);break
fails('swap-row-producer-bounds',c,'compact row chronology')
c = copy.deepcopy(data);r=bytearray(decode(c[0]['flushes'][0]['rows72']));r[62]^=1;c[0]['flushes'][0]['rows72']=base64.b64encode(r).decode()
fails('wrong-compact-score',c,'compact row chronology')
c=copy.deepcopy(data);c[0]['sum']+=1;fails('wrong-width-sum',c)
c=copy.deepcopy(data)
for owner in c:
    found=False
    for flush in owner['flushes']:
        for p in flush['producers']:
            if p['subtype']==3:
                request=bytearray(decode(p['request64']));request[48]^=1;p['request64']=base64.b64encode(request).decode();found=True;break
        if found:break
    if found:break
fails('wrong-protocol-bound-width',c)
c=copy.deepcopy(data);c[0]['selectedRecordIndex']=c[0]['recordCount'];fails('foreign-selected-compact-range',c)
assert len(negative)==6
print(f'PASS independent8 global width consumers/{contributions} exact float-width/double contributions/{ordinary} raw-compact-derived ordinary and{protocol} real FFFA packet bindings/6negativecaptures; partial controller, no WORD/publication claim')
