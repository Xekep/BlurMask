"""Pinned193860/195520 stored-range/dual-guide geometry and prototype extent."""
import argparse,hashlib,json,struct,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);ins={};blocks=[]
for a,z,h in [(0x180193860,0x180194140,'a913702c074d72d5f406d29510bff732edf0c53b04aab397e923576e587b8d6b'),
              (0x180195520,0x180195650,'18488dd18ac09dee2fbb6f5938271af69f4686dbb1b9376285b6316b1eb2e760')]:
    raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
    ins.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)});blocks.append(dict(start=hex(a),endExclusive=hex(z),sha256=h))
pins={0x1801938b5:('cmp','r13d, 1'),0x1801938b9:('jbe','0x180194100'),
      0x180193913:('mov','r8d, dword ptr [rax + 0x10]'),0x180193917:('cmp','r8d, -1'),
      0x180193940:('mov','r9, qword ptr [rax + 0x40]'),0x180193966:('mov','r9, qword ptr [rax + 0x10]'),
      0x180193988:('mov','r9, qword ptr [rax + 0x40]'),0x180193996:('subss','xmm0, dword ptr [rbx + 4]'),
      0x18019399f:('maxss','xmm7, xmm1'),0x1801939a3:('subss','xmm1, dword ptr [rax + 4]'),
      0x1801939a8:('minss','xmm1, xmm0'),0x1801939ac:('subss','xmm7, xmm1'),
      0x1801939b8:('movss','xmm0, dword ptr [rax + 4]'),
      0x18019555d:('movzx','r14d, byte ptr [rax + 0x20]'),0x1801955ae:('mov','r10, qword ptr [rcx + 0x90]'),
      0x1801955d5:('mov','r9, qword ptr [rax + 0x78]'),0x1801955f1:('mov','r9, qword ptr [rax + 0x80]'),
      0x18019560b:('call','0x180193860'),0x180195610:('mov','ecx, dword ptr [rbp + 0x118]'),
      0x180195618:('cmovl','ecx, eax'),0x180195623:('mov','dword ptr [rbx], ecx'),0x18019562d:('jne','0x180195623')}
for a,pin in pins.items():assert ins[a]==pin,(hex(a),ins.get(a),pin)
f=functions();assert 'param_2 + 4,1,1)' in f['180010d80'] and '(param_1,0xc,(longlong)(char)param_2[4])' in f['180010d80']
assert '*(undefined1 *)(param_2 + 9) = 0' in f['1800103e0']
ak=(ROOT/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
at=188222;version,count=struct.unpack_from('<HH',ak,at);assert (version,count)==(0x201,192);at+=4
first=ak[at];at=(at+4)&~3;at+=first*4;at+=count+3
prototypes=ak[at];at=(at+4)&~3;at+=prototypes*4+2*(count+3)+13*prototypes
assert (first,prototypes,at)==(7,19,189165)
out=dict(blocks=blocks,pins=[f'{a:x} {m} {o}'for a,(m,o)in pins.items()],
         serializedGeometryPrototypeCount=prototypes,profileFieldsOffset=at,
         ownership='195520 graph90 supplies STORED group ranges; graph78/80 supply source descriptors.193860 one-range returns0; assigned cross groups use union vertical interval.195520 repeats value over profile+20 GEOMETRY prototype count19, not composite count at+48(35).',vendorExecution=False)
Path(args.output).write_text(json.dumps(out,indent=2)+'\n');print(f'PASS static{len(pins)} stored-range/dual-guide/195520 pins +2 exact PE blocks; raw prototype count19 distinct from composite35')
