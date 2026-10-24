"""Independent actual two-WORD distinct linked-label ranking and275 score oracle.

The narrower fixture coverage is asserted, not substituted for general13D360.
"""
import base64,hashlib,importlib.util,json,math,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('raw_al',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
al=importlib.util.module_from_spec(spec);spec.loader.exec_module(al)
ak=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
alphabet=al.read_alphabet(ak);assert not set('\u2610\u2611\u2612')&set(alphabet.values())
ignored=next((i for i,t in alphabet.items()if t=='\u200d'),65535)
dec=base64.b64decode;u16=lambda b,n:struct.unpack_from('<H',b,n)[0]
u64=lambda b,n:struct.unpack_from('<Q',b,n)[0];rf=lambda b,n:struct.unpack_from('<f',b,n)[0]
f32=lambda v:struct.unpack('<f',struct.pack('<f',v))[0]
def close(a,e):
    aa=struct.unpack('<I',struct.pack('<f',a))[0];ee=struct.unpack('<I',struct.pack('<f',e))[0]
    assert abs(aa-ee)<=4,(a,e)

ENABLED="Enabled219580 candidate augmentation cannot be omitted."

def completed(c):
    before=list(map(dec,c['before']['words']));after=list(map(dec,c['after']['words']))
    assert len(before)==len(after)==2 and c['before']['selected']==c['after']['selected']==[0,1]
    assert c['before']['refs']==c['after']['refs'] and c['comparisons']==1 and c['quota']==2
    pool={n['key']:dec(n['records'])for n in c['pool']}
    def sequence(word):
        ref=u64(word,8);seen=set();output=[]
        while ref!=2**64-1:
            assert ref not in seen;seen.add(ref)
            owner=ref&0x010000ffffffffff;ordinal=ref>>40&65535
            row=pool[owner][ordinal*40:(ordinal+1)*40];assert len(row)==40
            if not row[6]&2:
                kind=u16(row,4)&7;label=u16(row,8);assert kind in(0,1)
                if kind==1:label=((label&3)-7)&65535
                if label!=ignored:output.append(label)
            ref=u64(row,32)
        return output
    assert sequence(before[0])!=sequence(before[1]),'fixture must prove genuine unequal linked WORDs'
    assert all(u16(w,50)&768==0 for w in before)
    #Two distinct selected groups, quota2, first selected pinned: no payload
    #swap, pruning or alternate first-group selection is reachable here.
    assert rf(before[0],16)>rf(before[1],16)
    delta=f32(rf(before[1],16)-rf(before[0],16));mass=f32(math.exp(delta))if delta>=-75 else 0.0
    p=f32(1/f32(1+mass));weights=[p,f32(mass*p)]
    minimum=min(rf(w,28)for w in before)
    for i,(a,b)in enumerate(zip(after,before)):
        assert a[:16]==b[:16]and a[24:]==b[24:]
        close(rf(a,16),weights[i]);close(rf(a,20),f32(math.exp(-f32(rf(b,28)-minimum))))
    compact=dec(c['compact']);assert len(compact)%40==0 and all(not compact[i+32]&8 for i in range(0,len(compact),40))
    source=dec(c['sources26']);assert len(source)%26==0 and len(source)>0
    assert len(c['textChoices'])==2 and all(len(word)>0 for word in c['textChoices'])


def integer(v,lo=0,hi=2**63-1):
    assert type(v)is int and lo<=v<=hi
    return v

def strict(a,b):
    assert type(a)is type(b)
    if type(a)is dict:
        assert set(a)==set(b)
        for k in b:strict(a[k],b[k])
    elif type(a)is list:
        assert len(a)==len(b)
        for x,y in zip(a,b,strict=True):strict(x,y)
    else:assert a==b

def validate(samples):
    assert type(samples)is list and len(samples)==3
    done=refused=active=positive=0
    for c in samples:
        assert type(c)is dict
        compact=base64.b64decode(c['compact'],validate=True);assert len(compact)>0 and len(compact)%40==0
        count=len(compact)//40;bit8=sum(bool(compact[i*40+32]&8)for i in range(count));assert integer(c['bit8Rows'])==bit8
        if bit8:
            assert set(c)=={'branch','bit8Rows','count','compact','before','after','guards','completionObjectCreated','sourceRunCalls','sourcePrefixCompleted','sources26','sourcesAfter26','pool','poolAfter','refusals'}
            assert c['branch']=='positive-bit8-refused'and integer(c['count'])==count and integer(c['guards'])==4
            assert c['completionObjectCreated']is False and integer(c['sourceRunCalls'])==0 and c['sourcePrefixCompleted']is False
            strict(c['before'],c['after']);assert set(c['before'])=={'words','selected','refs'}
            assert c['before']['selected']==[0,1] and all(type(v)is int for v in c['before']['selected'])
            assert len(c['before']['words'])==len(c['before']['refs'])==2
            for word,refs in zip(c['before']['words'],c['before']['refs'],strict=True):
                raw=base64.b64decode(word,validate=True);assert len(raw)==120 and type(refs)is list and len(refs)>0
                assert struct.unpack_from('<I',raw,40)[0]==len(refs)
                for r in refs:integer(r,0,2**64-1)
            source=base64.b64decode(c['sources26'],validate=True);assert len(source)>0 and len(source)%26==0 and c['sourcesAfter26']==c['sources26']
            strict(c['pool'],c['poolAfter']);assert type(c['pool'])is list and len(c['pool'])>0
            keys=set()
            for row in c['pool']:
                assert set(row)=={'key','records'};key=integer(row['key'],0,2**64-1);assert key not in keys;keys.add(key)
                data=base64.b64decode(row['records'],validate=True);assert len(data)>0 and len(data)%40==0
            assert type(c['refusals'])is list and len(c['refusals'])==2
            for e in c['refusals']:strict(e,{'type':'System.NotSupportedException','message':ENABLED})
            refused+=1
        else:
            assert set(c)=={'branch','bit8Rows','before','after','pool','compact','comparisons','quota','sources26','textChoices'}
            assert c['branch']=='zero-bit8-completed'
            integer(c['comparisons']);integer(c['quota'])
            #Historical linked-label/ranking/all120B/source/glyph checks are unchanged.
            completed(c);done+=1
        active+=count;positive+=bit8
    return {'ActualInputs':3,'CompletedZeroBit8':done,'RefusedPositiveBit8':refused,'ActiveCompactRows':active,'PositiveBit8Rows':positive,'ZeroBit8CompletionExercised':done>0,'Scope':'TEST_DATA_TWO_DOMAINS_NOT_AUGMENTATION_OR_ACCEPTANCE'}

if __name__=='__main__':
    assert len(sys.argv)==2
    print(json.dumps(validate(json.loads(Path(sys.argv[1]).read_bytes())),sort_keys=True))
