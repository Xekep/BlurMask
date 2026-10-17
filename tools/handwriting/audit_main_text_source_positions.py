"""Static05AFA0 ordinary/insertion source-position branches; no vendor execution."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
b=Binary()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[(6442820880, 6442821040, 'b7bb476c2d0955537b1bab889cdb482716be278ec4bad908641e1dd61986a75c'), (6442823584, 6442825648, '6ca505eecff0b55186b5eaa3a068a760924b0be90596c272e7c73081087c45c9'), (6442823040, 6442823584, '6a3631548a1753bcb1dd1c1f3eb453b64a60047312ac7620cad8703c851ffa02'), (6442794896, 6442795840, 'bc5994707cd91744656553dfbc583bb92542d08698c284bed7c430422dfaa471')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h,hex(a)
pins=[(6442820942, 6442820968, [('ucomiss', 'xmm6, xmm0'), ('jp', '0x18005a572'), ('jne', '0x18005a572'), ('movzx', 'eax, cx'), ('cmp', 'eax, edx'), ('jne', '0x18005a572'), ('mov', 'word ptr [rsp + 0x40], 4'), ('mov', 'rax, qword ptr [rsp + 0x40]')]), (6442820978, 6442821024, [('mulss', 'xmm6, dword ptr [rip + 0x292eb6]'), ('mov', 'byte ptr [rsp + 0x40], 4'), ('addss', 'xmm6, dword ptr [rip + 0x292bb9]'), ('cvttss2si', 'eax, xmm6'), ('mov', 'byte ptr [rsp + 0x41], al'), ('cmp', 'al, 0xc8'), ('jne', '0x18005a5a0'), ('inc', 'cx'), ('mov', 'byte ptr [rsp + 0x41], 0'), ('mov', 'word ptr [rsp + 0x42], cx')]), (6442825471, 6442825502, [('mov', 'r8, qword ptr [r15]'), ('mov', 'edx, edi'), ('add', 'rdx, rdx'), ('movzx', 'eax, byte ptr [r8 + rdx*8 + 9]'), ('cmp', 'al, 0xff'), ('je', '0x18005b76e'), ('cmp', 'word ptr [r8 + rdx*8 + 0xa], 0'), ('jne', '0x18005b71e'), ('test', 'al, al'), ('je', '0x18005b76e')]), (6442825557, 6442825595, [('cmp', 'r11b, sil'), ('je', '0x18005b76e'), ('cmp', 'r11b, 0xff'), ('je', '0x18005b76e'), ('cmp', 'sil, 0xff'), ('je', '0x18005b76e'), ('mov', 'byte ptr [r8 + rdx*8 + 8], 1'), ('jmp', '0x18005b77b'), ('mov', 'rax, qword ptr [r15]'), ('mov', 'ecx, edi'), ('add', 'rcx, rcx'), ('mov', 'byte ptr [rax + rcx*8 + 8], 5')])]
md=Cs(CS_ARCH_X86,CS_MODE_64)
for a,z,expected in pins:
    actual=[(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(a):b.off(z)],a)]
    assert actual==expected,(hex(a),actual)
ink=Binary('MyScriptInk.dll')
assert hashlib.sha256(ink.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
assert ink.qwords(0x1800e5c90+0x20,1)[0]==0x1800e5ac0
assert ink.qwords(0x1800e5ac0+0x80,1)[0]==0x18007fa50
assert ink.qwords(0x1800e5ac0+0xa0,1)[0]==0x18007fae0
ink_regions=[(6442457360, 6442457652, '58798b0b35e7afbba100f94a563237238207634d49cf6070cd2b24f876e07ae7'), (6442973776, 6442973904, '851dbad6bca32a170d95d915f4e6de5c5d4c91a2c4c18f62204da53463fe6cbb'), (6442973600, 6442973776, 'e5ed71f8ea61937a54a7c7e25f498d910df499ca528ccffc29d2f47b764d1acd')]
for a,z,h in ink_regions:assert hashlib.sha256(ink.data[ink.off(a):ink.off(z)]).hexdigest()==h,hex(a)
report=dict(regions=len(regions),exactInstructions=sum(len(p) for _,_,p in pins),
    scope='fresh ordinary source rows; one original closed selection; ordered append, no coalescing',
    insertion='05A510 mode4, terminal fraction0 and float32 quantization',
    normalEnd='original-end match,terminalFF,point0,degenerate=>5; otherwise1',
    notImplemented=['cached/C25 sources','multiple original collection ranges','024F30 source-node interning'])
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(f"PASS{len(regions)} native source-position regions/{report['exactInstructions']} instruction pins; static reads only.")
