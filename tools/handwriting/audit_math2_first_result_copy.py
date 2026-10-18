"""Static first Math2 result copy, pool/range ownership and F0 caller proof.

No DLL is loaded. This does not fill the missing C79 source-range runtime join.
"""
from pathlib import Path
import hashlib,json,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions=[(6442727232, 6442727824, '89737d3d271c8e8e9ab603c73018a50ba5fb6721c393ad6ad8c9102c1adfc5af'),
 (6442728128, 6442728960, 'a33db9e452ccdfc9255c0dfc34053e278b3c0ebf36ef06d17c96b917cbfb9017'),
 (6442608512, 6442609280, '5ec4f960060dfc2702e03e2ed80a71e88ae3e1503b641fcc1a4a54b03f3fd4e0'),
 (6442610432, 6442610672, '1fb26c7308d16446dc794d8a8b1676cc5b5946f93b779263239f8c2d593e3d51'),
 (6442612112, 6442612400, '0cb5a2f52f3ce01e362d82adf732f63caf1d2329ba9073e0ef778d82283a5fee'),
 (6442619536, 6442624064, 'f1c0dbbf545e6b0d33f19eb84c481fc2ed8d47909e4e8f7133fa26a0545f372a'),
 (6442617472, 6442618768, '97200d33a35841e789d9c6fac8dc6b5953a94c02ca456e1dd3963a69469f565d'),
 (6442744912, 6442747312, '7176ded3572da44ea4d47c139eb64fd44af6d8552f45df66c5dbaaa1a357a49b')]
pins={'0x180043791': ('call', '0x180043650'),
 '0x1800437cf': ('mov', 'byte ptr [rdi + 0x20], al'),
 '0x1800437d7': ('mov', 'dword ptr [rdi + 0x24], eax'),
 '0x180043844': ('mov', 'r14d, dword ptr [rdx + rcx*8 + 8]'),
 '0x180043849': ('movss', 'xmm6, dword ptr [rdx + rcx*8 + 0xc]'),
 '0x18004384f': ('test', 'r14d, r14d'),
 '0x180043858': ('test', 'r8, r8'),
 '0x180043861': ('mov', 'rdx, qword ptr [r9 + 0x10]'),
 '0x180043868': ('lea', 'r9, [r8 + r14]'),
 '0x180043874': ('mov', 'r10, qword ptr [rax + 0x10]'),
 '0x180043878': ('call', 'r10'),
 '0x1800438e7': ('mov', 'qword ptr [rdx], rbx'),
 '0x1800438ea': ('mov', 'dword ptr [rdx + 8], esi'),
 '0x1800438ed': ('movss', 'dword ptr [rdx + 0xc], xmm6'),
 '0x1800438f2': ('inc', 'dword ptr [rdi + 0x18]'),
 '0x18004390b': ('mov', 'r9d, dword ptr [r12 + r15*8 + 8]'),
 '0x180043914': ('mov', 'r8, qword ptr [r12 + r15*8]'),
 '0x18004391f': ('mov', 'r10, qword ptr [rax + 0x88]'),
 '0x180043926': ('call', 'r10'),
 '0x180043ae2': ('mov', 'dword ptr [rdx + 0x10], r12d'),
 '0x180043ae9': ('mov', 'dword ptr [rdx + 0x20], r12d'),
 '0x180043af0': ('mov', 'dword ptr [rdx + 0x48], r12d'),
 '0x180043af4': ('mov', 'dword ptr [rdx + 0x58], r12d'),
 '0x180043af8': ('mov', 'dword ptr [rdx + 0x70], r12d'),
 '0x180043b4c': ('mov', 'dword ptr [r15 + 0x80], r12d'),
 '0x180043b5a': ('mov', 'rdx, qword ptr [r15 + 0x88]'),
 '0x180043b69': ('mov', 'r8, qword ptr [rax + 0x38]'),
 '0x180043b74': ('mov', 'edx, 0x3e'),
 '0x180043baa': ('mov', 'qword ptr [r15 + 0x88], rax'),
 '0x180043bc6': ('mov', 'dword ptr [r15 + 0x38], edi'),
 '0x180043bca': ('mov', 'dword ptr [r15 + 0xa0], edi'),
 '0x180043bd4': ('mov', 'dword ptr [r15], ecx'),
 '0x180043bdb': ('mov', 'dword ptr [r15 + 0x60], ecx'),
 '0x180043be4': ('mov', 'r9d, dword ptr [r14 + 0x10]'),
 '0x180043be8': ('lea', 'rdx, [r15 + 8]'),
 '0x180043bf3': ('call', '0x180033de0'),
 '0x180043bfc': ('mov', 'r9d, dword ptr [r14 + 0x20]'),
 '0x180043c00': ('lea', 'rdx, [r15 + 0x18]'),
 '0x180043c0b': ('call', '0x180033de0'),
 '0x180043c14': ('mov', 'rax, qword ptr [r14 + 0x28]'),
 '0x180043c1c': ('mov', 'qword ptr [r15 + 0x28], rax'),
 '0x180043c60': ('call', '0x180033ed0'),
 '0x180043c89': ('mov', 'r9d, dword ptr [r14 + 0x48]'),
 '0x180043c8d': ('lea', 'rdx, [r15 + 0x40]'),
 '0x180043c98': ('call', '0x180033de0'),
 '0x180043ca5': ('mov', 'r9d, dword ptr [r14 + 0x58]'),
 '0x180043ca9': ('lea', 'rdx, [r15 + 0x50]'),
 '0x180043cb4': ('call', '0x180043560'),
 '0x180043cc1': ('mov', 'ebx, dword ptr [r14 + 0x70]'),
 '0x180043cc9': ('mov', 'rdi, qword ptr [r14 + 0x68]'),
 '0x180043cd9': ('mov', 'r8d, 8'),
 '0x180043d2d': ('call', '0x1801ac820'),
 '0x180043d4b': ('lea', 'rdx, [r15 + 0x78]'),
 '0x180043d4f': ('mov', 'r9d, dword ptr [r14 + 0x80]'),
 '0x180043d59': ('mov', 'r8, qword ptr [r14 + 0x78]'),
 '0x180043d5d': ('call', '0x180043740'),
 '0x180043d6c': ('cmp', 'qword ptr [r14 + 0x90], r12'),
 '0x180026f2e': ('mov', 'dword ptr [rdx + 0x20], ebp'),
 '0x180026f31': ('mov', 'dword ptr [rdx + 0x80], 0xffffffff'),
 '0x180026f75': ('add', 'rdx, 0x98'),
 '0x180026f7c': ('call', '0x180026610'),
 '0x1800275ba': ('mov', 'r14, qword ptr [rdx]'),
 '0x1800275c5': ('mov', 'r13, qword ptr [rdx + 8]'),
 '0x18002761f': ('mov', 'r9, r13'),
 '0x180027627': ('mov', 'r8, r14'),
 '0x180027630': ('mov', 'r10, qword ptr [rax + 0x20]'),
 '0x18002763e': ('call', 'r10'),
 '0x180028d24': ('imul', 'rsi, rax, 0xf0'),
 '0x180028d2b': ('mov', 'eax, dword ptr [rsi + r14 + 0xa0]'),
 '0x180028d33': ('mov', 'r13, qword ptr [rsi + r14 + 0x98]'),
 '0x180028e43': ('call', '0x180026780'),
 '0x180047ca8': ('mov', 'qword ptr [rsp + 0x60], r9'),
 '0x18004845e': ('mov', 'rdx, qword ptr [rsp + 0x60]'),
 '0x180048466': ('mov', 'r8, qword ptr [rsp + 0x70]'),
 '0x18004846b': ('add', 'rdx, 0x20'),
 '0x18004846f': ('call', '0x180043ac0'),
 '0x180048474': ('test', 'al, al'),
 '0x18004847b': ('je', '0x18004849a'),
 '0x180026a05': ('mov', 'r10, qword ptr [rax + 0x88]')}
instructions={}
for start,end,digest in regions:
 raw=b.data[b.off(start):b.off(end)]
 assert hashlib.sha256(raw).hexdigest()==digest
 instructions.update({hex(i.address):(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,start)})
for address,wanted in pins.items():assert instructions[address]==tuple(wanted),(address,instructions.get(address),wanted)
assert b.qwords(0x1801fd0d8+0x20,1)==(0x180047c50,)
report=dict(staticOnly=True,vendorExecution=False,
 regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins=pins,
 binding=dict(table='0x1801fd0d8',slot='0x20',target='0x180047c50'),
 firstTarget='027590 passes workitem+8 as EED20 arg4;047C50 saves arg4 at rsp60 and calls043AC0 with target F0+20, first local168 record as source. F0 init026F00 gives record+60 parent=-1.',
 newPool='043AC0 destroys prior row payloads/ranges, releases old Library88, creates new3E Library88;043740 receives vector78, so adjacent vector+10 is this NEW pool. Nonempty UTF8 intervals are re-interned through its V10; scalar bits, flags and metadata are copied separately.',
 ranges='043740 also clones each source row00 pointer/count08 via range V88 AFTER its alternatives.026780 uses the same pool+range pattern for outer aggregation from F0+98/A0. Fresh initializer range-empty does not prove the completed source range empty.',
 boundary='The fresh source-aware library copy now requires the actual registered C79 attachment and clones each typed range after its alternative. This static byte audit does not itself execute or validate the portable runtime. Outer F0 aggregation, optional branches and accepted formula/DOM publication remain separate.',
 failure='Native target may already have reset fields, a new pool and copied row/alternative prefixes when a later service fails. Portable fresh-target poison/disposal is a cleanup adaptation, not generic existing-target rollback or native allocation-order equivalence.')
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(regions)} complete regions/{len(pins)} instructions/EED20 binding; new string-pool AND range copy required; static only')
