"""Independent full WORD/pool/packet chain and raw AK UTF8/source ranges.
Only the older oracle's fixed fixture totals are removed. Its math/byte checks
are unchanged; no expected transcript or C# implementation is imported.
"""
import ast
import base64
import hashlib
import json
import struct
import sys
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('AK capture.json')
root=Path(__file__).resolve().parents[2];capture=Path(sys.argv[2]).resolve()
d=json.loads(capture.read_bytes());assert len(d['owners'])==2 and d['noWinnerGuards']==2
assert [o['preparedEarlyGate'] for o in d['owners']]==[False,True]
source=root/'tools/LegacyMainGlobalGlyphExecution.Probe/audit_output.py'
tree=ast.parse(source.read_text());keep=[];removed=[]
for node in tree.body:
    text=ast.unparse(node)
    if isinstance(node,ast.Assert) and (text.startswith("assert len(d['owners'])") or text.startswith("assert len(d['preparedOutcomes'])") or text.startswith('assert callbacks == 64')):
        removed.append(node)
    elif isinstance(node,ast.Expr) and isinstance(node.value,ast.Call) and isinstance(node.value.func,ast.Name) and node.value.func.id=='print':
        pass
    else:keep.append(node)
assert len(removed)==3
previous=sys.argv
try:
    sys.argv=[str(source),str(capture)]
    ns={'__file__':str(source),'__name__':'__main__'}
    exec(compile(ast.Module(body=keep,type_ignores=[]),str(source),'exec'),ns)
finally:sys.argv=previous
assert(ns['callbacks'],ns['words'],ns['zero'])==(16,8,8)
#Raw resource parser and UTF8 scalar emitter from independently written Python.
raw=Path(sys.argv[1]).read_bytes();assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
al_source=root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py'
al_tree=ast.parse(al_source.read_text());functions=[n for n in al_tree.body if isinstance(n,ast.FunctionDef) and n.name=='read_alphabet']
al_ns={'struct':struct};exec(compile(ast.Module(body=functions,type_ignores=[]),str(al_source),'exec'),al_ns)
alphabet=al_ns['read_alphabet'](raw);count=struct.unpack_from('<H',raw,113)[0]
emit_source=root/'tools/LegacyMainWordUtf8.Probe/audit_output.py';emit_tree=ast.parse(emit_source.read_text())
needed={'emit','u16','i32','u64','canonical'};functions=[n for n in emit_tree.body if isinstance(n,ast.FunctionDef) and n.name in needed]
emit_ns={'struct':struct,'alphabet':alphabet,'count':count};exec(compile(ast.Module(body=functions,type_ignores=[]),str(emit_source),'exec'),emit_ns)
i32=emit_ns['i32'];u16=emit_ns['u16'];u64=emit_ns['u64'];emit=emit_ns['emit'];b64=base64.b64decode
glyphs=0
for owner in d['owners']:
    for case in owner['cases']:
        for part in case['flushes']:
            expected=[];rows=b64(part['rows72']);published=part['published'];items=part['rawGlyphs']
            if published is None:assert items==[] and b64(part['rawUtf8'])==b'';continue
            assert [g['reference'] for g in items]==published['flat']
            pool={n['key']:b64(n['records']) for n in part['pool']}
            for glyph in items:
                ref=glyph['reference'];key=ref&0x010000ffffffffff;ordinal=(ref>>40)&65535
                record=pool[key][ordinal*40:(ordinal+1)*40]
                assert record==b64(glyph['record']) and key==glyph['key']
                matches=[]
                for i in range(len(rows)//72):
                    row=rows[i*72:(i+1)*72];node=i32(row,0)
                    if u64(row,48)==key and i32(row,8)-owner['starts'][node]==i32(record,0):matches.append(i)
                assert matches==glyph['rows'] and matches
                kinds={rows[i*72+60]==3 for i in matches};assert kinds=={glyph['protocol']}
                if glyph['protocol']:assert glyph['original'] is None and glyph['ranges']==[]
                else:
                    assert glyph['original']==key
                    for i in matches:
                        row=rows[i*72:(i+1)*72];node=i32(row,0);arc=u16(row,4)
                        assert node|(arc<<24)==key
                        assert glyph['ranges']==owner['groupRanges'][owner['graphGroups'][node][arc]]
                emitted=emit(record);assert emitted==b64(glyph['utf8']);expected.append(emitted);glyphs+=1
            assert b64(part['rawUtf8'])==b''.join(expected)
print(f'PASS new published route:16 full callbacks/8WORD/8zero, pool40/WORD120/packet64/reset;{glyphs} raw-AK glyphs with real range ownership, no expected text.')
