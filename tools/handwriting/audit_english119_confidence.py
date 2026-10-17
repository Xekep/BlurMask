"""Static-only actual FIRST119 3A0/1BCBD0 confidence ownership."""
from pathlib import Path
import hashlib,json,runpy,struct,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);saved=sys.argv;sys.argv=[str(Path(__file__).with_name('audit_english119_cache_plan.py')),str(target.with_suffix('.cache.json'))]
g=runpy.run_path(sys.argv[0]);sys.argv=saved;b=g['b'];f=g['f'];d=g['d'];md=Cs(CS_ARCH_X86,CS_MODE_64)
regions=[(0x1801bcbd0,0x1801be160,'43eee565898bbe976b7fdfef20ac1c4699e6088669d093166c09729940ac520c'),
 (0x1800ebd30,0x1800ebd4b,'0ce2ce6e08e03b86adc5effaf41626a1a23022d1e98dc60037c7381c0a1379ce')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
score=f['1801bcbd0'];init=f['1801b0760'];calibrate=f['18013e8e0']
assert 'local_f0 = 0x14'in score and'param_2 + 0x3a0'in score
assert '*(ushort *)((longlong)puVar2 + 0x12)'in score and'*(ushort *)((longlong)puVar2 + 0x14)'in score
assert'0.0 < *(float *)(param_2[0x1c]'in init and'(uVar22 < *(ushort *)(param_2 + 0x52))'in init
assert'if (local_138 != 0)'in init and'*(ushort *)(lVar15 + 0x14) = uVar25'in init
assert'*(undefined1 *)(param_2 + 0x35) = *(undefined1 *)((longlong)param_3 + 0x19)'in init
assert'FUN_18025ee50(param_2 + 8,0,0x150)'in f['1801b0330'] #Includes110/118.
assert'dVar3 = 0.575'in calibrate and'dVar4 = 0.0'in calibrate
assert'fVar19 + fVar18 + fVar20 + fVar21 + fVar22 + fVar23 + fVar24'in score
assert' / (float)uVar16'in score and'if (1.0 <= fVar18)'in score
begin=score.index('uVar1 == *(ushort *)(param_2 + 0x1e4)');end=score.index('\n    else {',begin);closed=score[begin:end]
assert'*(longlong *)(param_2 + 0x178) + 0x60'in closed and'*local_e8[0]'in closed
assert'FUN_18013e8e0'not in closed and'*(undefined4 *)(lVar3 + uVar14 * 4) = uVar17'in closed
assert b.qwords(0x1802df340+0x60,1)[0]==0x180168c00 and b.qwords(0x1802e1ff0+0x90,1)[0]==0x1801a7970
pins={0x18013e922:('movsd','xmm1, qword ptr [rip + 0x1ae926]'),0x18013e93a:('cvtps2pd','xmm0, xmm0'),
 0x18013e941:('mulsd','xmm0, xmm1'),0x18013e945:('call','0x18024e970'),0x18013e97a:('cvtpd2ps','xmm0, xmm0')}
for a,p in pins.items():i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==p
assert struct.unpack_from('<d',b.data,b.off(0x18013e922+8+0x1ae926))[0]==.575
assert g['flags']&0x800==0 and g['c']['108']==0 and g['c']['128']==0
assert all(label<182 for label in g['c']['D8'])
labels=g['alphabet']['mapping'];reverse={v:k for k,v in labels.items()}
assert(reverse['-'],reverse['_'],reverse['\u00ad'],reverse['\U000f0002'],reverse['\U000f0003'],reverse['\U000f0001'])==(187,188,185,189,190,211)
report=dict(regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins={hex(a):' '.join(p)for a,p in pins.items()},
 source=g['model']['first']['offset'],networkWidth=182,alphabet=217,individualChannel1a8=False,calibrationMode108=0,
 map='Same actual AL/header/eight C8 groups; all17 members below182 so no group extended remap. Insert-only3A0 dash187/188/185/189/190 ->178; synthetic218->74,219->69. Output symbols unchanged.',
 scalar='Candidate24 list top20 only; D8 actualmixed107 result via alias target.13E8E0 mode0 absenttable: DOUBLE0.575 logistic, floatstores; family1+family0+fivezero, divide2, upper1 clamp. Math.Exp portable adaptation.',
 closed='Label211/headerCFFFF: graph90 actual stored range firstU32 physicalID ->21860 original source probability; rawfloat store, no whole-scope/sigmoid/mean/clamp. Missing candidate returns0 before lookup.',
 boundary='Fresh owning119 cache only. No candidate24.field0 weight, word confidence, controller selection, text or English readiness.')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS2newranges/5scalarinstructions +actual119 cache dependencies;217AL/182NN/3A0/default0/closed21860 ownership; static only')
