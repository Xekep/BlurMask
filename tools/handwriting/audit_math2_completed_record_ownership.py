"""Pinned completed168 constructor/copy/release contract. Static PE reads only."""
import hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll');e=Binary('MyScriptEngine.dll')
assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
regions=[
(0x180037430,0x180037790,'a8ec00faa8c399da401ef220d771754232d4f1d684fb6c1d95a84c7a4330620d'),
(0x180034ea0,0x180035000,'7bf6e00f137ec749e92603c5585719f0aa68a8c6047f82580af21a961948eb3c'),
(0x180034d10,0x180034ea0,'13c99fa9bbedf8763901cd6d22410ee60c8e6a7fe0a750499d410bd850385232'),
(0x180033d60,0x180033de0,'c077406d2fb073573d0ad8cc601e80b723abeef9333452de46af8ac0a097bfe0'),
(0x180033de0,0x180033ed0,'7e43c662cc699e266454d2459d44f90dc4551441425b2d0f35c1cb9505f7afed'),
(0x180026a80,0x180026b10,'159f07c3339f535951c56d626698f7b60adc9b6b0157101fa880e620f6c302e4'),
(0x180035000,0x180035110,'bde2b83414f859683bf2fea706f024d5772c91d7a9e4946169384dcd5c649fd4')]
md=Cs(CS_ARCH_X86,CS_MODE_64);ins={}
for lo,hi,sha in regions:
 raw=b.data[b.off(lo):b.off(hi)];assert hashlib.sha256(raw).hexdigest()==sha
 ins.update((i.address,(i.mnemonic,i.op_str))for i in md.disasm(raw,lo))
pins={
0x180034ed1:('mov','r8d, 0xa8'),0x180034ee8:('call','r10'),
0x180034eee:('je','0x180034fdb'),0x180034efb:('mov','dword ptr [r14 + 0xc], ebx'),
0x180034f05:('imul','rcx, rax, 0xa8'),0x180034f0c:('add','rsi, 8'),
0x180034f20:('mov','dword ptr [rsi - 8], r12d'),0x180034f24:('mov','edx, 0x3e'),
0x180034f29:('mov','dword ptr [rsi + 0x58], 0xffffffff'),
0x180034fa0:('mov','qword ptr [rsi + 0x80], rax'),
0x180034fbf:('je','0x180034fdb'),0x180034fd3:('inc','dword ptr [r14 + 8]'),
0x18003744a:('call','0x180034ea0'),0x180037451:('je','0x180037775'),
0x18003746b:('cmp','qword ptr [rdi + 0x90], 0'),
0x180037498:('mov','qword ptr [r14 + 0x90], rax'),
0x1800374d6:('mov','eax, dword ptr [rcx + 0x28]'),
0x1800374e1:('mov','dword ptr [r14 + 0x60], eax'),
0x1800374fc:('mov','qword ptr [r14 + 0x28], rax'),
0x180037563:('movss','dword ptr [r14], xmm6'),
0x1800375ba:('je','0x180037731'),
0x1800375c0:('mov','dword ptr [r14 + 0x14], ebx'),
0x18003761a:('je','0x180037731'),
0x180037624:('mov','dword ptr [r14 + 0x4c], ebx'),
0x180037656:('call','0x180033d60'),0x18003768d:('inc','dword ptr [r14 + 0x10]'),
0x1800376b2:('call','0x180033d60'),0x1800376e4:('inc','dword ptr [r14 + 0x48]'),
0x180037714:('lea','rdx, [r14 + 0x18]'),0x180037724:('call','0x180033de0'),
0x180037731:('xor','bl, bl'),0x18003773e:('call','0x180026a80'),
0x18003775d:('mov','dword ptr [r14 + 0x38], 0'),
0x180033d9f:('mov','r8d, 2'),0x180033dc2:('mov','dword ptr [rbx + 0xc], edi'),
0x180033e1e:('mov','r8d, 2'),0x180033e8b:('call','0x1801ac820'),
0x180033ea5:('call','0x1801ac460'),0x180033ead:('add','dword ptr [rax], esi'),
0x180034d5f:('call','0x180026a80'),0x180034e41:('mov','rdx, qword ptr [rbp + 0x88]'),
0x180034e54:('call','r8'),0x180034e7a:('mov','rdx, qword ptr [rbp + 0x90]'),
0x180026ae3:('add','rbx, 0x110'),0x180035066:('call','0x180034d10'),
0x180035096:('call','0x180034d10'),0x180035078:('mov','dword ptr [r14 + 8], r15d')}
for address,value in pins.items():assert ins[address]==value,(hex(address),ins.get(address))
bindings=[(0x18023cf80,3),(0x18023cf88,0x40000003e),(0x18023cf98,0x18023d000)]
for address,value in bindings:assert e.qwords(address,1)[0]==value
assert hashlib.sha256(e.data[e.off(0x18023cf80):e.off(0x18023d050)]).hexdigest()=='5124e908a1ea22b5ef37531a608a0673d38d324d311758ce2e1fd378c91cffeb'
# Derive written-byte coverage from the actual constructor stores. Do not zero
# the two padding DWORDs merely because a managed byte[] would start zeroed.
coverage=set();zeroes=[];minus=[];pointer=[]
for address,(op,args)in ins.items():
 if not(0x180034f20<=address<=0x180034fb5)or op!='mov':continue
 import re
 m=re.fullmatch(r'(dword|qword) ptr \[rsi(?: ([+-]) (0x[0-9a-f]+|[0-9]+))?\], (r12d|r12|rax|0xffffffff)',args)
 if not m:continue
 width=4 if m[1]=='dword'else 8;off=8+(int(m[3],0)*(-1 if m[2]=='-'else 1)if m[3]else 0)
 coverage.update(range(off,off+width))
 (pointer if m[4]=='rax'else minus if m[4]=='0xffffffff'else zeroes).append([off,width])
assert set(range(168))-coverage==set(range(4,8))|set(range(100,104))
assert pointer==[[136,8]]and minus==[[96,4]]
report=dict(regions=len(regions)+1,instructionPins=len(pins),bindings=len(bindings),
 constructor=dict(recordBytes=168,zeroStores=zeroes,minusOneStores=minus,owned3EStores=pointer,unwrittenPadding=[[4,4],[100,4]],countIncrementAfter3ESuccess=True),
 completed=dict(prefixU16=[8,16,20],secondaryU16=[24,32,36],auxiliaryU16=[64,72,76],selectedParentI32=96,opaqueQword=40,ownedLibrary3E=136,optionalDiagnostics=144),
 failure='After constructor succeeds the record remains counted when later allocation/copy fails; cleanup only clears nested tensor payload count38, not whole record rollback',
 limits=['static only','3E constructors and pointer identities not emulated','prefix/sidecar producers not joined','no completed-record runtime/acceptance/app changes'],vendorExecution=False)
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(regions)+1} PE regions/{len(pins)} pins/{len(bindings)} bindings; completed168 ownership, failure prefixes and untouched padding')
