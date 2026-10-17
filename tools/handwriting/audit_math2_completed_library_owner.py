"""Static standard Engine creation of completed-record type3E; never loads DLLs."""
import hashlib,json,re,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptEngine.dll')
assert hashlib.sha256(b.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
regions=[
(0x18002ea80,0x18002ea90,'be6d9d73996d45c0425ce37e03965b71f4d99230a62ab753f83345ef5d43b667'),
(0x180034010,0x180034080,'2e866f326c5c48c178795f2a95fec133aa1101ea799f573f9f4bc6bc6fdcbca7'),
(0x1800312c0,0x180031390,'f29603e06887dcb8b7d248dde1f04e20f108d876d177a589bc15b8be61cd6c91'),
(0x180038250,0x180038350,'ef80da4098b256ed260f1cc54dd393c8dd6c12a9ed617a220ecd9021f5d5e1ed'),
(0x180038e70,0x180039130,'6633d0e46fde982f2ba30c98ce4fb144ecffb0d19165ddc49086dde27b01c244')]
md=Cs(CS_ARCH_X86,CS_MODE_64);ins={}
for lo,hi,digest in regions:
 raw=b.data[b.off(lo):b.off(hi)];assert hashlib.sha256(raw).hexdigest()==digest
 ins.update((i.address,(i.mnemonic,i.op_str))for i in md.disasm(raw,lo))
pins={
0x18002ea80:('mov','eax, 0x18'),
0x18003402b:('lea','rax, [rip + 0x21fb1e]'),
0x180034032:('mov','qword ptr [rcx + 8], rax'),
0x180034039:('mov','rax, qword ptr [rip + 0x21dff8]'),
0x180034043:('mov','rdx, qword ptr [rax + 0x20]'),
0x18003404e:('mov','qword ptr [rip + 0x21fb03], rdx'),
0x18003825f:('mov','byte ptr [rsp + 0x20], 0'),
0x18003826c:('call','0x180038e70'),
0x180038282:('mov','r8, qword ptr [rdx]'),
0x180038288:('call','r8'),0x18003828d:('je','0x18003829d'),
0x18003828f:('mov','rax, rbx'),
0x180038e80:('mov','qword ptr [r8], r11'),
0x180038eca:('cmp','dword ptr [rcx], 3'),
0x180038f09:('mov','rcx, qword ptr [rcx + 0x18]'),
0x180038f16:('mov','rcx, qword ptr [rcx + 0x10]'),
0x180038f29:('cmp','byte ptr [rsp + 0x60], r11b'),
0x180038f36:('cmp','qword ptr [rcx], rax'),
0x180038f58:('mov','rax, qword ptr [r14]'),
0x180038f5b:('call','rax'),0x180038f66:('call','0x180039130'),
0x180038f9e:('call','r9'),
0x180038fa9:('mov','rax, qword ptr [r14 + 8]'),
0x180038fad:('mov','qword ptr [r11], rax'),
0x180038fb2:('mov','qword ptr [rdi], r11'),
0x1800312d5:('mov','edx, 4'),0x1800312f1:('call','r8'),
0x1800312fc:('mov','edx, 0x48'),0x180031305:('mov','r8d, 1'),
0x180031314:('call','r9'),
0x180031317:('mov','qword ptr [rdi + 0x10], rax'),
0x18003131e:('jne','0x180031354'),
0x180031324:('mov','edx, 4'),
0x180031334:('mov','r8, qword ptr [rax + 0x28]'),
0x180031344:('call','r8'),0x180031347:('xor','al, al'),
0x18003135b:('mov','qword ptr [rax], rcx'),
0x180031362:('mov','dword ptr [rax + 0x10], ecx'),
0x180031379:('mov','qword ptr [rax + 0x40], rcx')}
for address,value in pins.items():assert ins[address]==value,(hex(address),ins.get(address))
bindings=[(0x180252038,0x1801db8f0),(0x1801db910,0x1801db830),
(0x1801db8b8,0x180038250),(0x18023cf80,3),
(0x18023cf88,0x40000003e),(0x18023cf98,0x18023d000),
(0x18023d000,0x18002ea80),(0x18023d008,0x18023cfc8),
(0x18023d010,0x18023d038),(0x18023d038,0x1800312c0)]
for a,v in bindings:assert b.qwords(a,1)[0]==v,(hex(a),hex(v))
#Constructor initializes semantic pool lanes, not its alignment padding14..17.
covered=set()
for address,(op,args)in ins.items():
 if not(0x18003135b<=address<=0x180031379)or op!='mov':continue
 m=re.fullmatch(r'(dword|qword) ptr \[rax(?: \+ (0x[0-9a-f]+|[0-9]+))?\], (?:rcx|ecx)',args)
 assert m,args
 off=int(m[2],0)if m[2]else 0;width=4 if m[1]=='dword'else 8
 covered.update(range(off,off+width))
assert set(range(72))-covered==set(range(20,24))
report=dict(regions=len(regions),instructionPins=len(pins),bindings=len(bindings),
 standardTypeService='Engine34010 initializes context+8 from252038 descriptor+20; V88=38250',
 creation='38250 ->38E70 validates classkind3; primary+10 holds default/copy constructor table; objectsize fromprimary0; installprimary+8 then invoke default ctor',
 actual3E=dict(size=24,constructor='312C0',poolAllocationBytes=72,emptySemanticFields=True,unwrittenPoolPadding=[20,4]),
 failure='Base4 failure returns false; pool allocation failure invokes Base4 destructor then returns false; generic creation releases unfinished object, not a successful record',
 limits=['Default registered type3E only; no custom pool allocator or descriptor overrides',
 'No runtime Library3E implementation or completed-record join','Dictionary insertion/hash/copy behavior remains separate'],vendorExecution=False)
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(regions)} Engine PE regions/{len(pins)} pins/{len(bindings)} bindings; actual completed Library3E constructor and empty pool')
