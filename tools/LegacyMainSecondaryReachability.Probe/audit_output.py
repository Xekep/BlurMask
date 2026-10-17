"""Independent207CB0 chain marking from full source snapshots; no labels oracle."""
import base64,json,struct,sys
from pathlib import Path
d=json.loads(Path(sys.argv[1]).read_text())
read=lambda s:bytearray(base64.b64decode(s))
i32=lambda b,p:struct.unpack_from('<i',b,p)[0]
u16=lambda b,p:struct.unpack_from('<H',b,p)[0]
s8=lambda n:n if n<128 else n-256
imp=0x4000000

def chain(matrix,selected,primary,node,ordinal):
    for alt in range(2):
        start=(ordinal*2+alt)*40
        if i32(matrix[node],start)>=imp:break
        n,r,k=node,ordinal,alt
        replacement=65535 if r<primary else u16(matrix[n],start+32)
        replacealt=255 if r<primary else matrix[n][start+34]
        while True:
            at=(r*2+k)*40
            current=matrix[n]
            if r>=primary:
                if current[at+27]&128:break
                selected[n]=1
            current[at+27]|=128
            n-=s8(current[at+26])+1
            if n<0:break
            r=u16(current,at+24);k=current[at+27]&127
            if r>=primary:
                if replacement!=65535:r,k=replacement,replacealt
                at=(r*2+k)*40
                replacement=u16(matrix[n],at+32);replacealt=matrix[n][at+34]

for control in d['prepared']:
    matrix=list(map(read,control['before']));selected=[0]*4
    chain(matrix,selected,2,3,control['ordinal'])
    assert matrix==list(map(read,control['after']))
    assert selected==list(read(control['selected']))

rows=[read(v['before']['records']) for v in d['actual']]
primary=[read(v['before']['primary']) for v in d['actual']]
secondary=[read(v['before']['secondary']) for v in d['actual']]
ends=[v['before']['sourceEnd'] for v in d['actual']]
limit=d['primaryLimit'];selected=[1]*len(rows)
for n in range(len(rows)):
    for r in range(ends[n]):
        rows[n][r*88+6]&=253
        for k in range(2):secondary[n][(r*2+k)*40+27]&=127
    for slot in range(limit*2):primary[n][slot*40+27]&=127
for n in reversed(range(len(rows))):
    if not selected[n]:continue
    for r in range(ends[n]):
        if i32(rows[n],r*88+16)>=imp:continue
        if r<limit:
            for k in range(2):
                if i32(primary[n],(r*2+k)*40)>=imp:break
                pn,pr,pk=n,r,k
                while pn>=0 and pr<limit:
                    current=primary[pn];at=(pr*2+pk)*40
                    if current[at+27]&128:break
                    current[at+27]|=128
                    if current[at+31]&32:break
                    pr=u16(current,at+24);pn-=s8(current[at+26])+1;pk=current[at+27]&127
        chain(secondary,selected,limit,n,r)
        #ASM2084B7 falls through even when all configured chains were visited.
        pn,pr=n,r
        while pn>=0:
            current=rows[pn];at=pr*88
            if pr<limit:
                if current[at+6]&2:break
            else:selected[pn]=1
            current[at+6]|=2
            pr=u16(current,at+30);pn-=s8(current[at+32])+1
for n,value in enumerate(d['actual']):
    assert rows[n]==read(value['after']['records'])
    assert primary[n]==read(value['after']['primary'])
    assert secondary[n]==read(value['after']['secondary'])
assert selected==list(read(d['selected'])) and d['guards']==5
assert len(d['prepared'])==256
print('PASS actual4nodes full88/2408/2410 marks and256 independent replacement/visited chains; no compact/WORD output.')
