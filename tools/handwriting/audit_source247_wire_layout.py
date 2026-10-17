"""Static native reader/field proof for247 layout and bounded297 profile.
Delegated envelopes are not represented as decoded processor/NN payloads.
"""
from pathlib import Path
import hashlib,json,sys
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();f=functions();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[
 (0x1801d2680,0x1801d28c0,'ca21bc34a5572aedaf376ac733f707b7aa05ee7b3a236d0bee0dfb3f958dcd9d'),
 (0x1801d3210,0x1801d3550,'5792fc2cbbacca92a6448c019e92d4c6d3a3e4d5e198529d0bb62e1e3707b798'),
 (0x1801d3f00,0x1801d41e0,'bc94055ff99eb7e63d0ac1c70c7c91d73c5e529135bf066245201ea4c13042af'),
 (0x1801d41e0,0x1801d47c0,'de4302906e8e10cca30b22bd09e08ca8cb85f42cc2feeb2a9d9bf3fc97d36427'),
 (0x1801d47c0,0x1801d4950,'141f6bede99fa6aab460eedffc0749bb96a1cc748523cec001228645f6b6bc71'),
 (0x1801d4950,0x1801d51e0,'b228844e9a7159567052ffc453e3b1df1fdc61c03d54e31d70e280627ab807c9'),
 (0x180196500,0x180196f40,'d70df666938d2f080c39a0e0c2650668ac688cfae7c0c357083bc6609dc8dd83')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h,hex(a)
pins={
0x1801d26e8:('mov','qword ptr [rsp + 0x20], 0x38'),
0x1801d2738:('lea','r8, [rbp + 0x4c]'),0x1801d276a:('lea','r8, [rbp + 0x50]'),
0x1801d33e0:('mov','eax, 0x300'),0x1801d33e5:('cmp','bx, ax'),
0x1801d3420:('movzx','esi, byte ptr [rsp + 0x78]'),
0x1801d3425:('mov','eax, dword ptr [rdi + 0x70]'),
0x1801d343c:('mov','rdx, qword ptr [rdi + 0x68]'),
0x1801d3f7e:('mov','r9d, 4'),0x1801d3fa9:('lea','r8, [rbx + 0x10]'),
0x1801d4001:('lea','r8, [rbx + 0x18]'),
0x1801d4224:('lea','r8, [rdi + 0x10]'),0x1801d4271:('lea','r8, [rdi + 0x18]'),
0x1801d45db:('mov','qword ptr [rdi + 0x38], rax'),
0x1801d46da:('lea','r8, [rdi + 0x50]'),0x1801d4719:('lea','r8, [rdi + 0x60]'),
0x1801d4ac6:('mov','eax, 0x10f'),0x1801d4ace:('jae','0x1801d4af2'),
0x1801d4c07:('mov','eax, 0x10e'),0x1801d4d6a:('mov','qword ptr [rdi + 0x140], rax'),
0x1801d4d9c:('cmp','bp, cx'),0x1801d4d9f:('jb','0x1801d4dd3'),
0x1801d4df7:('mov','qword ptr [rdi + 0x148], rax'),
0x180196607:('mov','eax, 0x106'),0x180196901:('call','0x180010d80'),
0x180196d29:('mov','eax, 0x107'),0x180196d3c:('lea','r8, [rbx + 0x134]'),
0x180196d78:('lea','r8, [rbx + 0x128]'),0x180196e66:('mov','eax, 0x108'),
0x180196ed0:('mov','qword ptr [rbx + 0x1d0], rax'),0x180196ed7:('lea','r8, [rbx + 0x1a0]'),
}
for a,v in pins.items():
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==v,(hex(a),i.mnemonic,i.op_str)
terms={
'1801d2680':['local_68 [56]','local_88[0] == -1','param_2 + 0x4c','param_2 + 0x50'],
'1801d3210':['param_4 < 0x300','param_2 + 0x70','param_2 + 0x68','bVar6 < local_res20[0]'],
'1801d3f00':['param_2 + 0x18','param_2 + 0x19','param_2 + 0x1a','param_2 + 0x24','param_2 + 0x25','param_2 + 0x26','param_2 + 0x27'],
'1801d41e0':['0x10c < param_5','param_2[5] = lVar5','param_2[6] = lVar5','param_2[7] = lVar5','param_2 + 0xc'],
'1801d47c0':['0x107 < param_4','+ 0x28) + 0x18'],
'1801d4950':['param_4 < 0x10f','param_4 < 0x10e','0x10f < param_4','param_2 + 0x150','param_2 + 0x148','param_2 + 0x214','param_2 + 0x220'],
'180196500':['param_4 < 0x107','0x107 < param_4','param_2 + 0x134','param_2 + 0x1d0','param_2 + 0x1a0']}
for k,ss in terms.items():assert all(s in ' '.join(f[k].split()) for s in ss),(k,ss)
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
report=dict(noVendorExecution=True,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],
 pins=[dict(address=hex(a),instruction=' '.join(v))for a,v in pins.items()],decompiledTerms=terms,
 scope='Parent247/10D/110 wire fields and delegated child envelopes only; complete297/106 or108 requires actual category andfalse134/1D0 payload flags. Positive payloads,21D/119 weights and fullEnglish recognizer remain unsupported.')
p.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print(f'PASS7 complete reader ranges/{len(pins)} exact instructions/7 field-order checks; static PE only')
