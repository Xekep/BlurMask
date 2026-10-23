"""Independent STATIC expected rich-pool refresh from audited B4/B6 values.

No SDK loading, NN evaluation, target labels or C# B7 output is consumed.
Range ownership/bounds are checked; original-coordinate range arrays need the
separate chronology oracle before a future runtime can be integrated.
"""
import argparse
import ast
import base64
import hashlib
import json
import struct
from pathlib import Path

INF = 0x4000000
MASK = 0x010000ffffffffff


def f(x):
    return struct.unpack('<f', struct.pack('<f', x))[0]


def sha(data):
    return hashlib.sha256(data).hexdigest()


def definitions(path, names):
    tree = ast.parse(path.read_text(encoding='utf-8-sig'))
    body = [n for n in tree.body if isinstance(n, (ast.Import, ast.ImportFrom)) or
            isinstance(n, (ast.ClassDef, ast.FunctionDef)) and n.name in names]
    scope = {'__file__': str(path)}
    exec(compile(ast.Module(body=body, type_ignores=[]), str(path), 'exec'), scope)
    return scope


def raw_categories(models):
    data = (models / 'en-US/en_US-ak-cur.res').read_bytes()
    assert sha(data) == '355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'
    at = 8843 + 99
    header = list(struct.unpack_from('<8H', data, at))
    assert header == [181, 178, 65535, 180, 184, 182, 183, 217]
    # Parse the actual stored AL; no caller-supplied label/category mapping.
    at += 16

    def u16():
        nonlocal at
        value = struct.unpack_from('<H', data, at)[0]
        at += 2
        return value

    count = u16()
    for increment in [32, 64, 1]:
        count = next(v for v in reversed([u16() for _ in range(count)]) if v != 65535) + increment
    spans = [u16() for _ in range(count + 1)]
    entries = []
    for _ in range(spans[-1] + 1):
        offset, label = struct.unpack_from('<II', data, at)
        entries.append((offset, label & 65535))
        at += 8
    blob = data[at:at + entries[-1][0]]
    labels = {blob[a:b].decode('utf8'): label for (a, label), (b, _) in zip(entries, entries[1:])}
    bypass = {labels[s] for s in ['\U000f0001', '\U000f0002', '\U000f0003', '\U000f0004', '\U000f0008'] if s in labels}
    bypass.update(header[4:7])
    assert bypass == {179, 182, 183, 184, 189, 190, 211}
    at = 17017
    assert struct.unpack_from('<HH', data, at) == (0x201, 217)
    at += 4

    def ids():
        nonlocal at
        n = data[at]
        at = (at + 4) & ~3
        values = struct.unpack_from('<' + 'i' * n, data, at)
        at += n * 4
        return values

    first = ids()
    fm = struct.unpack_from('<220b', data, at)
    at += 220
    geometry = ids()
    sm = struct.unpack_from('<220b', data, at)
    at += 220
    em = struct.unpack_from('<220b', data, at)
    assert len(geometry) == 24
    categories, maps = [], []
    for a, b, c in zip(fm, sm, em, strict=True):
        row = []
        for x, y in [(a, b), (a, c), (first.index(0), b), (first.index(0), c)]:
            if x == -1 or y == -1:
                row.append(-1)
                continue
            item = (first[x], y)
            if item not in categories:
                categories.append(item)
            row.append(categories.index(item))
        maps.append(row)
    assert len(categories) == 42
    return bypass, maps, categories


def initialize(storage, descriptor):
    row = bytearray(storage)
    assert len(row) == 40
    struct.pack_into('<i', row, 0, descriptor)
    flags = struct.unpack_from('<H', row, 4)[0]
    struct.pack_into('<H', row, 4, flags & 0xfff0)
    struct.pack_into('<I', row, 6, 0xffff0000)
    # Only the explicitly written native fields are initialized. Opaque A/B
    # retains allocator storage; hex16/17 is written by I32@14, not opaque.
    struct.pack_into('<f', row, 12, 1.)
    struct.pack_into('<f', row, 16, -1.)
    struct.pack_into('<i', row, 20, 0)
    row[24:32] = bytes(8)
    row[32:40] = bytes([255]) * 8
    return row


def append(node, descriptor, force, storage):
    if not force and not node['force']:
        for index, row in enumerate(node['rows']):
            if struct.unpack_from('<i', row)[0] == descriptor:
                return index, True
    assert len(node['rows']) < 2040
    node['force'] |= force
    node['rows'].append(initialize(storage, descriptor))
    return len(node['rows']) - 1, False


def clone(owners):
    result = {}
    for source in owners:
        key, packed = source['Key'], source['PackedCount']
        raw = base64.b64decode(source['Records'])
        assert key == key & MASK and key not in result
        count = (packed >> 2) & 2047
        assert 1 <= count <= 2040 and len(raw) == count * 40
        node = dict(force=False, rows=[])
        for index in range(count):
            original = raw[index * 40:(index + 1) * 40]
            descriptor = struct.unpack_from('<i', original)[0]
            ordinal, existing = append(node, descriptor, bool(packed & 32768), bytes(40))
            assert ordinal == index and not existing
            node['rows'][ordinal] = bytearray(original)
        assert ((len(node['rows']) << 2) | (32768 if node['force'] else 0)) == packed
        assert b''.join(node['rows']) == raw
        result[key] = node
    return result


def refresh(row, symbol, likelihood, score, cost):
    # The admitted non-header ordinary branch. Header prototype remains OPEN.
    assert symbol not in [182, 183, 184]
    flags = struct.unpack_from('<H', row, 4)[0]
    struct.pack_into('<H', row, 4, flags & 0xfff8 & 0xfff7)
    struct.pack_into('<H', row, 8, symbol)
    publication = struct.unpack_from('<H', row, 6)[0] | 0x40
    if symbol == 209:
        publication |= 8
    struct.pack_into('<H', row, 6, publication)
    struct.pack_into('<f', row, 12, f(f(likelihood) / f(255.)))
    struct.pack_into('<f', row, 16, f(score))
    struct.pack_into('<i', row, 20, cost)


def output_owners(pool):
    return [dict(Key=key, PackedCount=(len(node['rows']) << 2) | (32768 if node['force'] else 0),
                 Records=base64.b64encode(b''.join(node['rows'])).decode()) for key, node in pool.items()]


def parent(old, source, contexts, profile):
    assert old['Name'] == source['Name']
    bypass, maps, categories = profile
    children, observed = [], dict(references=0, ordinary=0, attempts=0, fresh=0, existing=0, differentSymbols=0)
    for c4, c6 in zip(old['Children'], source['Children'], strict=True):
        assert c4['Descriptor'] == c6['Descriptor']
        assert [v['Residue'] for v in c6['Variants']] == [0, 1]
        cx = contexts[source['Name'], c6['Descriptor']]
        groups = {g['Group']: g for g in c4['Groups']}
        a8 = [base64.b64decode(a['Bytes']) for a in c4['A8']]
        plans, touched = [], set()
        refs = [ref for word in c6['Variants'][0]['Words'] for ref in word['References']]
        observed['references'] += len(refs)
        for encounter, ref in enumerate(refs):
            arc = (ref >> 24) & 65535
            if arc & 32768:
                continue
            assert (ref >> 56) & 1 == 0
            node, key, selected = ref & 0xffffff, ref & MASK, (ref >> 40) & 65535
            assert selected < 32768
            current = cx['Nodes'][node][arc]
            assert 0 <= (current['End'] >> 7) - (current['Start'] >> 7) <= 7
            touched.add(key)
            observed['ordinary'] += 1
            assert len(a8[node]) == 200
            cell = None
            for group in range(5):
                item = a8[node][group * 40:(group + 1) * 40]
                if struct.unpack_from('<i', item)[0] < INF and struct.unpack_from('<H', item, 16)[0] == arc:
                    cell = item
                    break
            if cell is None:
                continue
            symbol = struct.unpack_from('<H', cell, 12)[0]
            if symbol in [182, 183, 184]:
                raise ValueError('Reached header prototype is not closed; refuse before claim')
            category = maps[symbol][0]
            assert category >= 0
            prototype = categories[category][1]
            g = groups[c4['Nodes'][node][arc]['Group']]
            assert len(g['Entries']) == len(g['Confidence']) <= 10
            scores = {}
            for entry, score in zip(g['Entries'], g['Confidence'], strict=True):
                scores.setdefault(entry[0], f(score))
            ordinal = next((i for i, entry in enumerate(g['Entries']) if entry[0] == symbol), len(g['Entries']))
            cost = INF if ordinal >= len(g['Entries']) or ordinal > 19 else g['Entries'][ordinal][1]
            if cost < INF and symbol not in bypass:
                cost += g['Geometry']
            request = bytearray(16)
            struct.pack_into('<iHHHb', request, 0, node, arc, 65535, symbol, prototype)
            request[11] = 1
            plans.append(dict(encounter=encounter, reference=ref, key=key, selected=selected,
                              descriptor=10 + group, symbol=symbol, score=scores.get(symbol, 0.),
                              cost=cost, likelihood=cell[8], request=request.hex(), category=category,
                              prototype=prototype))
        variants = []
        for variant in c6['Variants']:
            pool = clone(variant['Owners'])
            stream = hashlib.sha256()
            rows, hits, different = 0, 0, 0
            for plan in plans:
                original_symbol = struct.unpack_from('<H', pool[plan['key']]['rows'][plan['selected']], 8)[0]
                different += original_symbol != plan['symbol']
                ordinal, existing = append(pool[plan['key']], plan['descriptor'], False, bytes(40))
                if existing:
                    hits += 1
                else:
                    rows += 1
                    refresh(pool[plan['key']]['rows'][ordinal], plan['symbol'], plan['likelihood'], plan['score'], plan['cost'])
                # Every attempted append in exact WORD/flat-reference order.
                stream.update(struct.pack('<iQQiiHH', plan['encounter'], plan['reference'], plan['key'],
                                          plan['descriptor'], ordinal, plan['symbol'], int(existing)))
                if not existing:
                    stream.update(bytes.fromhex(plan['request']))
                    stream.update(struct.pack('<if', plan['cost'], plan['score']))
            result = dict(Residue=variant['Residue'], NewRows=rows, ExistingHits=hits,
                          AppendAttempts=len(plans), AttemptSha256=stream.hexdigest(), Owners=output_owners(pool))
            variants.append(result)
            if variant['Residue'] == 0:
                observed['attempts'] += len(plans)
                observed['fresh'] += rows
                observed['existing'] += hits
                observed['differentSymbols'] += different
        children.append(dict(Descriptor=c6['Descriptor'], TouchedOwners=len(touched),
                             RangeCalls=2 * len(touched), MaximumRangeRows=8 * len(touched),
                             WordReferences=[w['References'] for w in c6['Variants'][0]['Words']],
                             QueryPlans=plans, Variants=variants))
    return dict(Name=source['Name'], Children=children), observed


def main():
    p = argparse.ArgumentParser()
    for name in ['repo', 'models', 'b4', 'b6', 'contexts', 'output']:
        p.add_argument('--' + name, type=Path, required=True)
    a = p.parse_args()
    Cases = definitions(a.repo / 'tools/LegacyFullEnglishFirstContext.Probe/audit_output.py', {'Cases'})['Cases']
    original = a.b6.read_bytes()
    b6 = json.loads(original)
    rows = json.loads(a.contexts.read_bytes())
    assert all(r['NNEvaluated'] is False for r in rows)
    contexts = {(r['Name'], r['Descriptor']): r for r in rows}
    assert len(contexts) == len(rows)
    profile = raw_categories(a.models)
    totals, result = {}, []
    for old, source in zip(Cases(a.b4), b6['Actual'], strict=True):
        expected, observed = parent(old, source, contexts, profile)
        result.append(expected)
        for key, value in observed.items():
            totals[key] = totals.get(key, 0) + value
    assert sum(len(row['Children']) for row in result) == len(contexts)
    assert a.b6.read_bytes() == original
    report = dict(Status='STATIC_EXPECTED_ONLY_NO_B7_RUNTIME', TargetsRead=False, NNEvaluated=False,
                  RangeCoordinates='OPEN until future raw chronology/range oracle; only bound/identity checked here',
                  Actual=result, Counts=totals, B6Sha256=sha(original))
    a.output.write_text(json.dumps(report, separators=(',', ':')) + '\n')
    print(json.dumps(dict(status=report['Status'], cases=len(result), counts=totals,
                          sha256=sha(a.output.read_bytes()))))


if __name__ == '__main__':
    main()
