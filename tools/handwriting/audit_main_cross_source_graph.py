"""Static PE only: ordinary source cross-merge/boundary-pruning ownership."""
import argparse,hashlib,json,pathlib,struct,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
blocks=[(0x18019c970,0x18019cb00,'dee6cc93aacb2b40ddc433bcf1c5f19a3b163936101b9b921a0ae19ae433cddb'),
 (0x18019cb00,0x18019cc50,'ba3f35960628674d64130076f17c4eace44fb8c3aa3e605f5262538482e32cbb'),
 (0x18019e8b0,0x18019eb10,'d5db4c7a1a16ce5d9186bd995225075e982f426c54ee5efef825454f1265c74f'),
 (0x1801a3b30,0x1801a4582,'c084906778182c19048aee4ab7aab42f1acd175484cfa8d412e15b346f331a29'),
 (0x1801a5b00,0x1801a61c0,'080fcf860b329bc36acc46680c17552da89cdfe95113f9636af4367ffe7b8972'),
 (0x180193860,0x180194140,'a913702c074d72d5f406d29510bff732edf0c53b04aab397e923576e587b8d6b'),
 (0x180194140,0x18019416c,'90cd1e529e3df7582ecda89d60edecbe765e619d35c77a6fd8108b13564b2aff'),
 (0x1801ad9d0,0x1801adf40,'759eabfb9f8e195e2c60f609bc4014f39168b3703f8161b361ce559f3809e045')]
instructions={}
for start,end,h in blocks:
 raw=b.data[b.off(start):b.off(end)];assert hashlib.sha256(raw).hexdigest()==h
 ins=list(md.disasm(raw,start));assert ins[-1].address+ins[-1].size==end
 instructions.update({v.address:(v.mnemonic,v.op_str) for v in ins})
pins={
0x18019ca10:('mov','dword ptr [rbx + 0x1d9], ecx'),0x18019caec:('mov','dword ptr [rbx + 0x2d0], ecx'),
0x18019cb18:('call','0x18019c970'),0x1801add85:('lea','rcx, [rbx + 0x158]'),
0x18019e93b:('call','0x18019d470'),0x18019e94f:('cmp','byte ptr [rdi + 0x1db], 0'),
0x18019e96d:('sub','ecx, r12d'),0x18019e970:('add','ecx, esi'),0x18019e974:('jge','0x18019e9a5'),
0x18019e976:('mov','byte ptr [rsp + 0x38], 1'),0x18019e982:('mov','qword ptr [rsp + 0x30], 0'),
0x18019e98b:('mov','r9d, r12d'),0x18019e98e:('mov','dword ptr [rsp + 0x28], edx'),0x18019e998:('mov','dword ptr [rsp + 0x20], esi'),
0x18019e99c:('call','0x1801a3b30'),0x18019e9ab:('mov','esi, dword ptr [r15]'),0x18019e9ae:('dec','esi'),
0x18019ea0c:('add','ecx, r15d'),0x18019ea11:('cmovge','ebp, ecx'),
0x18019ea23:('mov','byte ptr [rsp + 0x20], 1'),0x18019ea35:('call','0x1801a5b00'),
0x18019ea53:('mov','byte ptr [rsp + 0x20], 0'),0x18019ea61:('add','r9d, r15d'),0x18019ea64:('call','0x1801a5b00'),
0x18019ea97:('cmp','byte ptr [rdi + 0x2d3], 0'),
0x1801a3d22:('test','byte ptr [r8 + rdx + 6], 8'),0x1801a3d43:('je','0x1801a4326'),
0x1801a3dc6:('cmp','byte ptr [rcx + 0x158], 0'),0x1801a3df3:('jne','0x1801a4326'),
0x1801a3dfd:('cmp','ecx, 0x80000000'),0x1801a3e12:('jne','0x1801a4326'),
0x1801a3e2a:('sub','eax, ecx'),0x1801a3e2c:('cmp','eax, 1'),0x1801a3e2f:('jg','0x1801a4326'),
0x1801a3fa7:('movss','xmm1, dword ptr [rax + 0xc]'),0x1801a3fac:('subss','xmm1, dword ptr [rbx + 0xc]'),
0x1801a41c7:('inc','rcx'),0x1801a41cf:('add','rcx, rcx'),0x1801a41d2:('mov','esi, dword ptr [rax + rcx*8 + 8]'),
0x1801a4520:('call','0x1801a37b0'),
0x1801a5bd0:('sub','ax, 1'),0x1801a5be6:('js','0x1801a61a3'),
0x1801a5c24:('cmp','word ptr [rbx + 2], ax'),0x1801a5c28:('je','0x1801a616f'),
0x1801a5caa:('je','0x1801a615c'),0x1801a5d90:('cmp','dword ptr [r10 + rcx*4 + 4], 0'),
0x1801a5da1:('cmp','dword ptr [r10 + rcx*4 + 8], r14d'),0x1801a5dbd:('jl','0x1801a5e43'),
0x1801a5e38:('call','r9'),
0x180193903:('cmp','dword ptr [rax + 0x18], 0x2000000'),0x18019391d:('mov','eax, 0x4000000'),
0x18019398f:('movss','xmm0, dword ptr [rbx]'),0x180193996:('subss','xmm0, dword ptr [rbx + 4]'),
0x18019399f:('maxss','xmm7, xmm1'),0x1801939a3:('subss','xmm1, dword ptr [rax + 4]'),
0x1801939a8:('minss','xmm1, xmm0'),0x1801939ac:('subss','xmm7, xmm1'),
0x180194146:('cmp','dword ptr [r8 + 0x14], 0'),0x180194158:('call','0x180193860'),
0x18019415d:('cmp','eax, dword ptr [rbx + 0x11c]'),0x180194163:('setg','al')}
for address,pin in pins.items():assert instructions[address]==pin,(hex(address),instructions.get(address),pin)
fs=functions();init=fs['1801a6980'];ctor=fs['18019c970'];clone=fs['18019cb00'];reader=fs['1801ad9d0']
assert tuple(b.qwords(0x1802e1e00,4))==(0x18019c970,0x18019cb00,0x18019cc50,0x1801ad9d0)
assert '*(undefined4 *)(param_2 + 0x1d9) = 0' in ctor and '*(undefined4 *)(param_2 + 0x2d0) = 0' in ctor
assert '0x1db' not in init and '0x2d3' not in init and '0x1db' not in clone and '0x2d3' not in clone
assert 'lVar8 = 2' in clone and 'puVar10[0x1a] = puVar9[0x1a]' in clone
assert 'param_2 + 0x158,1,1' in reader
ak=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<HH',ak,189379+12)==(0,0x10a) and ak[189379+16]==8 and ak[189728]==1
report=dict(blocks=[dict(start=hex(s),endExclusive=hex(e),sha256=h) for s,e,h in blocks],pins=[f'{p:x} {m} {o}' for p,(m,o) in pins.items()],
 metaBindings=dict(constructor='19C970',clone='19CB00',prefixCopy='19CC50',reader='1AD9D0'),
 policy='NEW ctor/clone clears1DB/2D3;fresh init does not replace them;actual graph158 read from serialized349=1. No external/reused state admitted.',
 schedule='append then whole-previous/right-source cross candidate ifcap allows;all descriptor-start mode1 windows then explicit local-final mode0',
 mode1='secondaryFFFF immediately keeps;primaryFirst+Last physical membership keeps;else secondary complete-physical suffix required before194140',
 geometry='Raw descriptor4/8/10 predicates andguide+C delta;193860 cross separation uses union guide0/4 interval, not max(height)',
 scope='Ordinary pruned96-byte group/32-byte arc state only. No native compact38 materialization, global NN context or text recognition claim.')
pathlib.Path(a.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print(f'PASS static{len(pins)} cross/schedule/geometry/prune pins in{len(blocks)} exactPEblocks +actualmain cap8/runtime158 binding')
