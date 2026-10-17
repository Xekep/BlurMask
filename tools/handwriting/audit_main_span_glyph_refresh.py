"""Static fresh no-protocol and bounded negative-span217670 source contract."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('output.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=Cs(CS_ARCH_X86,CS_MODE_64);pins=0
regions=[
 (0x180152b20,0x180152b31,'31837281efde555e1d78500f2f0493ce5eaac9d280fc4c6df0ea6170711e6b50'),
 (0x180217784,0x180217834,'30f7f83841345bfa0d1597a4baae618c3a877ced0c81317350431777559c75a2'),
 (0x1801a9ae0,0x1801aa470,'f1c1d83c50d0d8a9dd275272dc40d1b454c5dd5958a624dfca35bf5579c8de55'),
 (0x180168ed0,0x180168f42,'d6b814b89e1ff34cbd4b7180143d48dc01f6bb52b96b3765defad3152ea79a3b'),
 (0x180168f50,0x180168f74,'9482a5d7b3ca8b82ce75342dd393cbebb24a59cff98c4485b455f742f6072a77'),
 (0x18019c970,0x18019cb00,'dee6cc93aacb2b40ddc433bcf1c5f19a3b163936101b9b921a0ae19ae433cddb'),
 (0x18019cb00,0x18019cc50,'ba3f35960628674d64130076f17c4eace44fb8c3aa3e605f5262538482e32cbb'),
 (0x18019cc50,0x18019cd70,'9c9e1de87bc12f69ae208121616c11999e66ba68ba47fd3f6e751b0ed0ca574c'),
 (0x1801a61c0,0x1801a6980,'5601d88e59901a9351625bc133a29baf174528c99f056ac71e1e8e06a47d8a4a'),
 (0x180150d00,0x1801517a0,'243191b3a72e4c3823d7f7f14bb39067ea42adfa6a02a41c6ef5c6193b920927'),
 (0x180151dc0,0x180151fa0,'72ce63d49474f2c6022d50638e502730bbf3c23b60c44df0ad3254c45471e2e0'),
 (0x18019f850,0x1801a1ef0,'9bfc00dfe87b2fc2b879dfe38a939196d1a193cc4e9b522b61eefcc75d5d6201'),
 (0x1801a6980,0x1801a6e70,'f800765cefd5dfd32a841e31153eb786b449c5ad309bb86d82ea228b981f88bc'),
 (0x18019eb10,0x18019ec90,'2dfa304f2a6675703112b969e3b3a9e76872bf10847b696c956adb6a27e62801'),
 (0x1801a1ef0,0x1801a37b0,'41d0f3a2a33b6137e754bcbd478d85b9ddf003868f8529300a3ecd3048a12500'),
 (0x1801a37b0,0x1801a3b30,'c77b55a03c0fb134c3feaaadf07ec7150b6e3ba459c38e5d36d570944c0c7d0c'),
 (0x18019d470,0x18019dc00,'405f6c0a9bc4ba9e30d4dd3e1280da58c715635b1369075b881064ccdcd89acc')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
def exact(a,z,wanted):
 global pins
 actual=[(i.mnemonic,i.op_str)for i in md.disasm(b.data[b.off(a):b.off(z)],a)]
 assert actual==wanted,(hex(a),actual);pins+=len(actual)
exact(0x180152b20,0x180152b31,[('mov','rdx, qword ptr [rdx + 0x70]'),('mov','rax, qword ptr [rdx]'),('mov','r10, qword ptr [rax + 0x120]'),('jmp','r10')])
exact(0x180217784,0x1802177cc,[
 ('mov','eax, edi'),('and','eax, 0xffffff'),('bt','rdi, 0x17'),('lea','ebp, [rax - 0x1000000]'),('cmovae','ebp, eax'),
 ('sar','rdi, 0x38'),('xor','r12d, r12d'),('and','edi, 1'),('mov','qword ptr [rsp + 0x30], r12'),
 ('mov','r8d, ebp'),('mov','qword ptr [rsp + 0x28], r14'),('lea','r15d, [rdx + rbp]'),('mov','dword ptr [rsp + 0x20], edi'),
 ('mov','rdx, qword ptr [rbx + 0x20]'),('mov','r9d, r15d'),('mov','rax, qword ptr [rdx]'),('mov','r10, qword ptr [rax + 0x188]'),('call','r10')])
exact(0x1802177cc,0x1802177fd,[
 ('mov','rax, qword ptr [rsi + 8]'),('mov','edx, 0x18'),('mov','r13, qword ptr [rsp + 0x88]'),('and','rax, 0xfffffffffffffffe'),
 ('mov','rcx, rsi'),('mov','rax, qword ptr [rax]'),('mov','r8, qword ptr [r13 + 0x30]'),('mov','r10, qword ptr [rax]'),
 ('call','r10'),('mov','qword ptr [r14], rax'),('test','rax, rax'),('je','0x180217d73')])
exact(0x1802177fd,0x180217834,[
 ('mov','rdx, qword ptr [rbx + 0x20]'),('mov','r9d, r15d'),('mov','qword ptr [rsp + 0x30], r12'),('mov','r8d, ebp'),
 ('mov','qword ptr [rsp + 0x28], r14'),('mov','rcx, rsi'),('mov','dword ptr [rsp + 0x20], edi'),('mov','rax, qword ptr [rdx]'),
 ('mov','r10, qword ptr [rax + 0x188]'),('call','r10'),('mov','al, 1'),('mov','dword ptr [r13 + 0x1c], 0x3f800000'),('jmp','0x180217d75')])
exact(0x180168f50,0x180168f74,[
 ('mov','r9, qword ptr [rdx + 0x120]'),('mov','ecx, r8d'),('mov','rax, qword ptr [r9 + 0xe8]'),('mov','ecx, dword ptr [rax + rcx*4]'),
 ('mov','rax, qword ptr [r9 + 0xf8]'),('lea','rdx, [rcx + rcx*2]'),('lea','rax, [rax + rdx*4]'),('ret','')])
exact(0x18019ca20,0x18019ca41,[('mov','qword ptr [rbx + 0x200], rcx'),('mov','qword ptr [rbx + 0x208], rcx'),
 ('mov','dword ptr [rbx + 0x1f0], ecx'),('mov','word ptr [rbx + 0x1f4], cx'),('mov','dword ptr [rbx + 0x1f8], ecx')])
exact(0x18019cb18,0x18019cb2e,[('call','0x18019c970'),('movzx','r8d, al'),('lea','r10, [rbx + 8]'),('lea','r9, [rdi + 8]'),('mov','edx, 2')])
exact(0x18019cc68,0x18019cc88,[('cmp','r9, 0x158'),('je','0x18019cc83'),('xor','al, al'),
 ('mov','rbx, qword ptr [rsp + 0x30]'),('mov','rsi, qword ptr [rsp + 0x38]'),('add','rsp, 0x20'),('pop','rdi'),('ret',''),('call','0x18019c970')])
exact(0x18019f92d,0x18019f93d,[('movzx','edx, word ptr [rax]'),('movzx','ecx, dx'),('and','cx, 0xf'),('je','0x18019fc3c')])
exact(0x180151efd,0x180151f38,[('mov','rax, qword ptr [r14]'),('mov','r8d, 0xa4'),('mov','rbx, qword ptr [rsi + 0x70]'),
 ('and','rax, 0xfffffffffffffffe'),('mov','rdx, rbx'),('mov','rcx, rdi'),('mov','rax, qword ptr [rax + 0x10]'),('mov','r9, qword ptr [rax + 0x30]'),
 ('call','r9'),('xor','r9d, r9d'),('xor','r8d, r8d'),('mov','rdx, rbx'),('mov','rcx, rdi'),('mov','r10, qword ptr [rax]'),('call','r10'),
 ('test','al, al'),('setne','al'),('jmp','0x180151f3e')])
exact(0x18019eb2e,0x18019eb5f,[('cmp','byte ptr [r9 + 1], 0'),('mov','rbx, r9'),('mov','r15, r8'),('mov','rsi, rdx'),('mov','r14, rcx'),
 ('jne','0x18019eb5f'),('cmp','byte ptr [r9], 0'),('jne','0x18019eb5f'),('cmp','byte ptr [r9 + 2], 0'),('jne','0x18019eb5f'),
 ('cmp','byte ptr [r9 + 3], 0'),('jne','0x18019eb5f'),('mov','eax, 0xffffffff'),('jmp','0x18019ec62')])
bindings=[(0x1802de610,0x188,0x180152b20),(0x1802e1ff0,0x120,0x1801a9ae0),
 (0x1802df340,0xa8,0x180168ed0),(0x1802df340,0xb0,0x180168f50)]
for table,slot,target in bindings:assert b.qwords(table+slot,1)[0]==target
descriptor,words=b.descriptor(0x21b)
assert descriptor==0x1802e1d40 and words[11]==0x1802e1e00
assert b.qwords(words[11],3)==(0x18019c970,0x18019cb00,0x18019cc50)
assert b.qwords(0x1802e1f10,1)[0]==0x1801a61c0
result=dict(regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],exactInstructions=pins,bindings=len(bindings),
 branch='negative signed arc; low24 last context; first=last+signedarc; mode=bit56',allocationStride=24,
 residualOutput='null in both source calls; no native zero-byte substitute',ownerScalarBits='3f800000 after fill',
 freshScope=['actual raw-result private factory, one descriptor kind0, no external property/reuse',
 '150D00 clone selected247.V90 -> frontend70 -> V10=1A6980',
 '151DC0 empty-graph -> graph A4=1A61C0 -> ordinary19F850; not kind2/6 producers',
 '19C970 clears pending flags/tokens;19CB00 and19CC50 reset then copy only8..160',
 '19EB10 all four gate bytes0 -> FFFF;1A1EF0 copiesgroup4 -> arc18;1A37B0 carriesarc18',
 'one descriptor excludes any preceding kind3 pending event'],
 remaining=['general protocol contexts/multidescriptor source','native allocator-failure replay','full string-wrapper integration'])
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(result,indent=2)+'\n').encode())
print(f'PASS{len(regions)} regions/{pins} exact instructions/4bindings/21B constructorregistration; fresh ordinary negative-span scope only.')
