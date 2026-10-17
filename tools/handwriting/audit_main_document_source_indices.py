"""Static-only fresh020920 AVL, duplicate membership and page identity proof."""
import argparse,hashlib,json,re,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
b=Binary();ink=Binary('MyScriptInk.dll');md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(ink.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
def dis(s,e,binary=b):return list(md.disasm(binary.data[binary.off(s):binary.off(e)],s))
def pins(s,e,expected):
 rows=[(i.mnemonic,i.op_str)for i in dis(s,e)]
 for pin in expected:assert pin in rows,(hex(s),pin)
 return rows
pins(0x180020920,0x1800209ed,[('mov','rbx, qword ptr [rdx + 0x50]'),('test','dword ptr [rbx + 0x30], 0x200000'),
 ('call','0x1800207a0'),('mov','rbx, qword ptr [rbx + 0x88]'),('mov','rbx, qword ptr [rdi + 0x80]'),
 ('call','0x180020860'),('mov','byte ptr [rdi + 0x1f0], 1')])
body=dis(0x180020920,0x1800209ed)
assert [i.address for i in body if i.mnemonic=='call']==[0x18002095c,0x1800209ac]
assert [i.address for i in body if '0x1f0' in i.op_str]==[0x1800209e0]
for s,e,collection,keyroot in [(0x1800207a0,0x180020854,0x170,0x120),(0x180020860,0x180020914,0x1b0,0x148)]:
 pins(s,e,[('cmp','dword ptr [r8 + 0x38], 0'),('mov','ecx, 0x20'),('mov','esi, 0x30'),('cmove','esi, ecx'),
  ('add',f'rdx, {hex(collection)}'),('lea',f'rdx, [rbx + {hex(keyroot)}]'),('call','0x1800184e0'),('call','0x18001a520'),
  ('mov','r8, qword ptr [rcx + 0x78]'),('mov','qword ptr [rbx + 0x20], rbp'),('mov','qword ptr [rbx + 0x18], rsi')])
members=pins(0x1800184e0,0x1800185a1,[('call','0x180017ef0'),('call','0x180017a40'),('mov','rcx, qword ptr [rbp + 0x38]'),
 ('mov','qword ptr [rax], r12'),('mov','qword ptr [rax + 8], rcx'),('mov','qword ptr [rbp + 0x38], rax'),('mov','byte ptr [r15 + 0xc], 1')])
assert members.index(('mov','qword ptr [rbp + 0x38], rax'))<members.index(('mov','byte ptr [r15 + 0xc], 1'))
pins(0x180017ef0,0x180018190,[('cmp','r11, qword ptr [rax + 0x10]'),('jb','0x180017fdd'),('ja','0x180018003'),
 ('cmp','r10, rcx'),('call','0x180017de0'),('mov','qword ptr [rax + 0x38], rbp'),('mov','byte ptr [rax + 0x20], bpl')])
pins(0x18001a520,0x18001a7e0,[('mov','r10, qword ptr [rax + 0xf8]'),('call','r10'),('je','0x18001a7b4'),('call','0x180019990')])
#Two native rotation functions differ only in balance-byte displacement and
#absolute jump addresses. Compare the whole executable algorithm, not a list
#of interesting instructions or agreement with the managed implementation.
def rotations(s,e):
 rows=[i for i in dis(s,e)if i.mnemonic!='int3'];address={i.address:n for n,i in enumerate(rows)};out=[]
 for i in rows:
  op=i.op_str.replace('+ 0x20]','+ 0x10]')
  if i.mnemonic.startswith('j'):
   target=int(op,16);assert target in address;op='instruction'+str(address[target])
  out.append((i.mnemonic,op))
 return out
rot=rotations(0x180017de0,0x180017ee3);assert rot==rotations(0x180019990,0x180019a93)
for s,e,count,stride in [(0x180017130,0x1800172e0,0x40,6),(0x180017a40,0x180017bf0,0x100,4),(0x18001a0c0,0x18001a24d,0x49,None)]:
 rows=pins(s,e,[('cmp',f'eax, {hex(count)}')])
 assert ('shl',f'rdi, {stride}')in rows if stride else ('imul','rdi, rcx, 0x38')in rows
#Page directories are sorted by native address. Expose portable creation-page
#identity plus slot, never claim those are sorted-directory indices/pointers.
pins(0x180016b30,0x180016c90,[('mov','r8d, 0x1000'),('cmp','r8, 0x1000')])
pins(0x1800173c0,0x1800175a0,[('mov','r8d, 0x1000'),('cmp','rdx, 0x1000')])
pins(0x180019aa0,0x180019c00,[('mov','r8d, 0xff8'),('cmp','r8, 0xff8')])
for s,e in [(0x180016b30,0x180016c90),(0x180019aa0,0x180019c00)]:
 pins(s,e,[('sub','r8, qword ptr [rax + r9]'),('call','0x18025f210'),('mov','dword ptr [r15], ebx')])
pins(0x18001c010,0x18001c2c0,[('xor','ebp, ebp'),('mov','qword ptr [rdi + 0x120], rbp'),('mov','qword ptr [rdi + 0x148], rbp'),
 ('mov','qword ptr [rdi + 0x170], rbp'),('mov','qword ptr [rdi + 0x1b0], rbp'),('mov','word ptr [rdi + 0x1f0], bp')])
assert b.qwords(0x1802754d0+8,1)[0]==0x4000002da
assert b.qwords(0x1802754d0+0x58,1)[0]==0x1802754b0
assert b.qwords(0x1802754b0,1)[0]==0x18001c010
assert b.qwords(0x180275550+0x18,1)[0]==0x1800283b0
assert ink.qwords(0x1800e5ac0+0xf8,1)[0]==0x180081870
assert ink.qwords(0x1800e5ac0+0x78,1)[0]==0x18007f820
assert [(i.mnemonic,i.op_str)for i in dis(0x18007f820,0x18007f828,ink)]==[('mov','dword ptr [rdx + 8], 0'),('ret','')]
regions=[(0x180020920,0x1800209ed),(0x1800207a0,0x180020854),(0x180020860,0x180020914),(0x1800184e0,0x1800185a1),
 (0x180017ef0,0x180018190),(0x180017de0,0x180017ee3),(0x180019990,0x180019a93),(0x18001a520,0x18001a7e0),
 (0x180017130,0x1800172e0),(0x180017a40,0x180017bf0),(0x18001a0c0,0x18001a24d),(0x180016b30,0x180016c90),
 (0x1800173c0,0x1800175a0),(0x180019aa0,0x180019c00),(0x18001c010,0x18001c2c0)]
report=dict(regions=[dict(start=hex(s),end=hex(e),sha256=hashlib.sha256(b.data[b.off(s):b.off(e)]).hexdigest())for s,e in regions],
 rotationInstructions=len(rot),virtualBindings=2,intervalSlots=64,memberSlots=256,vectorSlots=73,
 traversal='CHAR insertion list50/88 thenWORD80/88, skip200000, ready1F0 onlyafterboth',
 duplicates='raw16B membership prepends evenequal; wholevector keyowner18/latestpayload20 replaced; cachecountcleared',
 pages='creation identities+slots only; native directories address-sorted; reuse/freepages unsupported',vendorExecution=False,documentAcceptance=False)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print(f'PASS15 native PE regions/2 C21 bindings/{len(rot)} identical rotation instructions/freshroots/rawduplicates/both-pass ready gate.')
