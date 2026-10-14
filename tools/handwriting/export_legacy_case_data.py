"""Read-only pinned scalar casing tables. Emits data; never loads the vendor engine."""
import argparse
import hashlib
import json
import struct
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

ENGINE = 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
UCD = '68dfc414d28257b9b5d6ddbb8b466c768c00ebdf6cbf7784364a9b6cad55ee8f'
SPECIAL = 'dfc4f159c5c68328114ff17cd520451714a72ff48657287e5fe2f64344980695'

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--unicode-data', required=True, type=Path)
    parser.add_argument('--special-casing', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args()
    if args.output.suffix.lower()!='.json': raise ValueError('Output must be a .json data file')
    engine = Binary('MyScriptEngine.dll')
    if hashlib.sha256(engine.data).hexdigest() != ENGINE or hashlib.sha256(args.unicode_data.read_bytes()).hexdigest() != UCD or hashlib.sha256(args.special_casing.read_bytes()).hexdigest() != SPECIAL:
        raise ValueError('Pinned engine/public Unicode source bytes differ')
    def rows(address, count, width):
        return [list(struct.unpack_from('<' + 'i' * width, engine.data, engine.off(address) + i * width * 4)) for i in range(count)]
    result = dict(format='legacy-native-case-data-v1', engineSha256=ENGINE, unicodeReferenceVersion='9.0.0', unicodeDataSha256=UCD, specialCasingSha256=SPECIAL,
        alternatingRanges=rows(0x1801df210,58,2), lowerRanges=rows(0x1801df070,34,3), lowerSingles=rows(0x1801debb0,89,2),
        upperRanges=rows(0x1801deec0,35,3), upperSingles=rows(0x1801de840,110,2), titleSingles=rows(0x1801dee80,8,2),
        specialRows=rows(0x180201820,130,9), sequenceScalars=rows(0x1802012b0,348,1))

    def simple(cp):
        lower = upper = title = None
        lower_range = next(((lo,hi) for lo,hi in result['alternatingRanges'] if lo <= cp <= hi),None)
        upper_range = next(((lo,hi) for lo,hi in result['alternatingRanges'] if lo <= cp-1 <= hi),None)
        if lower_range is not None:
            if (cp-lower_range[0])%2 == 0: lower = cp+1
        else:
            for lo,hi,delta in result['lowerRanges']:
                if lo <= cp <= hi: lower = cp+delta; break
            if lower is None:
                lower = next((cp+delta for value,delta in result['lowerSingles'] if value == cp),None)
        if upper_range is not None:
            if (cp-upper_range[0])%2 == 1: upper = cp-1
        else:
            for lo,hi,delta in result['upperRanges']:
                if lo <= cp <= hi: upper = cp+delta; break
            if upper is None:
                upper = next((cp+delta for value,delta in result['upperSingles'] if value == cp),None)
        for value, delta in result['titleSingles']:
            if value == cp: title = cp+delta
        return (cp if lower is None else lower, cp if upper is None else upper, title if title is not None else (cp if upper is None else upper)), lower is not None or upper is not None

    #This batch restores the null-output count queried by04FFE0, not the full
    #72-byte variant formatter. Native variants need not equal a canonical
    #simple lowercase/uppercase/title triple (notably the titlecase digraphs).
    compared = 0
    special_points = {row[0] for row in result['specialRows']}
    public_special = {int(line.split(';')[0],16) for line in args.special_casing.read_text(encoding='utf8').splitlines() if line.strip() and not line.lstrip().startswith('#')}
    differences = []
    presence_differences = []
    for line in args.unicode_data.read_text(encoding='utf8').splitlines():
        fields = line.split(';'); cp = int(fields[0],16)
        expected = (int(fields[13],16) if fields[13] else cp, int(fields[12],16) if fields[12] else cp,
                    int(fields[14],16) if fields[14] else (int(fields[12],16) if fields[12] else cp))
        actual, present = simple(cp)
        if cp == 0xb5: continue #Exact native exception.
        native_has = present or cp in special_points
        public_has = cp in public_special or any(value != cp for value in expected)
        if native_has != public_has:
            presence_differences.append(cp)
        if actual != expected: differences.append(cp)
        compared += 1
    payload = bytearray()
    for name in ['alternatingRanges','lowerRanges','lowerSingles','upperRanges','upperSingles','titleSingles','specialRows','sequenceScalars']:
        table = result[name]; payload += struct.pack('<i',len(table))
        for row in table: payload += struct.pack('<'+'i'*len(row),*row)
    result['tableSha256'] = hashlib.sha256(payload).hexdigest()
    result['genericVariantDifferencesFromSimpleCasing'] = differences
    result['casePresenceDifferencesFromUnicodeReference'] = presence_differences
    args.output.parent.mkdir(parents=True,exist_ok=True)
    #The manifest hashes wire bytes, so output must not depend on host newline translation.
    args.output.write_bytes((json.dumps(result,indent=2)+'\n').encode('utf8'))
    print(f'READ {compared} public reference records; {len(presence_differences)} presence differences retained; {len(differences)} variant/simple-triple differences retained, native130 special rows, SHA {result["tableSha256"]}. Contextual matching/case292 remain separate.')

if __name__ == '__main__': main()
