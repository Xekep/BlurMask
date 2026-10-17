"""Prepared segmentation sigmoid036C70 float operation order; static only."""
import hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll');assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
a,z=0x180036c70,0x180036f20;raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()=='7da75f24610dcae0c0123ddbc09e0020bbd8ecded95bedefc17a6c00b22de64c'
md=Cs(CS_ARCH_X86,CS_MODE_64);ins={i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)}
pins=[(6442675384, 'mov', 'qword ptr [rsp + 0x30], r10'), (6442675389, 'mov', 'qword ptr [rsp + 0x28], rdx'), (6442675397, 'mov', 'qword ptr [rsp + 0x20], 2'), (6442675406, 'call', '0x180034320'), (6442675416, 'test', 'ebx, ebx'), (6442675418, 'jne', '0x180036d7a'), (6442675447, 'mov', 'r14, qword ptr [r15]'), (6442675472, 'mov', 'r15, qword ptr [r14]'), (6442675488, 'movss', 'xmm0, dword ptr [r15 + rbx*4]'), (6442675494, 'xorps', 'xmm0, xmm7'), (6442675497, 'call', '0x1801a87c0'), (6442675505, 'addss', 'xmm0, xmm6'), (6442675514, 'movaps', 'xmm1, xmm6'), (6442675517, 'divss', 'xmm1, xmm0'), (6442675539, 'movss', 'dword ptr [rax + rcx*4], xmm1'), (6442675552, 'add', 'r14, 0x110'), (6442675578, 'cmp', 'ebx, 2')]
for a,m,o in pins:assert ins[a]==(m,o),(hex(a),ins.get(a))
assert struct.unpack_from('<I',b.data,b.off(0x180036d02+0x1f310e))[0]==0x80000000
assert struct.unpack_from('<I',b.data,b.off(0x180036d0e+0x1f2f5e))[0]==0x3f800000
#The expf entry takes its argument in lowF32 XMM0 and can dispatch FMA. It
#does not validate the portable MathF.Exp adaptation by executing this DLL.
e=0x1801a87c0;exp=list(md.disasm(b.data[b.off(e):b.off(e+0x50)],e))
assert (exp[2].mnemonic,exp[2].op_str)==('vmovd','edx, xmm0')
report=dict(scope='Prepared sigmoid arithmetic only; vendor EXP CPU dispatch and attention ancestry not ported',regions=1,instructionPins=len(pins),input='Contiguous prepared[symbols,segments]',operations='F32 sign-bit negate;EXP;ADDSS1;DIVSS1',sourceDigest='7da75f24610dcae0c0123ddbc09e0020bbd8ecded95bedefc17a6c00b22de64c')
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS036C70 sigmoid region/'+str(len(pins))+' pins/constants/F32EXPABI; no vendor execution')
