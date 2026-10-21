"""Static-only ordinary global118/140 status, path and fresh-container binding."""
import argparse,hashlib,json,sys,struct
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
blocks=[
(0x1801a7f20,0x1801a82a0,'e9b8de67474514784dddbe930efc1ab94ce882611836d350909b6c65295829a1'),
(0x1801a82a0,0x1801a8380,'bf2e65ee6828a470948e16d2aae4768f3fd44114469cbbc6182e5008e23b49de'),
(0x1801a8670,0x1801a8e60,'3f52c407425fe6f4fe3dd13bea3cddeed46f842a86c083645e5101332d1c0f18'),
(0x180195c40,0x1801961b0,'fa91f697ab48d3f550abeda5207596400a454f88f9f81ab060db35d98500cec6'),
(0x1801525e0,0x180152790,'44802b8ea4ae585365cc93bee7fbff94b76f4bbf26453bc57b72f492c62fc8de'),
(0x1801ad890,0x1801ad9d0,'15c5caba20b85234dbf6c13e045040625e3cc8e2403f39fe5c9832630f5a8152'),
(0x180195650,0x180195c40,'986d9e5f8ed5c7f75922cc0bdc60b105af641a9555a1599f38a194af18078762'),
(0x1801aaa60,0x1801aacd0,'9d091b064ea9382aabf8bf750153314bd7199ce78eb5ce85949e51b8494d9e01'),
(0x1801aacd0,0x1801aae60,'0e3c8ed0450580ee20e6d8d6aeac1ab82d15fbfe4662de5f3cfba9d57ea4bace'),
(0x1801acfa0,0x1801ad890,'b3843307b03925ef3957ed27baf2b4cbf4bf99eac975dc1a7a3ad0bbdeec51df'),
(0x180194170,0x1801941b0,'a2c6feb60f91fe349479510bf9ffb121b4c2e9006d1e1d3eb8a2692832a5b93e'),
(0x18000c860,0x18000ca10,'6a054d84178aaea3b74280fad232cc298c5795b7f162b48a125a87921f048ef8'),
(0x18000eba0,0x18000eca0,'abc90fccdf1fd04b4f6ce5f2f4fe105c6130ced6869a0eda96c4613022f7c7ce')]
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);md.detail=True;ins={}
for a,z,h in blocks:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h,hex(a)
 ins.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)})
pins={
0x180194170:('mov','word ptr [r8], 0x101'),0x180194176:('movzx','eax, byte ptr [rdx + 0x50]'),
0x18019417a:('mov','byte ptr [r8 + 2], al'),
0x1801acfae:('cmp','byte ptr [rdx + 0x1dc], 0'),0x1801acfce:('cmp','byte ptr [rdx + 0x191], 0'),
0x1801ad012:('cmp','byte ptr [rdx + 0x192], 0'),0x180195676:('cmp','byte ptr [rdx + 0x210], 0'),
0x180195717:('mov','r9, qword ptr [rcx + 0xd0]'),0x18019576c:('mov','r10, qword ptr [rax + 0x128]'),
0x1801958cd:('mulss','xmm0, dword ptr [rdi + 0x48]'),0x180195a77:('mulss','xmm8, dword ptr [rdi + 0x4c]'),
0x1801aadbd:('movups','xmmword ptr [rcx], xmm0'),
0x1801aade0:('movss','dword ptr [rax], xmm1'),0x1801aadf3:('movss','dword ptr [rax + 4], xmm1'),
0x1801aae03:('movss','dword ptr [rax + 8], xmm0'),0x1801aae13:('movss','dword ptr [rax + 0xc], xmm0'),
0x18000c901:('mov','qword ptr [rdi + 0xd0], rax')}
for address,expected in pins.items():assert ins[address]==expected,(hex(address),ins.get(address),expected)
for slot,target in {0x100:0x1801a7f20,0x108:0x1801a82a0,0x118:0x1801a8670,0x128:0x1801aacd0,0x188:0x1801acf70,0x190:0x1801acfa0,0x198:0x1801ad890}.items():assert b.qwords(0x1802e1ff0+slot,1)[0]==target
assert b.qwords(0x1802e1490+0xa0,1)[0]==0x180195650
f=functions();init=f['1801a6980'];front=f['180150d00'];ctor=f['18000c860'];copy=f['18000eba0']
assert "*(char *)(param_2 + 0x1dc) = param_3[10]"in init
assert "*(undefined8 *)(param_2 + 0xd0) = 0"in ctor
assert "local_b8[0x1a] == 0"in front and "*(char *)(local_b8[0x1a] + 8) != '\\x01'"in front
assert "CONCAT12(uVar6,CONCAT11(uVar5,uVar7))"in front and "+ 0xf5"in front
#Packet starts at local_a8: uStack_a0 low bytes are E1 at8, D0 at9,
#F5/type23F at10 DECIMAL. D0 callback scans do not prove graph1DC's F5 producer.
property_rows=b.data[b.off(0x180274ba0):b.off(0x180274ba0)+52*32]
assert hashlib.sha256(property_rows).hexdigest()=='af88a033bb95e941dca5f41e50dcfb00e50018c010f79e7457fc4005ebd93b38'
callbacks=[]
for row in range(52):
 for address in b.qwords(0x180274ba0+row*32+8,2):
  if not address:continue
  body=[]
  for i in md.disasm(b.data[b.off(address):b.off(address)+1024],address):
   body.append(i)
   if i.mnemonic=='ret':break
  assert body[-1].mnemonic=='ret'
  assert not any(o.type==capstone.CS_OP_MEM and o.mem.disp==0xd0 for i in body for o in i.operands),hex(address)
  callbacks.append(hex(address))
ak=(ROOT/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<2f',ak,189165)==(.5,5.) and ak[189173]==0
assert b.qwords(0x1802e1490+0x78,1)[0]==0x180194170
report=dict(blocks=[dict(start=hex(a),endExclusive=hex(z),sha256=h)for a,z,h in blocks],pins=[f'{a:x} {m} {o}'for a,(m,o)in pins.items()],registeredCallbacks=callbacks,
 freshScope='OPEN producer proof:150D00 packet byte9 is D0-container predicate; byte10 decimal is F5/type23F predicate.1A6980 reads byte10 into1DC. D0 callback scans do not close F5 constructor/copy/property/event mutations; no new numerical conclusion.',
 freshGraph1DcProven=False,
 semantics='Kind6: request04 packed context key; graphD0 signextends24bits after7bit shift; ordered1AAA60 follows compact selector1; first path bbox copied, subsequent stored group boxes expanded. Main191=1 kind6A0 BEFORE192=0. Ordinary0/2/5 pair140 disabled;118 full status/source/descriptor/line and limit branches separately.',vendorExecution=False)
Path(args.output).write_text(json.dumps(report,indent=2)+'\n');print(f'PASS static{len(blocks)} PE blocks/{len(pins)} pins/8dispatch targets/{len(callbacks)} supporting property callbacks; no vendor execution')
