"""236910 forwards the SAME request pointer; only Skip/stamped output byte4 change."""
import hashlib,json,re,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise ValueError('output.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
f=functions()['180236910']
assert f.count('(*pcVar11)(param_1,plVar7,param_3)')==2
assert f.count('param_3[0xe] = param_3[0xe]')==2 and 'param_3[0xe] - uVar6' in f and 'param_3[0xe] + puVar1[2]' in f
assert f.count('*plVar7 + 0x40')==2 and f.count('*plVar7 + 0x48')==2
assert 'uVar8 + 4 +' in f and 'lVar9 + 4 +' in f
#No input source-ID/case/category rewrite before either dispatch.
assert not re.search(r'\*param_3\s*=(?!=)',f) and not re.search(r'param_3\[1\]\s*=(?!=)',f)
assert b.qwords(0x1802e85b0+0x40,1)[0]==0x180233090 and b.qwords(0x1802e85b0+0x48,1)[0]==0x180231ae0
assert b.qwords(0x1802e85b0+0x88,1)[0]==0x1801aea30
assert b.data[b.off(0x1801aea30):b.off(0x1801aea30)+17]==bytes.fromhex('488b5208488b024c8b908800000049ffe2')
out={'aggregate':'236910sameparam3 dispatchedforstartupandcontinuation;no inputsourceID/caseclearing',
 'mutation':'onlyskip38 becomesrelative/reverts;outputrecord32byte4 stampedselectedordinal+1',
 'case292':'V40=233090 normalexpansion,V48=231AE0copiedcasebits;V88forwardstoinnerV88',
 'adapter':'serializedowningfulltagframe;weakidentityperoriginalrequest;existingaggregatecursor/bases/stampunchanged',
 'remaining':'actualcontaineroptions/permissionflow and finalwordwinner'}
Path(sys.argv[1]).parent.mkdir(parents=True,exist_ok=True);Path(sys.argv[1]).write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
print('PASS pinned236910same-request forwarding/skip/stamps,292dispatch andeligibility forwarding;staticonly')
