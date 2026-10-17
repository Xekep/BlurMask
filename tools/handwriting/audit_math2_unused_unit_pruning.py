"""Static041660 stroke-boundary filtering, actual EF3 and unit removal."""
from pathlib import Path
import hashlib,json,subprocess,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
out=Path(sys.argv[1]).resolve();dep=out.with_suffix('.gate.json')
subprocess.run([sys.executable,'-B',str(ROOT/'tools/handwriting/audit_math2_observable_token_gate.py'),str(dep)],check=True)
b=Binary('MyScriptDocument.dll');start,end=0x180041660,0x180041a30
raw=b.data[b.off(start):b.off(end)];digest=hashlib.sha256(raw).hexdigest()
assert digest=='269bf16800f6bafbe1f7236773a0d6955e82f7b61846ff7d1986d2089d32d2fd'
ins={i.address:(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,start)}
pins=[
 (0x1800416dd,'mov','eax, 0xffffffff'),
 (0x1800416ff,'movsxd','rbx, dword ptr [r14 + rdi*4]'),
 (0x180041703,'cmp','ebx, eax'),
 (0x180041705,'je','0x1800417b5'),
 (0x18004170b,'lea','edx, [r10 + 1]'),
 (0x180041713,'cmp','edx, ecx'),
 (0x180041730,'cmp','qword ptr [r8 + rax*8], rdx'),
 (0x180041746,'test','r10d, r10d'),
 (0x180041760,'cmp','qword ptr [r8 + rax*8], rdx'),
 (0x180041777,'test','r11b, r11b'),
 (0x18004177c,'test','eax, eax'),
 (0x180041787,'mov','eax, ebx'),
 (0x180041794,'mov','dword ptr [r14 + rdi*4], 0xffffffff'),
 (0x1800417ba,'mov','eax, ebx'),
 (0x1800417bc,'inc','dword ptr [r14 + rbx*4]'),
 (0x1800417fc,'cmp','dword ptr [r14 + rdx*4], 1'),
 (0x180041801,'setl','cl'),
 (0x180041841,'cmp','byte ptr [rax + rdi*4 + 2], sil'),
 (0x18004184c,'mov','edx, 0xef3'),
 (0x18004186c,'mov','r9, qword ptr [rax + 0x10]'),
 (0x180041870,'movzx','r8d, word ptr [r8 + rdi*4]'),
 (0x180041875,'call','r9'),
 (0x180041884,'mov','dl, 1'),
 (0x1800418f8,'mov','dword ptr [rdi + rax*4], 0xffffffff'),
 (0x180041920,'dec','ebx'),
 (0x180041967,'call','0x1801ac820'),
 (0x18004196c,'dec','dword ptr [r15 + 8]'),
 (0x180041991,'mov','dword ptr [rdi + rdx*4], esi'),
 (0x180041994,'inc','esi'),
 (0x1800419b3,'cmp','eax, -1'),
 (0x1800419b8,'mov','ecx, dword ptr [rdi + rax*4]'),
 (0x1800419bb,'mov','dword ptr [rdx], ecx'),
]
for a,m,o in pins:assert ins[a]==(m,o),(hex(a),ins.get(a))
out.write_text(json.dumps(dict(regionSha256=digest,regions=1,instructionPins=len(pins),
 filter='Only the first unit-run segment ending a stroke but not beginning a stroke is suppressed; original predecessor remains',
 deletion='Zero use count and (unit byte2 nonzero or actual EF3 observable)',
 scope='Prepared fully assigned positions/non-null fingerprinted gate; no failed allocator prefixes or completed record publication',
 gateDependency=json.loads(dep.read_text())),indent=2)+'\n',encoding='utf8')
print(f'PASS041660 whole region/{len(pins)}pins plus actual EF3 resource/gate; no vendor execution')
