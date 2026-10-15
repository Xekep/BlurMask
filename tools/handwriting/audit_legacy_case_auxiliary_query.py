"""Pinned wrapper292 V48 distinct from normal V40 and continuation V58."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise ValueError('output.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert b.qwords(0x1802e85b0+0x40,1)[0]==0x180233090
assert b.qwords(0x1802e85b0+0x48,1)[0]==0x180231ae0
assert b.qwords(0x1802e85b0+0x58,1)[0]==0x180233fc0
f=functions()['180231ae0']
for text in ['+ 0x48','lVar3 + 0x20','param_3 + 7','& 0x7c','return iVar2']:
 assert text in f,text
assert b.qwords(0x1802b4360+0x48,1)[0]==0x180093f50
report={'wrapper292':'V48=231AE0 forwardsinnerV48 then copiescasebits58..62 into record32byte7, preserving83',
        'inner2B7':'V48=093F50 terminal-only32 query;distinctemptyV58=05A3F0 andnormalV40',
        'state31':'valid transportedstate;231AE0 neverdereferencespolicytable',
        'remaining':'actualwrapperselection/fulltagaggregateforwarding/controllerpermissions'}
Path(sys.argv[1]).parent.mkdir(parents=True,exist_ok=True);Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS pinned231AE0 / ordinary2B7 terminal-query binding; five copiedbits, no vendorexecution')
