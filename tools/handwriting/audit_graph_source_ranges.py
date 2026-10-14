"""Pinned ordinary forward21B source-range dispatcher. Static PE reads only."""
import argparse
import hashlib
import json
import pathlib
import sys

p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
f=functions()
bindings={0x1802de610+0x188:0x180152b20,0x1802e1ff0+0x120:0x1801a9ae0,
          0x1802df340+0x90:0x180168d00,0x1802df340+0xa0:0x180168e10,
          0x1802df340+0xc8:0x180168f80}
for slot,target in bindings.items():assert b.qwords(slot,1)[0]==target
path=f['1801a9070']
for text in ('if ((int)param_4 < (int)param_3[5])','if (param_3[5] != param_4)',
             'FUN_1801a8e60','*param_5 =', '*(uint *)(param_6 + 1) = uVar16',
             '((longlong)(int)uVar5 + 1) * 0x10'):
    assert text in path,text
emitter=f['1801a8e60']
for text in ('+ 0x90)', '+ 0xa0)', '+ 200)', '0x19 + lVar11', '0x1a + lVar11',
             '0x1b + lVar11','*(int *)(lVar6 + 8 + uVar7 * 0xc) + -1',
             '*pbVar1 ^ (*(byte *)(lVar11 + 0x18 + lVar3) ^ bVar4) & 1'):
    assert text in emitter,text
sort=f['1801a9570']
for text in ('iVar8 = *piVar1 - *piVar2','(uint)pbVar9[0x20] - (uint)*pbVar9',
             '(uint)pbVar9[0x21] - (uint)pbVar9[1]','param_4 = param_4 - 1'):
    assert text in sort,text
dispatcher=f['1801a9ae0']
for text in ('if ((int)param_3 <= (int)param_4)','puVar20[4] == param_3','FUN_1801a9070',
             'FUN_1801a9570','piVar9[2] == piVar2[2]','piVar9[2] == piVar2[5]',
             '*(byte *)(lVar21 + 0x18 + lVar28) & 3','((byte)~*(byte *)(puVar23 + 3) & 1) + 1',
             'uVar19 = uVar19 - 1'):
    assert text in dispatcher,text
report=dict(bindings=len(bindings),forwardPath='First matching starting context; ordered recursive successors; rollback failed paths; no acoustic group range substitution',
            metadata='168D00 start/168E10 end/168D00 endpoint start;168F80 prepared physical tail flag',
            sorting='Native stable bubble comparator; key triples ascending and remainder bytes descending',
            merge='Reverse pass: equal endpoint-start keys/remainders, or next F8 metadata record with flag1/2; end owner chosen by flag0',
            excluded=['reversed contexts','protocol/type1/type2 source records','main controller/winner/recognition accuracy'],vendorExecution=False)
target=pathlib.Path(a.output);target.parent.mkdir(parents=True,exist_ok=True);target.write_text(json.dumps(report,separators=(',',':'))+'\n',encoding='utf-8');print(json.dumps(report))
