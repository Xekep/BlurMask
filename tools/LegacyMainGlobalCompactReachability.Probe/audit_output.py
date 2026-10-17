"""Independent full-storage predecessor marking plus actual continuation gates.

Continuation backend responses are diagnostic source outputs, not OCR labels
or an independent full dictionary reimplementation.
"""
import base64,json,struct,subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
subprocess.run([sys.executable,str(root/'tools/LegacyCompactWordReachability.Probe/audit_output.py'),sys.argv[1]],check=True)
data=json.loads(Path(sys.argv[1]).read_bytes());assert len(data)==8
checks=calls=positive=gates=0
for case in data:
    assert case['continuationChecks']==len(case['continuations'])
    for node,ordinal,answer in case['continuations']:
        row=base64.b64decode(case['nodes'][node]['rows'])[ordinal*88:(ordinal+1)*88]
        assert len(row)==88 and struct.unpack_from('<i',row,16)[0]<0x4000000 and ordinal<case['primary']
        if row[6]&1 or struct.unpack_from('<i',row,48)[0]!=-1:assert not answer;gates+=1
        elif row[5]&2:assert answer;gates+=1
        checks+=1;positive+=answer
    calls+=case['continuationBackendCalls']
assert checks>10000 and positive>1000 and calls>0 and gates>0
print(f'PASS independent8 actual global marking owners/{checks} finite consumer rows/{gates} early gates/{positive} source continuations/{calls} backend calls; dictionary responses diagnostic, no text-quality claim')
