"""Pinned read-only producer/callback ownership evidence; never execute DLLs."""
import argparse, hashlib, json, re
from pathlib import Path
from audit_legacy_character_properties import PE

p=argparse.ArgumentParser()
p.add_argument('--dll',type=Path,default=Path('research/myscript/sdk/build/x64/MyScriptText.dll'))
p.add_argument('--decompiled',type=Path,default=Path('research/myscript/output/MyScriptText.dll/decompiled.c'))
p.add_argument('--output',type=Path,required=True)
a=p.parse_args(); data=a.dll.read_bytes(); sha=hashlib.sha256(data).hexdigest()
if sha!='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f':raise ValueError('Unknown native evidence fingerprint')
pe=PE(data)
bindings={0x1802de610+0x48:0x180151bd0,0x1802de610+0x168:0x180152a10,
          0x1802de610+0x170:0x180152a30,0x1802de610+0x188:0x180152b20,
          0x1802e1ff0+0xb8:0x1801a7bf0,0x1802e1ff0+0xc0:0x1801a7c70,
          0x1802e1ff0+0x120:0x1801a9ae0,0x1802e1ff0+0x170:0x1801acf30}
for address,value in bindings.items():
    if pe.read(address,'Q')[0]!=value:raise ValueError(f'Producer table changed at{address:x}')
instructions={0x180152a10:'488b5270488b024c8b90b8000000',
              0x180152a30:'488b5270488b024c8b90c0000000',
              0x180152b20:'488b5270488b024c8b9020010000',
              0x1801acf30:'488b8288010000c3',
              0x1801a7c69:'8b440810c3',0x1801a7ce9:'8b440814c3'}
for address,value in instructions.items():
    if bytes(pe.read(address,f'{len(value)//2}B')).hex()!=value:raise ValueError(f'Callback implementation changed at{address:x}')
if pe.read(0x1802ed2e8,'d')[0]!=3 or pe.read(0x1802ed1b0,'d')[0]!=.02:raise ValueError('Submission double constants changed')
source=a.decompiled.read_text()
def function(address):
    m=re.search(r'/\* Address 180'+address+r';.*?(?=/\* Address |\Z)',source,re.S)
    if not m:raise ValueError(f'Required export missing:{address}')
    return m[0]
glyph=function('156950'); filter_source=function('157c60'); category=function('157340')
patterns=[(glyph,r'psVar13 = \*\(short \*\*\)\(param_3 \+ 0x68\)'),
          (glyph,r'plVar2 = \*\(longlong \*\*\)\(psVar13 \+ 0xc\)'),
          (glyph,r'plVar2\[1\] \* 0x78 \+ \*\(longlong \*\)\(\*plVar2 \+ 0x18\)'),
          (glyph,r'\*\(undefined8 \*\)\(param_3 \+ 0x48\)'),
          (glyph,r'\*\(longlong \*\)\(lVar12 \+ 0x20\) \+ 8'),
          (filter_source,r'FUN_180156950\('),
          (filter_source,r'FUN_180157340\('),
          (category,r'\+ 0x28\)'),(category,r'\+ 0x30\)')]
for block,pattern in patterns:
    if not re.search(pattern,block,re.S):raise ValueError(f'Ownership producer export changed:{pattern}')
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_text(json.dumps(dict(schemaVersion=1,dllSha256=sha,
    bindings={f'{k:x}':f'{v:x}' for k,v in bindings.items()},
    topOwner='TOP273+68 -> firstTOPalternative+18 linked node -> owner+18+index*78 glyph',
    nestedOwner='glyph120+48 -> result service8.virtual08 -> nestedOwner18count/20array',
    warning='Nested glyph alternative label8 is not TOP273 mass8. Glyph materializer and source callback production remain upstream.',
    childPointIndices='Parent retained float buffers submitted as child raw strokes; child original point indices map to parent retained indices',
    filtering='Float height/width compared after conversion to double .02; width compared with double sourceScale*3',
    noVendorExecution=True),indent=2),encoding='utf-8')
print('PASS pinned source720 producer/nested glyph owner/callback bindings and native double flat filter; DLL was not executed.')
