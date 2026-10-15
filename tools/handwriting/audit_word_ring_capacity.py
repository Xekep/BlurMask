"""Pinned PE/decompilation proof only; never loads the vendor DLL."""
import hashlib, json, sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary, functions
b=Binary(); f=functions()['1801fa300']
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
for token in ['param_3 / 0x28', 'param_3 = iVar5 + 0x28',
              '*(int *)(lVar3 + 0x68)', '*(int *)(lVar3 + 0x58)',
              '*(int *)(lVar3 + 0x50)', '*(int *)(lVar3 + 0x54)',
              'iVar14 + iVar12 < iVar6', 'iVar15 - iVar5',
              'param_2 + 0x23e0', 'param_2 + 0x23f0', 'param_2 + 0x23f8']:
    assert token in f,token
assert '*(undefined8 *)(param_2 + 0x50) = 0xffffffffffffffff' in functions()['18000c860']
out={'function':'1FA300 is grouped storage allocation/growth and pointer aliasing; not final winner selection',
     'retention':'signed min(max(field50,field58,field68)+field54,max(field68,field50+field58)); additions wrap32',
     'growth':'requested positive logical extent rounded upward to a multiple of40',
     'alias':'new logical row reuses reference from logical index minus retention distance',
     'scope':'prepared positive scalar plan only; no resolved container factory, native heap/failure rollback, buffer migration or tracing allocation'}
if len(sys.argv)!=2:raise ValueError('output.json')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
p.write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
print('PASS pinned1FA300 storage growth/retention/alias proof; final selection is separate')
