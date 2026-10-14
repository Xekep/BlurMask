"""Extract scalar Unicode service data after an independent public-UCD9 audit.

Read-only PE analysis; no DLL execution. Emits stable LF portable data, not code.
"""
import argparse
import bisect
import hashlib
import json
import struct
from pathlib import Path
from audit_legacy_character_properties import PE, ENGINE_SHA, UCD_SHA

DERIVED_SHA = '02d8e12cea7c61a9f3cc5fbf8eaceadf8da23fe83d60c64cf647088830b810cc'


def binary_table(ranges, decompositions, compositions):
    out = bytearray()
    for rows in [ranges, decompositions, compositions]:
        out += struct.pack('<i', len(rows))
        for row in rows:
            out += struct.pack('<'+'i'*len(row), *row)
    return bytes(out)


def export(engine, unicode_data, derived):
    if hashlib.sha256(engine).hexdigest() != ENGINE_SHA or hashlib.sha256(unicode_data).hexdigest() != UCD_SHA or hashlib.sha256(derived).hexdigest() != DERIVED_SHA:
        raise ValueError('Pinned source bytes differ')
    pe = PE(engine)
    ranges = [list(pe.read(0x1801f8560+i*12, 'iii')) for i in range(517)]
    previous = -1
    for lo, hi, ccc in ranges:
        if lo <= previous or hi < lo or hi > 0x10ffff or not 0 <= ccc <= 255:
            raise ValueError('Invalid native ordered combining-class range')
        previous = hi
    starts = [r[0] for r in ranges]

    def native_class(cp):
        index = bisect.bisect_right(starts, cp)-1
        return ranges[index][2] if index >= 0 and cp <= ranges[index][1] else -1

    public = {}; range_start = None; assigned = 0
    for line in unicode_data.decode('utf8').splitlines():
        fields = line.split(';'); cp = int(fields[0], 16); ccc = int(fields[3])
        if native_class(cp) != ccc:
            raise ValueError(f'Native/public assigned combining class differs: {cp:x}')
        assigned += 1
        if fields[1].endswith(', First>'):
            range_start = (cp, ccc)
        elif fields[1].endswith(', Last>'):
            lo, klass = range_start
            if klass != ccc or any(native_class(c) != ccc for c in range(lo+1, cp)):
                raise ValueError('Native/public assigned range differs')
            assigned += cp-lo-1; range_start = None
        if fields[5] and not fields[5].startswith('<'):
            public[cp] = [int(c, 16) for c in fields[5].split()]
    base, indices = pe.read(0x1802521f0, 'QQ')
    decompositions = []
    for position in pe.read(indices, '5722I'):
        header, cp = pe.read(base+position*4, 'II')
        if header >> 8 == 1:
            parts = list(pe.read(base+(position+2)*4, 'I'*(header & 255)))
            decompositions.append([cp, len(parts), *parts])
    if {r[0]: r[2:] for r in decompositions} != public or len(decompositions) != 2060:
        raise ValueError('Native/public canonical decompositions differ')
    excluded = set()
    for line in derived.decode('utf8').splitlines():
        line = line.split('#')[0].strip()
        if not line:
            continue
        fields = [v.strip() for v in line.split(';')]
        if fields[1] == 'Full_Composition_Exclusion':
            interval = fields[0].split('..')
            excluded.update(range(int(interval[0], 16), int(interval[-1], 16)+1))
    expected = {(p[0], p[1]): cp for cp, p in public.items() if len(p) == 2 and cp not in excluded}
    compositions = []
    for index in pe.read(0x1801f9da0, '940I'):
        header, cp, first, second = pe.read(base+index*4, 'IIII')
        if header != 0x102:
            raise ValueError('Unexpected native composition reference')
        compositions.append([first, second, cp])
    if {(r[0], r[1]): r[2] for r in compositions} != expected or len(compositions) != 940:
        raise ValueError('Native/public composition exclusions differ')
    sha = hashlib.sha256(binary_table(ranges, decompositions, compositions)).hexdigest()
    return {'format': 'legacy-native-unicode-normalization-data-v1', 'unicodeVersion': '9.0.0',
            'engineSha256': ENGINE_SHA, 'unicodeDataSha256': UCD_SHA, 'derivedNormalizationSha256': DERIVED_SHA,
            'tableSha256': sha, 'combiningRanges': ranges, 'canonicalDecompositions': decompositions,
            'compositions': compositions}, assigned


if __name__ == '__main__':
    p = argparse.ArgumentParser()
    p.add_argument('engine', type=Path)
    p.add_argument('unicode_data', type=Path)
    p.add_argument('derived_normalization', type=Path)
    p.add_argument('output', type=Path)
    args = p.parse_args()
    result, assigned = export(args.engine.read_bytes(), args.unicode_data.read_bytes(), args.derived_normalization.read_bytes())
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes((json.dumps(result, ensure_ascii=True, separators=(',', ':'))+'\n').encode('ascii'))
    print(f'PASS {assigned} public assigned CCC entries/ranges,2060 canonical decompositions,940 composition pairs; {len(result["combiningRanges"])} exact native ranges, tableSHA={result["tableSha256"]}.')
