"""Pinned selected first word-list ->156950 ->720/24B source binding."""
import argparse,hashlib,json,pathlib,sys
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();fs=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
checks={}
def terms(address,*values):
    body=fs[f'{address:x}'];assert all(v in body for v in values),(hex(address),values);checks[f'{address:x}']=list(values)
terms(0x180156950,'psVar13 = *(short **)(param_3 + 0x68)','plVar2 = *(longlong **)(psVar13 + 0xc)',
    'plVar2 = (longlong *)plVar2[4]','if (0 < sVar4)','local_78 & 0xffff',
    'uVar26 >> 0x1f & 1',"if ((char)psVar13 == '\\0')",'0x2d0','param_2 + 0x1c8',
    'lVar12 + 0x20) + 0xc','lVar12 + 0x20) + 0x10','lVar12 + 0x20) + 0x14')
terms(0x1801416b0,'plVar13[4] = *(longlong *)(param_2 + 0x18)','*(longlong **)(param_2 + 0x18) = plVar13')
terms(0x180157c60,'param_3[0x16] * 3.0','0.02 <=','FUN_180156950')
terms(0x180158970,'0x34','0x38','FUN_180157c60')
terms(0x180170400,'if (iVar3 < 0)','if (iVar5 != iVar4)','FUN_18016fec0','FUN_18016f170','0x34','0x38')
terms(0x180158bc0,'param_2 + 0x1c8,4,1')
report=dict(checks=checks,scope='Same selected first273 pool/linked word list/current parent descriptor; source720 scalar owners remain glyph alternatives, never TOP mass or WORD cost',
    empty='Zero source720 rows preserve first=end and native24B leaves line unassigned; actual24A lifecycle is not fabricated',
    portableBudgets='Global4096 refs/rows,65536 raw ranges,1048576 parent point visits across the full TEXT list; no per-word budget reset',
    ownership='Private produced TEXT and submission owner, exact factory/graph/chronology/category/resource/exponent; parent filter recomputed before use',noVendorExecution=True)
pathlib.Path(args.output).write_bytes((json.dumps(report,indent=2)+'\n').encode())
print('PASS static6 word-list/negative-arc/glyph scalar/249 map/descriptor/24B skip and source-exponent bindings')
