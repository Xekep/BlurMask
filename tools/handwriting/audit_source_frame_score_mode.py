"""Read-only pinned source150/property12d007e evidence. No DLL execution."""
import argparse,hashlib,json,re
from pathlib import Path
from audit_legacy_character_properties import PE
p=argparse.ArgumentParser();p.add_argument('--dll',type=Path,default=Path('research/myscript/sdk/build/x64/MyScriptText.dll'));p.add_argument('--decompiled',type=Path,default=Path('research/myscript/output/MyScriptText.dll/decompiled.c'));p.add_argument('--output',type=Path,required=True);args=p.parse_args()
data=args.dll.read_bytes();expected='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
if hashlib.sha256(data).hexdigest()!=expected:raise ValueError('Unknown native evidence fingerprint')
pe=PE(data)
instructions={0x18016a222:'888250010000',0x180169140:'488b8220010000',0x180169147:'0fb68850010000',0x180169160:'410fb600',0x180169164:'488b8a20010000',0x18016916b:'888150010000'}
for a,h in instructions.items():
 if bytes(pe.read(a,f'{len(h)//2}B')).hex()!=h:raise ValueError(f'Native source150 producer/getter/setter changed at{a:x}')
table={0x1802df018:0x1012d007e,0x1802df020:0x180169140,0x1802df028:0x180169160,0x1802de610+0x218:0x180150c00}
for a,v in table.items():
 if pe.read(a,'Q')[0]!=v:raise ValueError(f'Native source150 property table changed at{a:x}')
source=args.decompiled.read_text();m=re.search(r'/\* Address 180150d00;.*?(?=/\* Address |\Z)',source,re.S)
if not m:raise ValueError('Required initial frontend export missing')
ids=re.findall(r'FUN_180150800\(param_1,param_2,(0x[0-9a-f]+)\)',m[0])
if len(ids)!=19 or '0x12d007e' in ids:raise ValueError('Initial property-forwarding scope changed')
args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_text(json.dumps(dict(schemaVersion=1,dllSha256=expected,property='12d007e',constructorSource150=0,getter='169140',setter='169160',initialForwardedProperties=ids,
 scope='Fresh direct source has no property events/clone; reused sources require actual flag150',warning='Generated Ghidra text supplements pinned PE checks and is not executable evidence'),indent=2),encoding='utf-8')
print('PASS pinned constructor/source150 property getter+setter and initial19-property list excluding12d007e; DLL was not executed.')
