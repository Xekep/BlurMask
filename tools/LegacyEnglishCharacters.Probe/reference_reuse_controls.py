"""Independent021760 initial-prefix scan, append and failure-write controls."""
import argparse
import json
from pathlib import Path
import random
import struct

p=argparse.ArgumentParser(description=__doc__)
p.add_argument('publication',type=Path)
p.add_argument('output',type=Path)
a=p.parse_args()
d=json.loads(a.publication.read_bytes())
literal={bytes.fromhex(r[0]):r[1] for r in d['literalRows']}
texts=['','A','B','\n',' ','\u00a0','\u2639','\u263a','AB','\U000f0000','\U000f0001','\u200c','\u03a9','\u4e00']
rawtexts=[s.encode() for s in texts]

def flags(raw,glyph):
    result=glyph&1023
    if not raw:return result
    cp=ord(raw.decode()[0])
    script=next((k for x,y,k in d['scriptRanges']if x<=cp<=y),0)
    if script in [7,8,9] or script!=10 and 0x3000<=cp<=0x303f:result|=0x40000
    elif script==10:result|=0x80000
    bit={10:0x2000,32:0x1000,160:0x1000,0x200c:0x8000}.get(cp,0)
    if bit:result|=bit
    elif literal.get(raw,-1)>=0:result|=0x10000
    if raw in ['\U000f0000'.encode(),'\U000f0001'.encode()]:result|=0x100000
    return result

rng=random.Random(21760)
cases=[]
for test in range(1024):
    initial=[];intern={};records=[];aggregate=rng.randrange(2**32);initial_aggregate=aggregate
    def identity(raw):
        if raw not in intern:intern[raw]=len(intern)+1
        return intern[raw]
    for i in range(rng.randrange(7)):
        raw=None if rng.randrange(8)==0 else rng.choice(rawtexts)
        storage=bytearray(rng.randbytes(40));storage[16:24]=b'\0'*8
        initial.append(dict(storage=storage.hex(),text=None if raw is None else raw.hex()))
        records.append([storage,None if raw is None else identity(raw)])
    count=rng.randrange(13);marker=rng.randrange(-2,count+3)
    preintern=[rng.choice(rawtexts) for _ in range(rng.randrange(4))]
    incoming=[]
    for i in range(count):
        incoming.append(dict(glyph=rng.randrange(65536),utf8=rng.choice(rawtexts).hex(),failure=0))
    if test%3==0 and incoming:incoming[rng.randrange(count)]['failure']=1+test%5
    #Explicit old-only scan, empty+smiley preservation, selection and failure
    #examples are retained in addition to random controls.
    if test<6:
        initial=[];records=[];intern={};preintern=[b'A'] if test==2 else [];marker=1
        incoming=[dict(glyph=0x1234,utf8='41',failure=0),dict(glyph=0x5678,utf8='41',failure=0)]
        if test==1:
            storage=bytearray(rng.randbytes(40));storage[16:24]=b'\0'*8
            initial=[dict(storage=storage.hex(),text='41')];records=[[storage,identity(b'A')]]
        if test in [3,4,5]:incoming[1]['failure']=test
    for raw in preintern:identity(raw)
    original=len(records);mapped=-1;visited=0;appended=0;ok=True
    for i,row in enumerate(incoming):
        raw=bytes.fromhex(row['utf8']);stage=row['failure']
        if stage==1:ok=False;break
        newly_interned=raw not in intern;ident=identity(raw)
        if stage==2:ok=False;break
        match=next((j for j in range(original) if records[j][1]==ident),None) if not newly_interned else None
        if match is not None:
            if i==marker:mapped=match
        else:
            if i==marker:mapped=len(records)
            if stage==3:ok=False;break
            storage=bytearray(40);struct.pack_into('<I',storage,8,row['glyph']&1023);struct.pack_into('<I',storage,32,len(raw))
            records.append([storage,None]);appended+=1
            if stage==4:ok=False;break
            records[-1][1]=ident
            if stage==5:ok=False;break
            f=flags(raw,row['glyph']);struct.pack_into('<I',storage,8,f);aggregate|=f
        visited+=1
    cases.append(dict(initial=initial,preintern=[raw.hex() for raw in preintern],aggregate=initial_aggregate,incoming=incoming,marker=marker,
        expected=dict(ok=ok,marker=mapped,original=original,visited=visited,appended=appended,aggregate=aggregate,
            poolCount=len(intern),records=[dict(storage=s.hex(),identity=i) for s,i in records])))
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_bytes((json.dumps(dict(cases=cases),separators=(',',':'))+'\n').encode())
print('Generated1024 independent reuse/whole40-byte/prefix controls.')
