"""Static pinned-PE evidence for206D30; never loads vendor code."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions()['180206d30']
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
fragments=[
 'FUN_1801fb940(param_1,param_2,*param_17 + *(int *)(param_2 + 0x2470))',
 '*(undefined8 *)(lVar11 + uVar18 * 0x28) = param_8;',
 '*(int *)(lVar11 + 0x10 + uVar18 * 0x28) = param_3;',
 '*(undefined4 *)(lVar11 + 0x24) = 0xffffffff;',
 'piVar16[2] = *(int *)(param_2 + 0x2470);',
 'param_3 = piVar16[2];',
 '*(undefined4 *)((longlong)puVar14 + 0x24) = param_12;',
 '*piVar16 = iVar10;',
 '*(int *)(param_2 + 0x2470) = *(int *)(param_2 + 0x2470) + 1;'
]
for fragment in fragments:assert fragment in f,fragment
#Machine code independently pins signed/unsigned comparisons, lazy lookup,
#intermediate upper-bit clearing and final flag publication.
pins={
 0x206e03:'410fb641202407',
 0x206e15:'4180e207664189511e4432d041c74124ffffffff',
 0x206e30:'4588512041894114',
 0x206ec3:'663bb71c2500000f84a0000000663bb71e2500000f8493000000',
 0x206eff:'0fb74411183b4424787407807c1105007c5c',
 0x206f11:'663bb7e22400007453663bb7dc240000744a',
 0x206f27:'663b700e7440b8ebff0000663bf07336',
 0x206f61:'0fb644011a243f3a043275034532e4',
 0x206fad:'c0e004c0e1034132492080e1084989194132492080e10f',
 0x206fc8:'32c8664189711c0fb68424c000000080e11f02c049895108440ae0',
 0x207001:'41c0e406410acc41894118',
 0x207014:'4432c14c8b6424204180e0074432c1664189411e',
 0x20702f:'458841204189412480bf68230000000f84a8030000',
}
for rva,expected in pins.items():
 data=bytes.fromhex(expected);assert b.data[b.off(0x180000000+rva):b.off(0x180000000+rva)+len(data)]==data,hex(rva)
report=dict(decompilerBindings=len(fragments),assemblyPins=len(pins),
 scope='prepared206D30, detailed2368=false; no207CB0 sequence producer/backtrack/allocator execution',
 preserved='whole40-byte rows including untouched22/23; no host case/category guesses')
if len(sys.argv)!=2:raise ValueError('output.json required')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
p.write_bytes((json.dumps(report,indent=2)+'\n').encode('ascii'))
print(f'PASS {len(fragments)} static bindings/{len(pins)} exact assembly pins; vendor code not executed')
