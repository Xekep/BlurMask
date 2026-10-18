"""Static040CD0 pair averaging/remapping; PE reads only."""
from pathlib import Path
import hashlib,json,struct,sys,subprocess
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll');assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions=[(6442715632, 6442716368, 'cfc76889e45a94ca5ad8eaa013f3be32e400b1b734598fb6bf59a3bfab1dc7c5'), (6442716368, 6442717216, '7bf96faf2da58746229242155bddf151d4d9801d50479190cee0912a4802fa07'), (6442721536, 6442723834, '5759f6dc6cc730baa03b865f1ba5a75a659b75a47801ef520f5fa8041d726599')]
pins=[(6442715851, 'inc', 'r8d'), (6442715854, 'movsxd', 'rdx, dword ptr [rdi + rax*4]'), (6442715858, 'mov', 'eax, dword ptr [rbx + rax*4]'), (6442715861, 'mov', 'dword ptr [rcx + rdx*4], eax'), (6442716194, 'movsxd', 'r8, dword ptr [rcx + rax*4]'), (6442716203, 'movsxd', 'rcx, dword ptr [rcx + rax*4]'), (6442716207, 'mov', 'rax, qword ptr [r9]'), (6442716210, 'movsxd', 'rdx, dword ptr [rax + rcx*4]'), (6442716214, 'mov', 'rcx, qword ptr [r10]'), (6442716217, 'mov', 'eax, dword ptr [r15 + r8*4]'), (6442716221, 'mov', 'dword ptr [rcx + rdx*4], eax'), (6442716439, 'mov', 'eax, dword ptr [rdx + 8]'), (6442716528, 'cmp', 'byte ptr [rcx], sil'), (6442716578, 'shr', 'rdi, 1'), (6442716651, 'movss', 'xmm1, dword ptr [rip + 0x1e8e59]'), (6442716717, 'cmp', 'byte ptr [rax + rcx*4 + 3], sil'), (6442716724, 'movss', 'xmm0, dword ptr [r8 + rcx*4]'), (6442716733, 'addss', 'xmm0, dword ptr [r8 + rcx*4]'), (6442716739, 'mulss', 'xmm0, xmm1'), (6442716915, 'call', '0x1800409f0'), (6442716944, 'cmp', 'eax, -1'), (6442716952, 'mov', 'rax, qword ptr [r14]'), (6442716955, 'mov', 'ecx, dword ptr [rax + rcx*4]'), (6442716958, 'mov', 'dword ptr [r8 + rdx*4], ecx'), (6442717028, 'shr', 'r9d, 1'), (6442717057, 'mov', 'r8d, dword ptr [rdx + r9*4]'), (6442717061, 'mov', 'dword ptr [rdx + rcx*4], r8d'), (6442717071, 'mov', 'dword ptr [rax + rcx*4], r8d'), (6442721874, 'lea', 'rdx, [rsp + 0x68]'), (6442721882, 'call', '0x180022df0'), (6442721927, 'mov', 'rax, qword ptr [rsp + 0x68]'), (6442721957, 'mov', 'qword ptr [rsp + 0x58], rax'), (6442722090, 'mov', 'rcx, qword ptr [rsp + 0x58]'), (6442722102, 'mov', 'dword ptr [rcx], r13d'), (6442722163, 'mov', 'qword ptr [rsp + 0x58], rdi'), (6442723026, 'lea', 'rax, [rsp + 0x58]'), (6442723039, 'mov', 'qword ptr [rsp + 0x38], rax'), (6442723047, 'lea', 'rax, [rsp + 0x68]'), (6442723056, 'mov', 'qword ptr [rsp + 0x30], rax'), (6442723061, 'lea', 'rax, [r14 + 0x50]'), (6442723065, 'mov', 'qword ptr [rsp + 0x28], rax'), (6442723070, 'lea', 'rax, [r14 + 8]'), (6442723074, 'mov', 'qword ptr [rsp + 0x20], rax'), (6442723079, 'call', '0x180040cd0')]
instructions={}
for a,z,h in regions:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 instructions.update({i.address:(i.mnemonic,i.op_str) for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,a)})
for a,m,o in pins:assert instructions[a]==(m,o),(hex(a),instructions.get(a))
half=0x180040df3+0x1e8e59
assert b.data[b.off(half):b.off(half)+4]==struct.pack('<f',.5)
out=Path(sys.argv[1]).resolve();out.parent.mkdir(parents=True,exist_ok=True)
dep=out.with_name(out.stem+'-matching.json')
subprocess.run([sys.executable,str(ROOT/'tools/handwriting/audit_math2_column_matching.py'),str(dep)],check=True)
out.write_text(json.dumps(dict(regions=regions,instructionPins=len(pins),matching=json.loads(dep.read_bytes()),
 semantics=['byte3==0 source positions counted then halved; each encountered pair consumes two source columns',
 'separate F32 ADDSS then MULSS .5 per observable row/pair',
 'pairMap[column]=compressedRow; updatedOutput[observableOriginalRow]=fullSourceFirstPairUnit',
 'caller0426F0 binds param7 to observable original-row map;0426F9 binds param6 to separate output units',
 '040F18 reads observableMap[compressedRow]; associations are original output indices, unmatched -1 preserved',
 'backward expansion duplicates each full uint association twice'],
 adaptation='Prepared adjacent complete zero-flag pairs, finite activated probabilities, explicit increasing observable row map and copied outputs; no native allocator prefix claim',
 missing=['actual nonempty C25 producer','full EF4 and owning record join'],vendorExecution=False),indent=2)+'\n',encoding='utf8')
print(f'PASS3 complete correction/caller regions/{len(pins)} pins/half constant plus matching dependency; authenticated caller coordinates')
