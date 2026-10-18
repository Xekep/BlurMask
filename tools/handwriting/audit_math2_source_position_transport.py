"""Actual Math2 source-position owner through EED -> C79 -> symbol rows; static only."""
from pathlib import Path
import hashlib,json,sys,subprocess
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
out=Path(sys.argv[1]).resolve()
subprocess.run([sys.executable,'-B',str(ROOT/'tools/handwriting/audit_math2_cold_correction_request.py'),str(out.with_suffix('.cold-request.json'))],check=True)
binaries={name:Binary(name)for name in ['MyScriptInk.dll','MyScriptDocument.dll']}
expected={'MyScriptInk.dll':'b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a','MyScriptDocument.dll':'17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'}
for name,b in binaries.items():assert hashlib.sha256(b.data).hexdigest()==expected[name]
REGIONS=[['MyScriptInk.dll', 6442512240, 6442512368, 'f06762c893123eecca3a43036fe4d9b2c4eba61f8e1169700aef62fdb9189b05'],
 ['MyScriptInk.dll', 6442512688, 6442514400, '4d27644e87e3b7005b94adddcd263324f31294690881553a47f418b86ae13750'],
 ['MyScriptInk.dll', 6442514400, 6442515248, '7f5a8dcb5788d3703542586f02936b05cea55583ca9f11e528ee3f19d29c09cb'],
 ['MyScriptInk.dll', 6442515248, 6442517056, '961cf2e64c013635b25f5633cf61aff207727361c93bcd953544dbe0e3167ada'],
 ['MyScriptInk.dll', 6442517056, 6442518128, 'a7b34c448509b5951d3d94834905992b067693a76efdc2473140f3775639f1e5'],
 ['MyScriptInk.dll', 6442518608, 6442519600, 'd8c7ed1e9fb162be57585dad04db0a4f039c0e5f6f20c3c9b414139dd7fd7ccf'],
 ['MyScriptInk.dll', 6442532640, 6442533856, 'ec0cc6ba34d7f75fc83ab23cb323e175a8bc564ca3dab70885d0ecdd273bab98'],
 ['MyScriptInk.dll', 6442535904, 6442536832, 'e8c6bb53bdd4c79190c67f373fabeb88d25675716fa15febdeaa76d0ee9098cb'],
 ['MyScriptInk.dll', 6442545248, 6442550624, 'd0faf82af49ba0caf201aa24e9367f50b6f586728b5115f7f72a48905c22fb83'],
 ['MyScriptInk.dll', 6442577648, 6442579120, 'd9aa998074991ed1727b5f1f0791d0f450b0b8a4e08017666cce02ea06b897a8'],
 ['MyScriptDocument.dll', 6442608512, 6442609280, '5ec4f960060dfc2702e03e2ed80a71e88ae3e1503b641fcc1a4a54b03f3fd4e0'],
 ['MyScriptDocument.dll', 6442617472, 6442618768, '97200d33a35841e789d9c6fac8dc6b5953a94c02ca456e1dd3963a69469f565d'],
 ['MyScriptDocument.dll', 6442740448, 6442741568, '20cbb279671b9a71d1bf4c426e6f2d028f1fcca709eacb979154629d3ff7f59f'],
 ['MyScriptDocument.dll', 6442744912, 6442747312, '7176ded3572da44ea4d47c139eb64fd44af6d8552f45df66c5dbaaa1a357a49b']]
PINS=[('MyScriptDocument.dll', 6442745207, 'mov', 'qword ptr [rsp + 0x58], r14'),
 ('MyScriptDocument.dll', 6442745599, 'mov', 'rdx, qword ptr [r13 + 0x218]'),
 ('MyScriptDocument.dll', 6442745623, 'mov', 'r10, qword ptr [rax + 0x10]'),
 ('MyScriptDocument.dll', 6442745636, 'lea', 'rax, [rsp + 0x58]'),
 ('MyScriptDocument.dll', 6442745646, 'mov', 'qword ptr [rsp + 0x30], rax'),
 ('MyScriptDocument.dll', 6442745661, 'call', 'r10'),
 ('MyScriptDocument.dll', 6442746935, 'mov', 'r9, qword ptr [rsp + 0x58]'),
 ('MyScriptDocument.dll', 6442746956, 'call', '0x180046ae0'),
 ('MyScriptDocument.dll', 6442741154, 'call', '0x180045c60'),
 ('MyScriptDocument.dll', 6442741169, 'test', 'rsi, rsi'),
 ('MyScriptDocument.dll', 6442741172, 'je', '0x180046e8c'),
 ('MyScriptDocument.dll', 6442741185, 'cmp', 'byte ptr [rax + 0x51], 0'),
 ('MyScriptDocument.dll', 6442741189, 'je', '0x180046e8c'),
 ('MyScriptDocument.dll', 6442741235, 'lea', 'rsi, [rax + rdi*8]'),
 ('MyScriptDocument.dll', 6442741281, 'mov', 'r8d, r12d'),
 ('MyScriptDocument.dll', 6442741278, 'mov', 'r9d, r14d'),
 ('MyScriptDocument.dll', 6442741287, 'mov', 'r10, qword ptr [rax + 0x30]'),
 ('MyScriptDocument.dll', 6442741300, 'call', 'r10'),
 ('MyScriptDocument.dll', 6442741307, 'movsxd', 'rax, dword ptr [rsi]'),
 ('MyScriptDocument.dll', 6442741312, 'cmp', 'eax, -1'),
 ('MyScriptDocument.dll', 6442741315, 'jle', '0x180046e77'),
 ('MyScriptDocument.dll', 6442741325, 'mov', 'rax, qword ptr [r15 + 0x78]'),
 ('MyScriptDocument.dll', 6442741333, 'lea', 'rdx, [rax + rcx*8]'),
 ('MyScriptDocument.dll', 6442741352, 'mov', 'r10, qword ptr [rax + 0xb0]'),
 ('MyScriptDocument.dll', 6442741359, 'call', 'r10'),
 ('MyScriptDocument.dll', 6442741369, 'mov', 'r12d, r14d'),
 ('MyScriptInk.dll', 6442536000, 'mov', 'rdi, qword ptr [r15]'),
 ('MyScriptInk.dll', 6442536006, 'jne', '0x180014c7c'),
 ('MyScriptInk.dll', 6442536012, 'mov', 'edx, 0xc79'),
 ('MyScriptInk.dll', 6442536047, 'call', 'rbx'),
 ('MyScriptInk.dll', 6442536055, 'je', '0x180014cb6'),
 ('MyScriptInk.dll', 6442536057, 'mov', 'qword ptr [r15], rax'),
 ('MyScriptInk.dll', 6442536096, 'mov', 'r9, qword ptr [rax + 8]'),
 ('MyScriptInk.dll', 6442536100, 'call', 'r9'),
 ('MyScriptInk.dll', 6442536105, 'je', '0x180014d50'),
 ('MyScriptInk.dll', 6442533766, 'lea', 'r8, [rsi + 0x60]'),
 ('MyScriptInk.dll', 6442533776, 'mov', 'rax, qword ptr [rsi + 0x30]'),
 ('MyScriptInk.dll', 6442533783, 'cmp', 'byte ptr [r9 + rax], 0'),
 ('MyScriptInk.dll', 6442533790, 'add', 'rdi, 2'),
 ('MyScriptInk.dll', 6442533837, 'mov', 'qword ptr [rbx + r9*8], rdi'),
 ('MyScriptInk.dll', 6442550472, 'mov', 'edx, 0xc79'),
 ('MyScriptInk.dll', 6442550495, 'lea', 'r8, [rsp + 0x38]'),
 ('MyScriptInk.dll', 6442550506, 'mov', 'r9, qword ptr [rax + 0x18]'),
 ('MyScriptInk.dll', 6442550510, 'call', 'r9'),
 ('MyScriptInk.dll', 6442578839, 'mov', 'edx, 0xc79'),
 ('MyScriptInk.dll', 6442578878, 'mov', 'r9, qword ptr [rax + 0x20]'),
 ('MyScriptDocument.dll', 6442618148, 'imul', 'rsi, rax, 0xf0'),
 ('MyScriptDocument.dll', 6442618155, 'mov', 'eax, dword ptr [rsi + r14 + 0xa0]'),
 ('MyScriptDocument.dll', 6442618163, 'mov', 'r13, qword ptr [rsi + r14 + 0x98]'),
 ('MyScriptDocument.dll', 6442618398, 'add', 'r9d, -2'),
 ('MyScriptDocument.dll', 6442618402, 'add', 'r8, 0x28'),
 ('MyScriptDocument.dll', 6442618435, 'call', '0x180026780'),
 ('MyScriptDocument.dll', 6442618489, 'call', '0x180026780'),
 ('MyScriptDocument.dll', 6442618579, 'call', '0x180026780'),
 ('MyScriptDocument.dll', 6442608803, 'movzx', 'eax, byte ptr [r12 + r15*8 + 0x20]'),
 ('MyScriptDocument.dll', 6442608809, 'mov', 'byte ptr [rdi + 0x20], al'),
 ('MyScriptDocument.dll', 6442608812, 'mov', 'eax, dword ptr [r12 + r15*8 + 0x24]'),
 ('MyScriptDocument.dll', 6442608817, 'mov', 'dword ptr [rdi + 0x24], eax'),
 ('MyScriptDocument.dll', 6442609157, 'mov', 'r10, qword ptr [rax + 0x88]'),
 ('MyScriptDocument.dll', 6442609164, 'call', 'r10')]
ins={}
for name,a,z,h in REGIONS:
 b=binaries[name];raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 ins.update(((name,i.address),(i.mnemonic,i.op_str))for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,a))
for name,a,m,o in PINS:assert ins[name,a]==(m,o),(name,hex(a),ins.get((name,a)),m,o)
# Registered C79 service and concrete C6D primary, not numerical slot inference.
b=binaries['MyScriptInk.dll']
bindings=[(0x1800dec38,0x400000c79),(0x1800dec50,0x1800dec80),
 (0x1800dec80+8,0x18000f130),(0x1800dec80+0x10,0x18000f7e0),
 (0x1800dec80+0x18,0x18000fb30),(0x1800dec80+0x20,0x180010240),
 (0x1800dec80+0x30,0x180010850),(0x1800ded40,0x1800dece8),
 (0x1800dece8+0x10,0x180014be0),
 (0x1800e5ac0+0x88,0x18007fad0),(0x1800e5ac0+0xb0,0x180080300)]
for address,target in bindings:assert b.qwords(address,1)==(target,),(hex(address),hex(target))
# These values are exact source-owned row formats; equal strides don't merge owners.
d=binaries['MyScriptDocument.dll']
assert d.data[d.off(0x18020d52c):d.off(0x18020d52c)+4]==b'<\\>\0'
report=dict(staticOnly=True,vendorExecution=False,regions=[dict(dll=n,start=hex(a),end=hex(z),sha256=h)for n,a,z,h in REGIONS],
 pins=[dict(dll=n,address=hex(a),instruction=m+' '+o)for n,a,m,o in PINS],bindings=[dict(address=hex(a),value=hex(v))for a,v in bindings],
 conclusions=['Successful047C50 supplies SAME C79 via C6D014BE0 arg7 and046AE0 R9; fresh request.byte4=1 ->EEE51=1 does not skip range attachment.',
 'C79.V18 composes quantized resampling coordinates; V20 gathers whole position segments by actual sorted-parent indices; strk_ends consumes SAME C79 separators.',
 '046AE0 maps previous/current position endpoints, advances previous even for negative item, unions each nonnegative unit range after045C60.',
 '00F7E0 validates InkMapping; it is not a writer of the range cache. Raw processed mapping is not the existing fresh integer-point snapshot.',
 '026780 re-interns symbol alternatives into outerLibrary;028A80 aggregates actualF0 sources before028070/027E20 DOM.'],
 boundary='No runtime source projection, page selection, C25 cache or DOM/name/tag publication implemented by this audit.')
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(REGIONS)} complete regions/{len(PINS)} pins/{len(bindings)} bindings; actual C79 source-owner chain; no vendor execution')
