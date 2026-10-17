"""Independent raw AK -> copied imported2A/WORD bytes on published owners."""
import ast
import base64
import hashlib
import json
import struct
import subprocess
import sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)!=3:raise SystemExit('AK capture.json')
ak,path=map(Path,sys.argv[1:]);data=ak.read_bytes();capture=json.loads(path.read_bytes())
subprocess.run([sys.executable,str(root/'tools/LegacyMainGlobalPublishedCleanup.Probe/audit_output.py'),str(ak),str(path)],check=True)
assert capture['importCount']==8 and capture['importGuards']==72
source=root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py';tree=ast.parse(source.read_text());ns={'struct':struct}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name=='read_alphabet'],type_ignores=[]),str(source),'exec'),ns)
alphabet=ns['read_alphabet'](data);label_count=struct.unpack_from('<H',data,113)[0];unit=struct.unpack_from('<H',data,105)[0]
D=lambda x:base64.b64decode(x,validate=True)
reserved={65515+i:chr(0xf0018-i).encode() for i in range(9)}
reserved.update({65526:chr(0xf0000).encode(),65529:b'\n',65531:b' ',65532:b'\xc2\xa0'})
source=root/'tools/LegacyControlAlphabetImport.Probe/audit_output.py';tree=ast.parse(source.read_text())
ns={'struct':struct,'D':D,'alphabet':alphabet,'label_count':label_count,'unit':unit,'reserved':reserved}
keep=[n for n in tree.body if isinstance(n,ast.ClassDef) and n.name=='Alphabet' or isinstance(n,ast.FunctionDef) and n.name in {'u16','nodes','convert'}]
exec(compile(ast.Module(body=keep,type_ignores=[]),str(source),'exec'),ns)
source=root/'tools/LegacyImportedWordBytes.Probe/audit_output.py';tree=ast.parse(source.read_text())
byte_ns={'struct':struct,'reserved':reserved,'U':lambda b,p:struct.unpack_from('<H',b,p)[0]}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name in {'fallback','valid','expected'}],type_ignores=[]),str(source),'exec'),byte_ns)
processed=changed=symbols_count=words=bytes_count=0
for owner in capture['owners']:
 for case in owner['cases']:
  sample=case['import'];p,c,n=ns['convert'](sample);assert sample['succeeded']
  processed+=p;changed+=c;symbols_count+=n
  records=ns['nodes'](sample['after']);symbols=list(map(D,sample['symbols']))
  publications=[part['published'] for part in reversed(case['flushes']) if part['published'] is not None]
  assert len(publications)==len(sample['words'])==len(sample['rawWords'])==1
  for word,published,raw in zip(sample['words'],publications,sample['rawWords'],strict=True):
   assert D(word['storage120'])==D(published['word120'])
   assert word['references']==published['flat']
   assert(word['first'],word['last'],word['wordIndex'])==(published['first'],published['last'],published['index'])
   assert [g['reference'] for g in raw['glyphs']]==word['references']
   expected_word=bytearray()
   for ref,glyph in zip(word['references'],raw['glyphs'],strict=True):
    key=(ref&0xffffff)|(ref&0xffff000000)|(ref&(1<<56));ordinal=(ref>>40)&65535
    row=records[key][1][ordinal*40:ordinal*40+40]
    assert D(glyph['record40'])==row
    result=byte_ns['expected'](row,symbols);assert result is not None
    expected,tokens,_=result
    assert D(glyph['bytes'])==expected and glyph['tokens']==tokens
    expected_word.extend(expected)
   assert D(raw['bytes'])==expected_word and raw['validUtf8']==byte_ns['valid'](expected_word)
   words+=1;bytes_count+=len(expected_word)
assert words==8 and processed==16 and changed==16
print(f'PASS independent publishedimport:8actualimports/{processed} full40 rows/{symbols_count} private2A entries/{words} copiedWORDs/{bytes_count} rawbytes/72guards; no target text')
