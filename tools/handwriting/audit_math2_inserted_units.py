"""Static finite041240 insertion control; no vendor execution."""
from pathlib import Path
import hashlib,json,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll');assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
a,z=0x180041240,0x180041660;raw=b.data[b.off(a):b.off(z)]
assert hashlib.sha256(raw).hexdigest()=='a05eaf260d8cba544af148a65ed157ec81db6790af15008fae9416209de8d9df'
ins={i.address:(i.mnemonic,i.op_str) for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,a)}
pins=[(6442717833, 'call', '0x1800401b0'), (6442717919, 'cmp', 'dword ptr [r8 + r13*4], -1'), (6442717924, 'jne', '0x18004160d'), (6442717951, 'mov', 'ebp, dword ptr [r8 + rax*4]'), (6442717955, 'inc', 'ebp'), (6442717966, 'mov', 'ebx, dword ptr [rbx + r13*4]'), (6442717972, 'shr', 'eax, 0x18'), (6442717977, 'jne', '0x180041347'), (6442717988, 'cmp', 'eax, ebp'), (6442717990, 'jl', '0x18004132c'), (6442718010, 'mov', 'dword ptr [r8 + r13*4], ebp'), (6442718014, 'mov', 'dword ptr [r8 + rax*4], ebp'), (6442718031, 'test', 'r14b, r14b'), (6442718034, 'je', '0x1800414b6'), (6442718098, 'cmp', 'byte ptr [rdx - 4], 0'), (6442718127, 'addss', 'xmm0, dword ptr [rax + rcx*4]'), (6442718161, 'addss', 'xmm0, dword ptr [rax + rcx*4]'), (6442718195, 'addss', 'xmm0, dword ptr [rax + rcx*4]'), (6442718230, 'addss', 'xmm0, dword ptr [rax + rcx*4]'), (6442718308, 'addss', 'xmm0, dword ptr [rax + rcx*4]'), (6442718325, 'comiss', 'xmm0, xmm1'), (6442718328, 'jbe', '0x180041480'), (6442718333, 'mov', 'r14, r10'), (6442718370, 'movsxd', 'rcx, dword ptr [rax + r14*4]'), (6442718374, 'mov', 'ebx, dword ptr [rbx + rcx*4]'), (6442718390, 'movzx', 'eax, word ptr [rsp + 0xb0]'), (6442718414, 'mov', 'word ptr [rsp + 0xba], 0x100'), (6442718440, 'cmp', 'eax, ebp'), (6442718446, 'mov', 'dword ptr [rcx], eax'), (6442718450, 'cmp', 'eax, -1'), (6442718455, 'mov', 'dword ptr [rcx], ebp'), (6442718494, 'call', '0x180022df0'), (6442718550, 'call', '0x1801ac820'), (6442718608, 'call', '0x180040130'), (6442718673, 'call', '0x1801ac820'), (6442718700, 'mov', 'byte ptr [r8], 1')]
for address,m,o in pins:assert ins[address]==(m,o),(hex(address),ins.get(address))
out=Path(sys.argv[1]).resolve();out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(wholeRegion=[a,z,hashlib.sha256(raw).hexdigest()],instructionPins=len(pins),
 semantics=['0401B0 initializes mask before scanning assignments',
 'absent cursor uses previous assignment plus1 or0 at beginning',
 'source byte3==0 keeps full uint, shifts existing assigned indices and sets adjacent assignments to insertion position',
 'other source branch accumulates F32 probabilities over ALL nonzero-byte3 columns, first strongest observable row',
 'selected current output read uses observable original row; fallback upper word0100 plus replacement U16',
 'nonpair branch fills ALL remaining -1 assignments, including nonadjacent positions',
 'unit inserted before mask; inserted mask byte1, prior mask bytes preserved'],
 adaptation='Prepared well-formed complete absent source pairs, copied payloads, conservative1000-unit upper bound; no allocator-failure prefix claim',
 missing=['actual source/request provenance','041EC0 auxiliary data','owning general EF4 integration'],vendorExecution=False),indent=2)+'\n',encoding='utf8')
print(f'PASS complete041240 region/{len(pins)} instruction pins; insertion and initial-mask semantics only')
