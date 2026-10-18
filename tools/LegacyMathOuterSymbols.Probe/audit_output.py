"""Actual first-copy -> separate outer symbols; independent native copy/prefix oracle.

Only tracked stdlib/NumPy references. Does not execute a vendor SDK library.
"""
from pathlib import Path
import base64
import copy
import hashlib
import json
import subprocess
import sys

if len(sys.argv) not in (5, 6):
    raise SystemExit('capture.json rawMath2.res firstcopy.json sourcebound.json [same-platform-other.json]')
root = Path(__file__).resolve().parents[2]
subprocess.run([sys.executable, '-I', str(root / 'tools/LegacyMathFirstResultCopy.Probe/audit_output.py'),
                sys.argv[3], sys.argv[2], sys.argv[4]], check=True)
data = json.loads(Path(sys.argv[1]).read_bytes())
first = json.loads(Path(sys.argv[3]).read_bytes())
assert data['Resource'] == hashlib.sha256(Path(sys.argv[2]).read_bytes()).hexdigest() == '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'


def alternative(row, pool):
    payload = base64.b64decode(row['Bytes'])
    if payload not in pool:
        pool[payload] = len(pool) + 1
    assert row['ByteCount'] == len(payload)
    return dict(Identity=pool[payload], ByteCount=len(payload), ScalarBits=row['ScalarBits'], Bytes=row['Bytes'])


def symbols(source):
    pool = {}
    rows = [dict(Flags=r['Flags'], Metadata=r['Metadata'], Alternatives=[alternative(r['Alternative'], pool)],
                 Ranges=source['Ranges'][i]) for i, r in enumerate(source['Rows'])]
    return rows, pool


def value_ranges(rows):
    return [dict(First=p['First']['Value'], Last=p['Last']['Value']) for p in rows]


def compact_prefix(rows):
    return [None if r is None else dict(Flags=r['Flags'], Metadata=r['Metadata'],
            AlternativeCount=r['AlternativeCount'], AlternativeStorage=r['AlternativeStorage'],
            Ranges=value_ranges(r['Ranges'])) for r in rows]


assert len(data['Actual']) == 4 and data['Aliases'] == 1 and data['Guards'] == 107
for index, (actual, original) in enumerate(zip(data['Actual'], first['Actual'], strict=True)):
    assert actual['Width'] == original['Width'] and actual['Repeated'] == (index == 3)
    assert not actual['DecoderRetained']
    assert actual['Before']['Ranges'] == original['Before']['Ranges']
    for a, b in zip(actual['Before']['Rows'], original['Before']['Rows'], strict=True):
        assert a['Unit'] == b['Word'] and a['Flags'] == b['Flags'] and a['Metadata'] == b['Metadata']
        assert a['Alternative'] == dict(Identity=b['Identity'], ByteCount=b['Length'], ScalarBits=b['ScalarBits'], Bytes=b['Bytes'])
    expected, pool = symbols(actual['Before'])
    assert actual['After'] == expected
    assert actual['FirstCopyReservation'] == original['Statistics']['PayloadReservation']
    stats = actual['Statistics']; n = len(expected); ranges = sum(len(r['Ranges']) for r in expected)
    assert stats['Complete'] and not (stats['Failed'] or stats['Released']) and stats['Phase'] == 'BeforeCompletion'
    assert [stats[k] for k in ['Capacity', 'InitializedRows', 'VectorCount', 'MetadataRows', 'AlternativesWritten', 'RangeRows', 'CopiedRows']] == [n] * 7
    assert stats['RangesWritten'] == ranges and stats['StringsInterned'] == len(pool) and stats['ReleaseCount'] == 0
    prefix = [dict(Flags=r['Flags'], Metadata=r['Metadata'], AlternativeCount=1,
                   AlternativeStorage=r['Alternatives'], Ranges=r['Ranges']) for r in expected]
    assert compact_prefix(stats['Prefix']) == prefix
    strings = sum(5 * len(p) + 1 for p in pool)
    peak = max((5 * len(p) for p in pool), default=0)
    assert stats['PayloadReservation'] == actual['FirstCopyReservation'] + 96 + n * 72 + ranges * 16 + strings + peak

stages = ['Claimed', 'AfterPool', 'AfterVectorReserve', 'AfterRowInitialized', 'AfterCount', 'AfterMetadata',
          'AfterAlternativeReserve', 'AfterIntern', 'AfterAlternative', 'AfterRanges', 'AfterRow', 'BeforeCompletion']
assert len(data['Failures']) == 24
source = data['Actual'][1]['Before']; n = len(source['Rows']); assert n >= 2


def snapshots():
    pool, storage, count, initialized, meta, alts, range_rows, ranges, copied = {}, [], 0, 0, 0, 0, 0, 0, 0
    output = {}
    def save(stage):
        output[stage] = dict(Capacity=len(storage), InitializedRows=initialized, VectorCount=count,
            MetadataRows=meta, AlternativesWritten=alts, RangeRows=range_rows, RangesWritten=ranges,
            CopiedRows=copied, StringsInterned=len(pool), Prefix=copy.deepcopy(storage))
    save('Claimed'); save('AfterPool')
    storage = [None] * n; save('AfterVectorReserve')
    for index in range(n):
        storage[index] = dict(Flags=0, Metadata=2**32-1, AlternativeCount=0, AlternativeStorage=[], Ranges=[])
        initialized += 1
        if index == 1: save('AfterRowInitialized')
    count = n; save('AfterCount')
    for index, row in enumerate(source['Rows']):
        cell = storage[index]; cell['Flags'] = row['Flags']; cell['Metadata'] = row['Metadata']; meta += 1
        if index == 1: save('AfterMetadata')
        cell['AlternativeStorage'] = [None]
        if index == 1: save('AfterAlternativeReserve')
        value = alternative(row['Alternative'], pool)
        if index == 1: save('AfterIntern')
        cell['AlternativeStorage'][0] = value; cell['AlternativeCount'] = 1; alts += 1
        if index == 1: save('AfterAlternative')
        cell['Ranges'] = source['Ranges'][index]; range_rows += 1; ranges += len(cell['Ranges'])
        if index == 1: save('AfterRanges')
        copied += 1
        if index == 1: save('AfterRow')
    save('BeforeCompletion')
    return output


expected_prefix = snapshots()
for index, stage in enumerate(stages):
    for cancel, failure in zip((False, True), data['Failures'][index * 2:index * 2 + 2], strict=True):
        assert failure['Stage'] == stage and failure['Canceled'] == cancel
        prefix = failure['Prefix']
        assert prefix['Phase'] == stage and not (prefix['Complete'] or prefix['Failed'] or prefix['Released'])
        assert prefix['PayloadReservation'] == data['Actual'][1]['Statistics']['PayloadReservation']
        assert prefix['ReleaseCount'] == 0
        expected = expected_prefix[stage]
        for key, value in expected.items():
            got = compact_prefix(prefix[key]) if key == 'Prefix' else prefix[key]
            assert got == value, (stage, key, got, value)
assert data['Gc'] == dict(AllowsFirstCopyCollection=True, ExplicitFirstCopyKeepAlive=True,
                          Rows=data['Actual'][1]['After'])
if len(sys.argv) == 6:
    assert Path(sys.argv[1]).read_bytes() == Path(sys.argv[5]).read_bytes()
print('PASS4 actual typed first-copy vectors/new pools/source ranges;24 initialized-count/alternative/range prefixes;107 guards/GC; no multi-C23/DOM')
