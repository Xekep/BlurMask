"""Static2257C0 single-alternative scan proof; no vendor DLL execution."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions()['1802257c0']
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
for token in ['param_2 + 0x23f0','param_2 + 0x230a','param_2 + 0x2404','if (uVar8 < 2)',
              'param_6 ==','param_2 + 0x24b8','sVar2 != -7 && (sVar2 != -5)',
              'uVar11 - 0x200 < 0x801','if (iVar19 < *param_11)',
              '*param_11 = *param_11 +','param_2 + 0x2520','param_2 + 0x22f6']:
    assert token in f,token
_,q=b.descriptor(0x2d1);vt=b.qwords(q[3]+8,1)[0]
assert b.qwords(vt+0x68,1)[0]==0x1800f7a10
for address,expected in [(0x1800f7a10,'418b8034250000c3'),(0x18022591b,'f30f59c6'),
                         (0x180225937,'f30f58c7'),(0x18022594b,'f30f5cc7'),
                         (0x180225961,'f30f2cd8'),(0x1802ed16c,'cdcc4c3f'),(0x1802ed140,'0000003f')]:
    assert b.data[b.off(address):b.off(address)+len(bytes.fromhex(expected))]==bytes.fromhex(expected)
out={'selector':'2257C0 single-alternative-count0/1 branch; scans SOURCE23F0, strict first-tie ordering',
     'penalty':'selected2D1.V68=0F7A10 pure runtimecontext2534 int read; SSE float32*0.8 then signed0.5 and truncate',
     'filters':'finite cost/optional27.bit0; primary-or-headerbit2;24B8 sentinelsymbolFFF9/FFFB; invocation4/5 and sourcecategory gates',
     'publication':'retain rawrowcost14,rowfield22,resultflag8; cumulative2520 added with signed32 wrap only after selection',
     'scope':'prepared source table/penalties/flags; secondary0/1 and param7false only; no alternate exclusion list, rescue, live factory or accepted handwriting claim'}
if len(sys.argv)!=2:raise ValueError('output.json')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
print('PASS pinned2257C0 source-end/filters/strict ties/SSE penalty/cumulative publication proof; prepared no-rescue scope')
