"""Actual global WORD chains through the separate21FBF0 quote oracle."""
import base64
import hashlib
import importlib.util
import json
import struct
import subprocess
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[2]
if len(sys.argv) != 3:
    raise SystemExit('AK.res capture.json')
ak, path = map(Path, sys.argv[1:])
data = ak.read_bytes()
assert hashlib.sha256(data).hexdigest() == '7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
subprocess.run([sys.executable, str(root/'tools/LegacyMainGlobalFirstAlternative.Probe/audit_output.py'), str(path)], check=True)
spec = importlib.util.spec_from_file_location('raw_alphabet', root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)
alphabet = module.read_alphabet(data)
inverse = {text: label for label, text in alphabet.items()}
count = struct.unpack_from('<H', data, 113)[0]
unit = struct.unpack_from('<H', data, 105)[0]

def u16(row, offset):
    return struct.unpack_from('<H', row, offset)[0]

def read(row, scratch):
    kind, label = u16(row, 4) & 7, u16(row, 8)
    if kind == 3:
        assert label & 0x7ffc == 0, 'Nonempty control owner is not supplied'
        return scratch
    value = ((label & 3)-7) & 65535 if kind == 1 else label if kind in (0, 2, 4) else 65535
    struct.pack_into('<H', scratch, 0, value)
    scratch[2] = int(kind == 4)
    return scratch

def storage(nodes):
    result = {}
    for node in nodes:
        key = node['key']
        packed = node['packed']
        signed = ((packed << 2) & 65535)
        signed = signed if signed < 32768 else signed-65536
        size = signed >> 4
        rows = base64.b64decode(node['rows'], validate=True)
        assert size >= 0 and len(rows) == size*40
        assert key not in result
        result[key] = [bytearray(rows[i*40:(i+1)*40]) for i in range(size)]
    return result

capture = json.loads(path.read_bytes())
assert capture['quoteGuards'] == 56
words = writes = checked = 0
for owner in capture['owners']:
    quotes = owner['quotes']
    before, after = storage(quotes['before']), storage(quotes['after'])
    assert list(before) == list(after)
    leading, trailing = bytearray.fromhex('c14ea573'), bytearray.fromhex('d25fb684')
    def resolve(reference):
        key = (reference & 0xffffff) | (reference & 0xffff000000) | (reference & (1 << 56))
        ordinal = (reference >> 40) & 65535
        assert ordinal < 32768
        return before[key][ordinal]
    assert len(quotes['heads']) == owner['words']
    for head in quotes['heads']:
        chain, seen, reference = [], set(), head
        while reference != (1 << 64)-1:
            assert reference not in seen and len(seen) < 4096
            seen.add(reference)
            row = resolve(reference)
            chain.append(row)
            reference = struct.unpack_from('<Q', row, 32)[0]
        assert chain
        words += 1
        for row in chain:
            read(row, leading)
            label = u16(leading, 0)
            if leading[2] == 0 and label == unit:
                continue
            if leading[2] == 0 and label in (count+1, count+2):
                struct.pack_into('<H', row, 8, inverse["'" if label == count+1 else '"'])
                writes += 1
            break
        read(chain[-1], trailing)
        if trailing[2] == 0 and u16(trailing, 0) == count+1:
            struct.pack_into('<H', chain[-1], 8, inverse["'"])
            writes += 1
    assert before == after, 'Full actual pool records, including unvisited ones, must match'
    assert bytes(leading) == base64.b64decode(quotes['leading'], validate=True)
    assert bytes(trailing) == base64.b64decode(quotes['trailing'], validate=True)
    checked += sum(len(rows)*40 for rows in after.values())
assert words == 8 and writes == 0
print(f'PASS rawAK/full linked WORD quote oracle: {words} actual words/{checked} pool bytes/56 guards; no actual synthetic-quote fixture or accepted-text claim')
