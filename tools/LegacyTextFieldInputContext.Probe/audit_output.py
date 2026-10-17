"""Independent structural oracle; no model, label or ignored research imports."""
import argparse,base64,json,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('capture',type=Path);a=p.parse_args()
d=json.loads(a.capture.read_bytes());total=0;copies=0;objects=0
assert len(d['actualRecognitionRoots'])==4 and len(d['freshLibraryRoots'])==256
for row in d['actualRecognitionRoots']+d['freshLibraryRoots']:
    raw=row['rawPointBits'];n=len(raw);objects+=n
    assert n and all(len(x)%2==0 and len(x)>0 for x in raw)
    # Original-object position keys are not registry IDs. Native full C22
    # spans first raw object to the last, including terminal FF fractions.
    first=3 | (0xff00 if len(raw[0])==2 else 0)
    last=((n-1)<<32)|((len(raw[-1])//2-1)<<16)|0xff05
    expected_ranges=struct.pack('<QQ',first,last)
    # Replay the successful fresh C51 registration, independently of runtime.
    next_id=0;maximum=-(1<<63);actual_id=next_id
    maximum=max(maximum,actual_id);next_id+=1
    counter=struct.pack('<qqB',next_id,maximum,0)
    matrix=struct.pack('<6d',1,0,0,0,1,0)*n
    for depth,ctx in enumerate(row['contexts']):
        assert ctx['depth']==depth and ctx['registryId']==actual_id
        assert ctx['category']==100 and ctx['name']=='INPUT'
        assert base64.b64decode(ctx['ranges'])==expected_ranges
        assert base64.b64decode(ctx['counter'])==counter
        assert base64.b64decode(ctx['daf32'])==b''
        assert base64.b64decode(ctx['matrices'])==matrix
        assert ctx['matchedViews']==list(range(n))
        assert [base64.b64decode(x) for x in ctx['rawCopies']]==[struct.pack('<'+'I'*len(x),*x) for x in raw]
        if depth:
            # Fresh target Next0 + preserved explicit ID0 -> Next1;09ED40
            # restores source Next only when it is greater than live maximum.
            target_next=0;target_max=actual_id
            if target_next<=actual_id:target_next=actual_id+1
            assert target_max<next_id and target_next==next_id
            copies+=1
    total+=1
assert d['roots']==total and d['copies']==copies and d['objects']==objects
assert d['guards']==sum(2+4*len(r['contexts'])+2*(len(r['contexts'])-1) for r in d['actualRecognitionRoots']+d['freshLibraryRoots'])
assert any(len(r['rawPointBits'])==256 and r['contexts'][0]['registryId']==0 for r in d['freshLibraryRoots'])
print(f'PASS independent INPUT/C22 structural bytes: {total} roots/{copies} selected copies/{objects} objects/{d["guards"]} guards.')
