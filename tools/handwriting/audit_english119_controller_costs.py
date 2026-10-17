"""Static FIRST119 controller-cost and geometry bindings. No SDK execution."""
from pathlib import Path
import hashlib,json,runpy,struct,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);saved=sys.argv;sys.argv=[str(Path(__file__).with_name('audit_english119_cache_plan.py')),str(target.with_suffix('.cache.json'))]
g=runpy.run_path(sys.argv[0]);sys.argv=saved;b=g['b'];f=g['f'];md=Cs(CS_ARCH_X86,CS_MODE_64)
regions=[(0x180193860,0x180194140,'a913702c074d72d5f406d29510bff732edf0c53b04aab397e923576e587b8d6b'),
 (0x180195520,0x180195650,'18488dd18ac09dee2fbb6f5938271af69f4686dbb1b9376285b6316b1eb2e760'),
 (0x1801be530,0x1801be6a8,'9f6d27f219b191689f8e557b466f55292bfb52b3e32776dda918b62c3d18624a'),
 (0x1801c1260,0x1801c12f6,'31660a4020f650700e58d2e0ec9b2705bfee5c584d3a6415ece6ab5da30915ac'),
 (0x1801c13a0,0x1801c1520,'b9319d885c0b3698680031ce1204f244b2fd3be28a8c2ff3582c388a74b932fa'),
 (0x1801bc900,0x1801bcbd0,'3edff9e95b0130c78ad628de0e59b55e8cc46367b924c91404534b8e36d4feca')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins={0x18019555d:('movzx','r14d, byte ptr [rax + 0x20]'),0x18019560b:('call','0x180193860'),
 0x180195610:('mov','ecx, dword ptr [rbp + 0x118]'),0x180195623:('mov','dword ptr [rbx], ecx'),
 0x180194013:('movss','xmm0, dword ptr [rbp + 0x108]'),0x180194024:('subss','xmm1, dword ptr [rbp + 0x68]'),
 0x180194041:('addss','xmm1, xmm6'),0x180194045:('cvtss2sd','xmm0, xmm1'),0x180194049:('mulsd','xmm0, xmm9'),0x18019404e:('addsd','xmm0, xmm10'),
 0x1801bc910:('movss','xmm1, dword ptr [rdx + 0x100]'),0x1801bc929:('je','0x1801bcb99'),
 0x1801be55b:('mov','dword ptr [rax], 0x3f800000'),0x1801be5e3:('cvtdq2ps','xmm0, xmm0')}
for a,p in pins.items():i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==p,(hex(a),i.mnemonic,i.op_str,p)
assert b.qwords(0x1802e4180+0x78,1)[0]==0x1801c1260 and b.qwords(0x1802e4180+0x88,1)[0]==0x1801c13a0
assert b.qwords(0x1802de610+0x150,1)[0]==0x1801529a0 and b.qwords(0x1802de610+0x128,1)[0]==0x180152790
assert '*(char *)((longlong)param_4 + 10) * 4'in f['1801c13a0']and '*(float *)(lVar6 + 0x1c) * 40000.0'in f['1801c13a0']
assert '*(undefined4 *)(param_3 + 0x1c) = 0x3f800000'in f['1801bc900']and g['c']['100']==0
d=g['d'];assert struct.unpack_from('<f',d,18309)==(3.,)and struct.unpack_from('<f',d,18318)==(1.,)and struct.unpack_from('<i',d,18322)==(0x4000000,)
report=dict(regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins={hex(a):' '.join(p)for a,p in pins.items()},
 scope='Same owned EN119 cached group; no NN rerun, no scheduler claims. Prototype vector count24, not42 composite IDs. Actual raw100=0 fixes group scale1.',
 arithmetic='193860 stored ranges/source height; sampler68 offset and108 slope, cap118. Lookup index FLOAT(x+4) then DOUBLE*255+.5.1BE530 top20 EXP portable Math.Exp adaptation, sequential float stores/sum/division; singleton1/tail0.',
 channels='candidate24.field0->1C1260/frontend150 vs vector10[channel0]->1C1300/frontend1B8.1C13A0 prototype query is independent of both.',vendorExecution=False)
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print('PASS6native blocks/14instructions +actual119 cache proof; controller field0 vs vector10, prototype-index requests, sampler108 vs114')
