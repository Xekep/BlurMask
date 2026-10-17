"""Static composition for the separate actual249 FIRST119 wire reader."""
from pathlib import Path
import json,runpy,sys
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);plan=target.with_suffix('.plan.json');saved=sys.argv
sys.argv=[str(Path(__file__).with_name('audit_english_acoustic119_plan.py')),str(plan)]
g=runpy.run_path(sys.argv[0]);sys.argv=saved
f=g['f'];body=f['1801c2940'];checks=[]
def order(name,parts):
 p=-1
 for part in parts:
  p=body.find(part,p+1);assert p>=0,(name,part)
 checks.append(name)
order('119multiplier before optional initial/context', ['param_4 < 0x118','param_2 + 0xc','param_2 + 0x18','param_2 + 0x10','param_2 + 0x68'])
order('profile scalar native destinations', ['pcVar13 + 0x14','pcVar13 + 0x18','pcVar13 + 0x1c','pcVar13 + 0x28','pcVar13 + 0x2c','pcVar13 + 0x44','pcVar13 + 0x48','pcVar13 + 0x58','pcVar13 + 0x70','pcVar13 + 0x88','pcVar13 + 0x89','pcVar13 + 0x90','pcVar13 + 0x94','pcVar13 + 0x98','pcVar13 + 0x9c','pcVar13 + 0x9e'])
order('cache scalar wire order', ['param_2 + 0x78','param_2 + 0x7c','param_2 + 0x80','param_2 + 0x88','param_2 + 0x84','param_2 + 0x90','param_2 + 0xa4','param_2 + 0xb8','param_2 + 200','param_2 + 0xd0','param_2 + 0xd8','param_2 + 0xe0','param_2 + 0xe8','param_2 + 0x100','param_2 + 0x108','param_2 + 0x128','param_2 + 0x148','param_2 + 0x120','param_2 + 0x124'])
assert 'param_4 < 0x107' in f['1800ec490'] and 'uVar3 - uVar1 % uVar3' in f['1800ec490']
assert 'param_4 < 0x107' in f['1800ec890'] and 'FUN_1800ec070' in f['1800ec890']
report=json.loads(plan.read_bytes());report['additionalFieldOrderChecks']=checks
report['runtimeScope']='Separate first119/107 immutable reader. No EXP evaluation, first24C geometry, feature extraction or controller execution.'
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS FIRST119 composition: {len(checks)} native field-order checks + pinned119/107/24C inventory')
