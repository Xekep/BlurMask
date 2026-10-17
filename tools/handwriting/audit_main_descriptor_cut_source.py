"""Pinned static main193380 selected-source and config-byte3 audit. No DLL load."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,parse_alphabet
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
ranges=[(0x18000c8a8,0x18000c8fa),(0x180150e38,0x180150e95),(0x1801514d8,0x1801514ea),
 (0x180151593,0x1801515aa),(0x1801a6bc2,0x1801a6bdc),(0x1801933a2,0x1801933be),
 (0x1801933be,0x180193401),
 (0x180193401,0x180193427),(0x180193449,0x180193498),(0x1801934af,0x1801934c3),
 (0x18018dd50,0x18018dd5f),(0x1801a0a2d,0x1801a0a46),(0x18019fa17,0x18019fa46)]
instructions={};blocks=[]
for a,z in ranges:
 raw=b.data[b.off(a):b.off(z)];ins=list(md.disasm(raw,a));assert ins[-1].address+ins[-1].size==z
 instructions.update({i.address:(i.mnemonic,i.op_str) for i in ins})
 blocks.append(dict(start=hex(a),endExclusive=hex(z),sha256=hashlib.sha256(raw).hexdigest()))
pins={
 0x18000c8a8:('xor','eax, eax'),0x18000c8f3:('mov','word ptr [rdi + 0xc4], ax'),
 0x180150e41:('movzx','ecx, byte ptr [rax + 0xc4]'),0x180150e51:('jne','0x180150e8c'),
 0x180150e60:('call','r8'),0x180150e63:('cmp','eax, 0x137'),0x180150e68:('je','0x180150e8c'),
 0x180150e6e:('lea','r8, [rip + 0x19a52b]'),0x180150e7e:('call','0x180018f70'),
 0x180150e83:('cmp','ax, bx'),0x180150e86:('jne','0x180150e8c'),
 0x180150e88:('xor','al, al'),0x180150e8c:('mov','al, 1'),0x180150e8e:('mov','byte ptr [r14 + 0x91], al'),
 0x1801514df:('movzx','eax, byte ptr [r14 + 0x91]'),0x1801514e7:('mov','byte ptr [rbp - 0x46], al'),
 0x180151593:('lea','r8, [rbp - 0x49]'),0x1801515a7:('call','r9'),
 0x1801a6bd2:('movzx','eax, byte ptr [rbp + 3]'),0x1801a6bd6:('mov','byte ptr [rsi + 0x1d8], al'),
 0x1801933a2:('mov','r14d, r8d'),0x1801933b7:('mov','r9, qword ptr [rax + 0x10]'),0x1801933bb:('call','r9'),
 0x1801933cf:('mov','r8, qword ptr [rcx + 0x28]'),0x1801933d6:('call','r8'),0x1801933d9:('xor','r12d, r12d'),
 0x180193401:('mov','r8d, dword ptr [rdi + 0x10]'),0x180193409:('je','0x1801934cc'),
 0x180193420:('mov','r9, qword ptr [rax + 0x40]'),0x180193424:('call','r9'),
 0x180193449:('movss','xmm0, dword ptr [rbp + 4]'),0x180193453:('movss','xmm1, dword ptr [rbp]'),
 0x18019345c:('mov','r9d, dword ptr [rdi + 0x24]'),0x180193480:('mov','eax, dword ptr [r14]'),
 0x18019348f:('mov','dword ptr [rsp + 0x20], eax'),0x180193493:('call','0x180191950'),
 0x1801934b7:('mov','qword ptr [rsp + 0x20], r12'),0x1801934bc:('mov','r10, qword ptr [rax + 0x30]'),
 0x1801934c0:('call','r10'),0x18018dd52:('mov','dword ptr [rdx + 0x1e8], eax'),
 0x18018dd58:('mov','dword ptr [rdx + 0x1f8], eax'),
 0x1801a0a2d:('mov','rdx, qword ptr [rsi + 0x180]'),0x1801a0a43:('call','r8'),
 0x18019fa17:('cmp','cx, 3'),0x18019fa21:('cmp','qword ptr [rax + 0x20], -1'),
 0x18019fa2c:('mov','byte ptr [rsi + 0x1f0], 1'),0x18019fa3f:('mov','dword ptr [rbp - 0x70], 0xfffffff9')}
for address,pin in pins.items():assert instructions[address]==pin,(hex(address),instructions.get(address),pin)
assert b.data[b.off(0x1802eb3a0):b.off(0x1802eb3a0)+4]==bytes.fromhex('f3b08089')
targets={0x30:0x18018dd50,0x38:0x180193380,0x40:0x1801935d0,0x48:0x180093a90,0x50:0x1801935e0,0x58:0x180193690,0x60:0x180193720}
for slot,target in targets.items():assert b.qwords(0x1802e1490+slot,1)[0]==target
ak=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
al=parse_alphabet(ak,99);assert al['label_count']==192 and '\U000f0009' not in al['mapping'].values()
report=dict(pins=[f'{a:x} {m} {op}' for a,(m,op) in pins.items()],blocks=blocks,
 bindings={hex(k):hex(v) for k,v in targets.items()},
 configByte3='NEW no-property C4=0 AND selected source135!=137 AND actual AL F0009 absent =>frontend91/config3/graph1D8 false; pending kind3 metadata still retained',
 replacement='193380 selects actual descriptor/guide/physical interval;191950 outputs full prefix/selected/suffix;sourceV30 composes existing point map;samplerV30 resets only per-descriptor markers/records',
 scope='No vendor execution. Global graph append/remap/cross-descriptor merging and NN classification are not supplied by the cut-source API.')
pathlib.Path(args.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS static{len(pins)} instruction pins/{len(targets)} sampler bindings;actual absent F0009 and exact configbyte3 producer; selected-source and pending-control ownership')
