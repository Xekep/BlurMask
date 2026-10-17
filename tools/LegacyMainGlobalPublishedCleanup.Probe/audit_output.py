"""Independent raw-AK quote cleanup then zero-write freshF4 proof.
Reuses the independent original scalar-reader definitions, not C# runtime.
"""
import ast
import base64
import json
import struct
import subprocess
import sys
from pathlib import Path
root = Path(__file__).resolve().parents[2]
if len(sys.argv) != 3:
    raise SystemExit('AK capture.json')
ak, capture = map(Path,sys.argv[1:])
subprocess.run([sys.executable,str(root/'tools/LegacyMainGlobalPublishedAlternative.Probe/audit_output.py'),str(ak),str(capture)],check=True)
d = json.loads(capture.read_bytes())
assert d['noOpCount'] == 8 and d['cleanupGuards'] == 100
source = root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py'
tree = ast.parse(source.read_text())
ns = {'struct':struct}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name=='read_alphabet'],type_ignores=[]),str(source),'exec'),ns)
alphabet = ns['read_alphabet'](ak.read_bytes())
inverse = {text:label for label,text in alphabet.items()}
count = struct.unpack_from('<H',ak.read_bytes(),113)[0]
unit = struct.unpack_from('<H',ak.read_bytes(),105)[0]
source = root/'tools/LegacyMainGlobalQuoteNormalization.Probe/audit_output.py'
tree = ast.parse(source.read_text());ns = {'struct':struct,'base64':base64}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name in {'u16','read','storage'}],type_ignores=[]),str(source),'exec'),ns)
u16, read = ns['u16'],ns['read']
def storage(nodes):
    return ns['storage']([dict(key=n['key'],packed=n['packed'],rows=n['records']) for n in nodes])
words = writes = checked = 0
for owner in d['owners']:
    for case in owner['cases']:
        q = case['cleanup']
        before, after = storage(q['before']),storage(q['afterQuote'])
        assert list(before) == list(after)
        leading, trailing = bytearray.fromhex('c14ea573'),bytearray.fromhex('d25fb684')
        def resolve(reference):
            key = (reference & 0xffffff) | (reference & 0xffff000000) | (reference & (1<<56))
            ordinal = (reference>>40)&65535
            assert ordinal < 32768
            return before[key][ordinal]
        assert len(q['heads']) == 1
        for head in q['heads']:
            chain,seen,reference = [],set(),head
            while reference != (1<<64)-1:
                assert reference not in seen and len(seen) < 4096
                seen.add(reference);row = resolve(reference);chain.append(row)
                reference = struct.unpack_from('<Q',row,32)[0]
            assert chain
            words += 1
            for row in chain:
                read(row,leading);label = u16(leading,0)
                if leading[2] == 0 and label == unit:
                    continue
                if leading[2] == 0 and label in (count+1,count+2):
                    struct.pack_into('<H',row,8,inverse["'" if label==count+1 else '"']);writes += 1
                break
            read(chain[-1],trailing)
            if trailing[2] == 0 and u16(trailing,0) == count+1:
                struct.pack_into('<H',chain[-1],8,inverse["'"]);writes += 1
        assert before == after, 'Entire shared glyph pool must match scalar native quote oracle'
        assert bytes(leading) == base64.b64decode(q['leading'],validate=True)
        assert bytes(trailing) == base64.b64decode(q['trailing'],validate=True)
        assert after == storage(q['afterNoOp'])
        assert q['scoresBefore'] == q['scoresAfter']
        assert q['property'] == 0x012d0017 and q['f4'] == 0 and q['executed'] is False
        checked += sum(len(rows)*40 for rows in after.values())
assert words == 8 and writes == 0
print(f'PASS independent actualpublished quote/F4:8 WORDs/{checked} wholepool bytes,100 guards; actualsyntheticquotes0, no enabled rewrite/acceptance claim')
