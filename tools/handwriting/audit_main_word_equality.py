"""Static13F970 linked-WORD equality and fresh2360 identity; PE reads only."""
import hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
start,end=0x18013f970,0x18013fcbb
raw=b.data[b.off(start):b.off(end)];digest=hashlib.sha256(raw).hexdigest()
assert digest=='cdcb84d94322de35cbdbbe1374bc5adc84f2979ae0c1f4a9d42ef77ad0c85448'
md=Cs(CS_ARCH_X86,CS_MODE_64);ins={i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,start)}
pins={
 0x18013f981:('movzx','eax, word ptr [rdx + 0x32]'),0x18013f988:('xor','ax, word ptr [r8 + 0x32]'),
 0x18013f990:('mov','ecx, 0x300'),0x18013f9a3:('test','cx, ax'),0x18013f9a6:('je','0x18013f9b7'),
 0x18013fa14:('mov','r13, qword ptr [rax + 0x2360]'),0x18013fa1b:('movzx','eax, word ptr [rax + 0x24e0]'),
 0x18013facc:('mov','rsi, qword ptr [r14 + 8]'),0x18013fad4:('mov','rdi, qword ptr [rbp + 8]'),
 0x18013fb24:('test','byte ptr [rax + 6], 2'),0x18013fb2a:('mov','rsi, qword ptr [rax + 0x20]'),
 0x18013fb5e:('test','byte ptr [rax + 6], 2'),0x18013fb64:('mov','rdi, qword ptr [rax + 0x20]'),
 0x18013fba9:('mov','r10, qword ptr [rax + 0x58]'),0x18013fbd8:('mov','r10, qword ptr [rax + 0x58]'),
 0x18013fbec:('cmp','byte ptr [rsp + 0xa2], r12b'),0x18013fc0b:('call','r13'),
 0x18013fc25:('test','cl, cl'),0x18013fc3e:('call','r13'),
 0x18013fc57:('cmp','r12w, dx'),0x18013fc66:('cmp','ax, dx'),
 0x18013fc74:('cmp','r12w, ax'),0x18013fc7a:('cmp','byte ptr [rsp + 0xa2], cl')}
for a,v in pins.items():assert ins[a]==v,(hex(a),ins.get(a),v)
assert b.qwords(0x1802dc900+0x60,1)[0]==start
assert b.data[b.off(0x18000c939):b.off(0x18000c943)].hex()=='66c747100000c6471200'
for a,v in [(0x1802149dd,('test','cl, cl')),(0x1802149f1,('mov','qword ptr [rsi + 0x2360], rax')),
 (0x1801f8300,('movzx','eax, dx')),(0x1801f8303,('ret',''))]:
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==v
#Scalar service and descriptor shape are separate from fullV50 emission.
assert '*(byte *)(lVar8 + 6) & 2'in f['18013f970'] and 'local_res1a != local_res12'in f['18013f970']
report=dict(region=[hex(start),hex(end),digest],instructionPins=len(pins)+4,virtualBindings=1,
 scope='13F970 linked equality; sealed fresh no-property2360 identity; not13D360 mutation/fullV68',
 floatReads=False,vendorExecution=False)
Path(sys.argv[1]).write_bytes((json.dumps(report,indent=2)+'\n').encode())
print('PASS full13F970 region/27 instruction pins/276.V60 binding; linked heads, flags300, skip2, scalar alternate and fresh identity.')
