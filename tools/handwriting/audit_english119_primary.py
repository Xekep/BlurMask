"""Native primary plan plus exact shared numerical-core extraction proof."""
from pathlib import Path
import hashlib,json,runpy,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);saved=sys.argv;root=Path(__file__).resolve().parents[2]
sys.argv=[str(Path(__file__).with_name('audit_english119_primary_plan.py')),str(target.with_suffix('.plan.json'))]
g=runpy.run_path(sys.argv[0]);sys.argv=saved
b=g['b'];md=Cs(CS_ARCH_X86,CS_MODE_64)
ordinalPins={0x1801ef63f:('xor','ecx, ecx'),0x1801ef641:('movzx','r14d, cx'),
 0x1801ef94e:('cmp','r12, 0x14'),0x1801ef95c:('inc','r14w'),
 0x1801ef96b:('mov','word ptr [rsp + 0x20], r14w'),0x1801ef974:('mov','r10, qword ptr [rax + 0x160]'),
 0x1801ef831:('mov','word ptr [rsp + 0x40], r14w'),0x1801ef8d6:('mov','word ptr [rsp + 0x40], r14w'),
 0x1801efaae:('mov','word ptr [rsp + 0x40], 0xffff'),0x1801ee493:('movzx','eax, word ptr [rbp + 0xc00]'),
 0x1801ee49a:('mov','word ptr [rbp + 0x56], ax'),0x1801ee4a8:('mov','byte ptr [rbp + 0x5a], dl'),
 0x1801ee4bf:('mov','r9, qword ptr [rax + 0x1c0]')}
for a,expectedPin in ordinalPins.items():
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==expectedPin,(hex(a),i.mnemonic,i.op_str)
kernel=(root/'src/InkRecognition/LegacyFirstPrimaryRecurrenceKernel.cs').read_text(encoding='utf-8').replace('\r\n','\n')
start=kernel.index('\n    {',kernel.index('internal static LegacyFirstOrdinaryAcousticRecurrence Run('))
body=kernel[start:-3].replace('correct118','geometry.Correct118').replace('correct140','geometry.Correct140')
expected='9f376964237c202e0e5e1c5b11bb7a9744201116c642f72c1dff2c4b66145bd0'
assert hashlib.sha256(body.encode()).hexdigest()==expected
old=(root/'src/InkRecognition/LegacyFreshFirstBlankPrimary.cs').read_text(encoding='utf-8');assert'LegacyFirstPrimaryRecurrenceKernel.Run(requests,width,blank,replacement,closed,shadda,newline,space,nbsp,'in old
assert'geometry.Correct118,geometry.Correct140,observer,cancellationToken)'in old
candidate=(root/'src/InkRecognition/LegacyOwnedEnglish119Candidates.cs').read_text(encoding='utf-8');assert'internal LegacyOrdinaryGraphSourceRanges SourceRanges=>ranges;'in candidate
primary=(root/'src/InkRecognition/LegacyOwnedEnglish119FirstPrimary.cs').read_text(encoding='utf-8')
assert primary.index('graph.GetNode(n).Count>ushort.MaxValue')<primary.index('var geometry=new')<primary.index('for(ushort a=0;')
assert 'Add(s,category,checked((ushort)i))'in primary and'Add(policy.Blank,category,ushort.MaxValue)'in primary
assert 'group.ReadClassCost(symbol,ordinal,prototype,true,false,token)'in primary
target.write_text(json.dumps(dict(staticOnly=True,noVendorExecution=True,plan=str(target.with_suffix('.plan.json')),
 ordinalPins={hex(a):' '.join(p)for a,p in ordinalPins.items()},
 perNodeBound='Actual graph per-node count is preflighted <=65535 before transition allocations/ushort loops; proposal and total graph budgets are distinct.',
 requestLayout='1EF070 stack40 =1EE3B0 param9: actual R14W shortlisted ordinal for both category maps; FFFF for synthetic blank. 1EE3B0 packs at request+6, prototype at +A from category78, then frontend1C0.',
 sharedBodyOriginalSource='LegacyFreshFirstBlankPrimary.RunCore from committed36bb89c',sharedBodySha256=expected,
 extraction='Byte-identical normalized method BODY after replacing two correction method names with delegate parameters; old wrapper passes its same geometry instance.',
 scope='Actual owning119 primary98; source210/9C/category/blank/ancestor pins composed. No B0/B8/terminal/WORD. Prepared numeric requests use an internal friend-only kernel, production derives every request from actual cache.'),indent=2)+'\n',encoding='utf-8')
print(f'PASS{len(ordinalPins)} direct ordinal pins; exact extracted shared RunCore SHA256 +old same-callback delegation +actual source graph getter; native primary plan composed')
