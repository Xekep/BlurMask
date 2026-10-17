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
samples=json.loads(Path(sys.argv[1]).read_bytes());assert len(samples)==3
rows=0
for c in samples:
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
        close(rf(a,16),weights[i]);close(rf(a,20),f32(math.exp(-f32(rf(b,28)-minimum))));rows+=1
    compact=dec(c['compact']);assert len(compact)%40==0 and all(not compact[i+32]&8 for i in range(0,len(compact),40))
    source=dec(c['sources26']);assert len(source)%26==0 and len(source)>0
    assert len(c['textChoices'])==2 and all(len(word)>0 for word in c['textChoices'])
print(f'PASS independent3 shared spans/{rows} full120B payloads/6 actual279 selections/linked unequal labels/4ULP score channels; actual bit8 candidates absent, no acceptance claim')
