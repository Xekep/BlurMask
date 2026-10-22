"""Independent configured span export, insertion packing, raw AK emission and
sameTOP full-pool overlap audit. Graph/cut metadata are captured stage inputs;
no expected handwriting labels or mass acceptance threshold is read.
"""
import base64
import copy
import hashlib
import importlib.util
import json
import struct
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('raw_alphabet_oracle',
    root / 'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)
if len(sys.argv)!=3:raise SystemExit('AK CAPTURE')
ak = Path(sys.argv[1]).read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
alphabet = module.read_alphabet(ak)
label_count = struct.unpack_from('<H', ak, 113)[0]
synthetic = {65532: '\u00a0', 65531: ' ', 65529: '\n', 65526: '\U000f0000'}
synthetic.update({65523-i: chr(0xf0010+i) for i in range(9)})


def emit(flags, symbol):
    kind = flags & 7
    if kind == 4:
        return None
    count = (symbol >> 2) & 8191 if kind in (1, 3) else 1
    if not count:
        return b''
    if kind == 3:
        return None if symbol & 3 == 1 else '\ufffd'.encode()*count
    if kind == 1:
        symbol = ((symbol & 3)-7) & 65535
    elif kind not in (0, 2):
        symbol = 65535
    if symbol < label_count and symbol not in alphabet:
        return None
    text = alphabet.get(symbol, synthetic.get(symbol, '\ufffd'))
    assert not ('\U000f0000' in text and '\U000f0001' in text)
    return text.encode()*count


def audit(d):
    assert [source['input']for source in d['inputs']]==[0,1,2]
    actual = prepared = refused = emitted = projected = overlap_bytes = 0
    modes = set()
    branches = set()
    for source in d['inputs']:
        counts = source['rawCounts']
        #Analytic input0 has one adjacent duplicate, removed by native167210.
        #All other supplied points are adjacent-distinct. Original raw point IDs
        #survive this filter; they are not compacted to five new raw ordinals.
        keys = ([[0, 0, i] for i in (0, 2, 3, 4, 5)] if source['input'] == 0 else
                [[0, s, p] for s, n in enumerate(counts) for p in range(n)])
        ending_keys = [k[:] for k in keys]
        if source['input'] == 0:
            ending_keys[0] = [0, 0, 1]  #The duplicate's ending metadata belongs to raw point1.
        assert source['keys'] == keys and source['endingKeys'] == ending_keys
        point = []
        offset = 0
        for n in source['counts']:
            physical = []
            for p in range(n):
                tick = source['map'][offset+p] if source['map'] else offset+p
                q, r = divmod(tick, source['factor'])
                physical.append(q+(r != 0))
            point.append(physical)
            offset += n
        assert not source['map'] or len(source['map']) == offset
        graph, groups = source['graph'], source['groupRanges']

        def lookup(context, start):
            if context < 0 or context >> 7 >= len(graph):
                return None
            return next((group for group, span, a, z in graph[context >> 7]
                         if span == 0 and (a if start else z) == context), None)

        def span(key):
            last = key & 0xffffff
            if last & 0x800000:
                last -= 0x1000000
            delta = struct.unpack('<h', struct.pack('<H', key >> 24 & 65535))[0]
            first = last+delta
            mode = key >> 56 & 1
            assert delta < 0 and last >= 0 and first >= -1
            current = lookup(last, True)
            def begin(group):
                s, p, _ = groups[group][0]
                return point[s][p]
            if current is None and mode:
                previous = lookup(first, True)
                if previous is None:
                    return None
                end = begin(previous)
                result = [(keys[end], keys[end])]
                branches.add('mode1-no-start')
            else:
                if current is not None:
                    end = begin(current)
                    end_key = keys[end]
                    branches.add('matched-start')
                else:
                    current = lookup(last, False)
                    if current is None:
                        return None
                    s = groups[current][-1][0]
                    end = point[s][-1]
                    end_key = ending_keys[end]
                    branches.add('mode0-physical-end')
                if first == -1:
                    result = [(end_key, end_key)]
                    branches.add('before-source')
                else:
                    previous = lookup(first, False)
                    if previous is None:
                        return None
                    s, _, stop = groups[previous][-1]
                    start = point[s][stop-1]
                    adjacent = start+1 < len(keys) and keys[start+1] == end_key
                    if keys[start] == end_key or adjacent:
                        result = [(ending_keys[start], end_key)]
                        branches.add('coalesced')
                    else:
                        result = [(ending_keys[start], ending_keys[start]), (end_key, end_key)]
                        branches.add('two-degenerate')
            modes.add(mode)
            return result

        def project(refs):
            if len(refs) == 1:
                first, last = refs[0]
                coordinates = [(first[1], counts[first[1]]-1)]
                if first[:2] != last[:2]:
                    coordinates.append((last[1], 0))
            else:
                #These selected objects cover every caller point and no residual
                #output was requested. Relative integer coordinates therefore map
                #directly to insertion mode4 with terminal fraction zero.
                coordinates = [(a[1], a[2]) for a, _ in refs]
            return [[(obj << 32) | (point << 16) | 4]*2 for obj, point in coordinates]

        contexts=sorted({c for arcs in graph for _,span,a,z in arcs if span==0 for c in(a,z)})
        expected_keys=[(last&0xffffff)|(((first-last)&0xffff)<<24)|(mode<<56)
                       for last in contexts for first in [-1]+contexts
                       if first<last and last-first<=32768 for mode in range(2)]
        pool=source['initialPool'];assert len({n['key']for n in pool})==len(pool)
        expected_emission=[]
        for node in pool:
            data=base64.b64decode(node['rows']);assert len(data)==((node['packed']&0x7fff)>>2)*40
            expected_emission.extend((node['key']|(i<<40),data[40*i:40*(i+1)])for i in range(len(data)//40))
        expected_actual=[n['key']for n in pool if ((n['key']>>24)&0xffff)>=0x8000]
        assert [c['alternative']for c in source['candidates']]==[0,1]
        for candidate in source['candidates']:
            assert [(r['reference'],base64.b64decode(r['row40']))for r in candidate['emission']]==expected_emission,'complete ordered initial-pool emission membership'
            assert [r['key']for r in candidate['actual']]==expected_actual,'complete inferred span membership'
            assert [r['key']for r in candidate['prepared']]==expected_keys,'complete ordered context-pair-mode grid'
            assert all(r['prepared']is False for r in candidate['actual'])
            assert all(r.get('prepared',True)is True for r in candidate['prepared'])
            for row in candidate['emission']:
                raw = base64.b64decode(row['row40'])
                flags, symbol = struct.unpack_from('<H', raw, 4)[0], struct.unpack_from('<H', raw, 8)[0]
                expected = emit(flags, symbol)
                assert row.get('refused', False) == (expected is None)
                if expected is not None:
                    assert base64.b64decode(row['utf8']) == expected
                emitted += 1
            for row in candidate['actual']+candidate['prepared']:
                expected = span(row['key'])
                if row.get('refused', False):
                    assert expected is None
                    refused += 1
                    continue
                assert expected is not None
                expected_bytes = b''.join(struct.pack('<6i', *a, *z) for a, z in expected)
                assert base64.b64decode(row['source24']) == expected_bytes
                assert row['positions'] == project(expected)
                projected += len(row['positions'])
                if row['prepared']:
                    prepared += 1
                else:
                    assert row['scoreBits'] == 0x3f800000 and row['before'] == row['after']
                    actual += 1
        before, after = source['overlap']['before'], source['overlap']['after']
        assert before == after, 'Complete shared pool changed when refreshing the same base key from another actual candidate'
        assert len({n['key'] for n in before}) == len(before)
        for n in before:
            rows = base64.b64decode(n['rows'])
            assert len(rows) == ((n['packed'] & 0x7fff) >> 2)*40
            overlap_bytes += len(rows)
    assert len(d['inputs']) == d['overlap'] == 3
    assert (actual, prepared, refused, emitted) == tuple(d[k] for k in ['actualSpans', 'preparedSpans', 'refused', 'emitted'])
    assert actual==sum(2*sum(((n['key']>>24)&0xffff)>=0x8000 for n in source['initialPool'])for source in d['inputs'])
    assert d['guards']==8*sum(len(source['candidates'])for source in d['inputs'])+emitted+3*(actual+prepared)
    assert modes == {0, 1} and len(branches) == 6
    return actual,prepared,refused,emitted,projected,overlap_bytes

d=json.loads(Path(sys.argv[2]).read_bytes())
actual,prepared,refused,emitted,projected,overlap_bytes=audit(d)
negative=0
for mutate in(
    lambda c:c['inputs'][0]['candidates'][0]['emission'].pop(),
    lambda c:c['inputs'][0]['candidates'][0]['emission'].reverse(),
    lambda c:c['inputs'][0]['candidates'][0]['actual'].pop(),
    lambda c:c['inputs'][0]['candidates'][0]['prepared'].pop(),
    lambda c:c['inputs'][0]['candidates'][0]['actual'][0].__setitem__('source24','AA=='),
    lambda c:c.__setitem__('guards',c['guards']-1),
):
    changed=copy.deepcopy(d);mutate(changed)
    try:audit(changed)
    except AssertionError:negative+=1
    else:raise AssertionError('Corrupt configured-span capture admitted')
assert negative==6
print(f'PASS independent {actual} inferred span owners/{prepared} prepared exports/{refused} bounded refusals/{projected} insertion positions/{emitted} raw AK emissions/{overlap_bytes} complete sameTOP overlap bytes/all6 span branches/{d["guards"]} derived guards/6negativecaptures.')
