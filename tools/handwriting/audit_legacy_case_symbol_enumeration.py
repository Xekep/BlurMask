"""Optional pinned wrapper292 static callback proof; no vendor execution."""
import argparse,hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True,type=Path);a=p.parse_args()
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
f=functions();s=f['180232c80'];bridge=f['180232ed0']
assert b.qwords(0x1802e85b0+0x30,1)[0]==0x180232ed0 and 'FUN_180232c80' in bridge
for token in ('uVar5 <= param_2','param_2,0,&local_res18','param_2,1,&local_res18','param_2,2,&local_res18','local_res18 != -1','local_res1a != -1','local_res1c != -1','return false'):
 assert token in s,token
r=dict(callback='232C80 mode0 passthrough or three ordered triplets across0/1/2; duplicates, missing entries and early stop retained',slot='292VT2E85B0+30 ->232ED0 inner source30 callback',scope='Symbol callback only, full source factory/preparation/stateful query unfinished',vendorExecution=False)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(r)+'\n',encoding='utf8');print(r)
