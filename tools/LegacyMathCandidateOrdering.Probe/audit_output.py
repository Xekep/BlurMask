"""Separate literal036810 transcription over full candidate byte records."""
import base64
import collections
import hashlib
import json
import struct
import sys
from pathlib import Path

if len(sys.argv)!=2:
    raise SystemExit('capture.json')
capture=json.loads(Path(sys.argv[1]).read_bytes())
assert capture['caseCount']==181 and len(capture['cases'])==181 and capture['guards']==4
costs=[0xff800000,0xff7fffff,0xbf800000,0x80000000,0,0x3c23d70a,
       0x3f000000,0x3f800000,0x7f7fffff,0x7f800000,0x7fc00000,0x7fa00001]

def make(count,seed,equal):
    result=[]
    random=(seed*7919+17)&0xffffffff
    for i in range(count):
        row=bytearray((seed*23+i*37+j*19)&255 for j in range(168))
        random^=(random<<13)&0xffffffff
        random^=random>>17
        random^=(random<<5)&0xffffffff
        struct.pack_into('<II',row,0,0x3f800000 if equal else costs[random%len(costs)],i)
        result.append(bytes(row))
    return result

def reorder(rows,first,last):
    #Inclusive native pointer difference; short-range branch threshold0xD20.
    if (last-first)*168<0xd20:
        for current in range(first+1,last+1):
            saved=rows[current]
            key=struct.unpack_from('<f',saved)[0]
            previous=current-1
            while previous>=first and key<struct.unpack_from('<f',rows[previous])[0]:
                rows[previous+1]=rows[previous]
                previous-=1
            rows[previous+1]=saved
        return
    pivot=struct.unpack_from('<f',rows[first+(last-first)//2])[0]
    left,right=first,last
    while True:
        while struct.unpack_from('<f',rows[left])[0]<pivot:
            left+=1
        while pivot<struct.unpack_from('<f',rows[right])[0]:
            right-=1
        if right<left:
            break
        rows[left],rows[right]=rows[right],rows[left]
        left+=1
        right-=1
        if left>right:
            break
    if first<right:
        reorder(rows,first,right)
    if left<last:
        reorder(rows,left,last)

verified=0
for case in capture['cases']:
    rows=make(case['count'],case['seed'],case['equal'])
    initial=b''.join(rows)
    reorder(rows,0,len(rows)-1)
    final=b''.join(rows)
    if 'input' in case:
        assert base64.b64decode(case['input'],validate=True)==initial
        assert base64.b64decode(case['output'],validate=True)==final
    else:
        assert hashlib.sha256(initial).hexdigest()==case['inputSha256']
        assert hashlib.sha256(final).hexdigest()==case['outputSha256']
    assert collections.Counter(rows)==collections.Counter(initial[i:i+168] for i in range(0,len(initial),168))
    if case['equal'] and case['count'] in (20,21,42):
        identities=[struct.unpack_from('<I',r,4)[0] for r in rows]
        expected=list(range(20)) if len(rows)==20 else list(range(20,-1,-1)) if len(rows)==21 else list(range(21,42))+list(range(21))
        assert identities==expected
    verified+=len(final)
print(f'PASS181 independent candidate-ordering controls/{verified} full bytes incl65536 records; tie20/21/42 manual goldens and NaN payloads, prepared only')
