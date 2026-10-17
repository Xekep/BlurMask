"""Positive24C static proof: read PE bytes only, never load/execute SDK."""
from pathlib import Path
import hashlib,json,struct,sys
if len(sys.argv)!=2:raise SystemExit('report.json')
repo=Path(__file__).resolve().parents[2];sys.path.insert(0,str(repo/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64,CS_AC_WRITE
from capstone.x86 import X86_OP_MEM
b=Binary();md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True
regions=[
 (0x180176a10,0x180176fb0,'16899888bda61da733ac6261995cea2973ce6cb3703b404354d0b2f95ad1cea9'),
 (0x180176b15,0x180176f15,'af14ebdbc7ccb85a55b227ed2eec6ecab3395b4a3764a8b7b2812e30d99b1d77'),
 (0x18024dd90,0x18024e3f0,'02af77bbb686f977ddb3d8797d6b9a1b4cd1b8395e67f490aa731a2aa1653306'),
 (0x18024e3f0,0x18024e7a0,'19ebcee590818c3665b701f085fecfbb98fbd07618ad9f5b4edc9da273efe571'),
 (0x18024fe50,0x1802501d0,'00fb13852a612c9bc592cc95f59d9e30c45447feafacf61d1852663284189083')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins={
0x180176b23:('xorps','xmm10, xmm10'),0x180176b30:('xorps','xmm11, xmm11'),
0x180176b3a:('xorps','xmm12, xmm12'),
0x180176bb5:('and','ax, 0xf'),0x180176bbb:('cmp','ax, 6'),
0x180176bc5:('mov','r12d, dword ptr [r13 + 0x28]'),0x180176bc9:('mov','esi, dword ptr [r13 + 0x24]'),
0x180176c0a:('cmp','dword ptr [rax + r13 + 4], -1'),
0x180176c50:('movss','xmm6, dword ptr [rbx]'),0x180176c54:('subss','xmm6, dword ptr [rbx - 8]'),
0x180176c63:('subss','xmm7, xmm0'),0x180176c67:('comiss','xmm12, xmm6'),
0x180176c6d:('xorps','xmm7, xmm14'),0x180176c71:('xorps','xmm6, xmm14'),
0x180176c7b:('cvtss2sd','xmm0, xmm7'),0x180176c7f:('cvtss2sd','xmm1, xmm6'),
0x180176c83:('call','0x18024dd70'),0x180176c88:('comisd','xmm0, xmm13'),
0x180176c9c:('comisd','xmm1, xmm0'),
0x180176ca2:('mulss','xmm6, xmm6'),0x180176ca9:('mulss','xmm7, xmm7'),
0x180176cb0:('addss','xmm6, xmm7'),0x180176cb4:('cvtss2sd','xmm1, xmm6'),
0x180176cbe:('sqrtpd','xmm0, xmm1'),
0x180176ccf:('addsd','xmm10, xmm0'),0x180176cd4:('mulsd','xmm1, xmm8'),
0x180176cd9:('addsd','xmm11, xmm1'),0x180176d23:('ucomisd','xmm10, xmm15'),
0x180176d3c:('movaps','xmm11, xmm15'),0x180176dac:('divsd','xmm11, xmm10'),
0x180176df9:('maxss','xmm8, xmm1'),0x180176e01:('maxss','xmm6, xmm2'),
0x180176e05:('minss','xmm0, xmm9'),0x180176e15:('minss','xmm0, xmm10'),
0x180176e27:('call','0x18024e3f0'),0x180176e2f:('cvtsd2ss','xmm7, xmm0'),
0x180176e37:('call','0x18024fe50'),0x180176e49:('cvtsd2ss','xmm1, xmm0'),
0x180176e4d:('mulss','xmm6, xmm13'),0x180176e52:('mulss','xmm8, xmm13'),
0x180176e6d:('subss','xmm4, xmm0'),0x180176e78:('subss','xmm6, xmm0'),
0x180176e7c:('addss','xmm4, xmm8'),
0x180176ecb:('mulss','xmm2, xmm12'),0x180176ed0:('addss','xmm1, xmm0'),
0x180176ed4:('addss','xmm3, xmm2'),0x180176ed8:('addss','xmm1, xmm4'),
0x180176edc:('addss','xmm3, xmm6'),
0x180176ee0:('movss','dword ptr [rdx], xmm1'),0x180176ee4:('movss','dword ptr [rdx + 4], xmm3')}
result=[]
for a,expected in pins.items():
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==expected
 result.append(dict(address=hex(a),bytes=i.bytes.hex(),instruction=' '.join(expected)))
stores=[]
for i in md.disasm(b.data[b.off(0x180176b15):b.off(0x180176f15)],0x180176b15):
 for op in i.operands:
  if op.type==X86_OP_MEM and op.access&CS_AC_WRITE and i.reg_name(op.mem.base)!='rsp':stores.append(hex(i.address))
assert stores==['0x180176ee0','0x180176ee4'],stores
#Literal constants used by the source matrix are signbit and1/2.
assert struct.unpack_from('<I',b.data,b.off(0x180176b44+9+0x176c53))[0]==0x80000000
assert struct.unpack_from('<f',b.data,b.off(0x180176d4c+9+0x1763eb))[0]==.5
report=dict(staticOnly=True,noVendorExecution=True,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins=result,
 nonStackStores=stores,portableTranscendentalBoundary='Native CPU-dispatched atan2/cos/sin are not represented as universally bit-identical System.Math. Sqrt consumes float squared length widened to double.',
 nextScope='Fresh one ordinary descriptor spans all physical strokes; no events/prior/assignedphysical records. Weighted angle across all physical segments; descriptor-local bbox transform; guide unchanged. Positive24C is not rotation; matrix yx is literal0.')
p=Path(sys.argv[1]);p.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print(f'PASS5 PE ranges/{len(pins)} exactpositive24C pins/exclusiveXYstores/guideunchanged; static only -> {p}')
