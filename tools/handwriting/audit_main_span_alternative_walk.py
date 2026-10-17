"""Static218A30 direct-parent and219580->218560 argument proof; PE reads only."""
import hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x180218a30,0x180219580;raw=b.data[b.off(a):b.off(z)];h=hashlib.sha256(raw).hexdigest();assert h=='3b44806a3c8ecabd8e2ea03be74e438dfc12d278c579aca3c9f18598e135a8ee'
walk=f['180218a30'];pins=[
 'local_170 = 0xffffffffffffffff','local_1d0 = *(uint *)(lVar9 + 0x10 + lVar14 * 0x28)',
 '*local_1b8 = local_1b0','*local_c0 = local_170','*local_b8 = local_1d0',
 'local_1d4 = local_1d4 & 0xff','local_1d4 = 0xff00','local_1e4 & 0x10',
 'local_1c0 = uVar6','local_1c8 = local_190 & 0xffff','local_e8 = FUN_180216610',
 'local_170 = (uVar11 & 0xffff) << 0x18 | (ulonglong)(uVar6 & 0xffffff)',
 'if (param_8 ==','if ((int)uVar6 < (int)*local_1b8)','iVar12 = 4','iVar12 = 1','iVar12 = 2','iVar12 = 3']
for p in pins:assert p in walk,p
assert '(param_2 + 0x2600) = 0x2000000'in f['18020baa0']
assert '(param_2 + 0x98) = 0'in f['18000c860']and '(param_2 + 0x8c) = 0xff'in f['18000c860']
#The fresh no-property factory bypasses both2601 mutation paths: C4=false,
#and Knowledge8D (high byte of8C) zero. No mutable/reused owner is admitted.
assert '(lVar3 + 0x8d)'in f['1801eccb0']and '(lVar3 + 0xc4)'in f['1801eccb0']
assert 'if (*(char *)(*(longlong *)(param_2 + 0x18) + 0xc4)'in f['18020d8f0']
md=Cs(CS_ARCH_X86,CS_MODE_64)
def ins(addr):
 i=next(md.disasm(b.data[b.off(addr):b.off(addr)+16],addr));return(i.mnemonic,i.op_str)
instructions={
 0x180219b1a:('mov','byte ptr [rsp + 0x48], 0'), #p10 initialForceDuplicate
 0x180219b23:('mov','byte ptr [rsp + 0x40], 1'), #p9 conversionArgument
 0x180219b33:('mov','qword ptr [rsp + 0x38], rax'), #p8 first NEW record output
 0x180219b3b:('mov','dword ptr [rsp + 0x30], 0xffffffff'),
 0x180219b46:('mov','dword ptr [rsp + 0x28], 0xffffffff'),
 0x180219b4e:('mov','word ptr [rsp + 0x20], 0xffff'),
 0x180219b55:('call','0x180218560'),
 0x180152ae7:('mov','r9, qword ptr [rax + 0xd0]'),
 0x1801a7cf0:('sar','r8d, 7'),0x1801a7cfd:('bt','r8d, 0x17')}
for address,expected in instructions.items():assert ins(address)==expected,(hex(address),ins(address))
assert b.qwords(0x1802de610+0x178,1)[0]==0x180152ae0
assert b.qwords(0x1802e1ff0+0xd0,1)[0]==0x1801a7cf0
#Output first-new is stored only after successful finalization of a newly
#allocated record. Reused rows and a failed finalizer do not set that output.
chain=f['180218560'];p=chain.index('if ((!bVar3) && (param_8')
assert chain.rfind('FUN_1802173e0',0,p)>chain.rfind("if (local_res8[0] == '\\0')",0,p)
assert '*param_8 = uVar11' in chain[p:p+200]
report=dict(region=[hex(a),hex(z),h],semanticPins=len(pins),instructionPins=len(instructions),bindings=2,
 scope='Fresh one-WORD ordinary218A30 numeric inspect/emit only; explicit scratch, no actual219580candidate claimed',
 connection=dict(previous=65535,topAcBits='FFFFFFFF',topB0Bits='FFFFFFFF',conversion=1,initialForceDuplicate=False),vendorExecution=False)
Path(sys.argv[1]).write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS full218A30 PE/18semanticpins/10instructionpins/2bindings; directparents, normaloutputwrites and exact218560arguments.')
