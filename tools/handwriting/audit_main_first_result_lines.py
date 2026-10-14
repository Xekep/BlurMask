"""Pinned source249 submission/currentmap and24B guide lifecycle proof, static only."""
import argparse,hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args();b=Binary('MyScriptText.dll');fs=functions('MyScriptText.dll');checks={}
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def includes(addr,*terms):
    text=fs[f'{addr:x}'];assert all(t in text for t in terms),(hex(addr),terms);checks[f'{addr:x}']=list(terms)
includes(0x180157c60,'param_3[0x16] * 3.0','0.02 <=','FUN_180156950','local_1e0','param_3 + 5')
includes(0x180158970,'FUN_180157c60','param_4 + 0x18','0x34','0x38')
includes(0x180170400,'0x34','0x38','FUN_18016f6f0','FUN_18016f170','param_3 + 0x78','*(undefined4 *)(lVar10 + 0x10) = uStack_74')
includes(0x18016f6f0,'param_3','0x248','0xc1a')
includes(0x18016f170,'0x18','0x24','param_4','0x10')
includes(0x180163970,'*(undefined8 *)(param_2 + 0xe8) = 0')
Path(a.output).write_bytes((json.dumps(dict(checks=checks,scope='Owned currentdirectmain -> selected first275/source720 ->24B guides; firstwinner and24D/29B/265 separate'),indent=2)+'\n').encode('utf8'));print(f'PASS {len(checks)} pinned currentparent/firstsource/24B lifecycle checks; no vendor execution')
