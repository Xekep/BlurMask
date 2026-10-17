"""Independent reverse-chain byte composition from actual configured numeric rows/scalars.

Candidate confidence/geometry are captured upstream values, not a second NN
implementation. No expected labels or transcript enters this calculation.
"""
import base64, hashlib, importlib.util, json, struct, sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('raw_alphabet',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
ak=Path(sys.argv[1]).read_bytes();alphabet=module.read_alphabet(ak)
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
samples=json.loads(Path(sys.argv[2]).read_bytes());MAX=(1<<64)-1
def u16(b,n):return struct.unpack_from('<H',b,n)[0]
def i32(b,n):return struct.unpack_from('<i',b,n)[0]
def u64(b,n):return struct.unpack_from('<Q',b,n)[0]
def w16(b,n,v):struct.pack_into('<H',b,n,v&65535)
def w32(b,n,v):struct.pack_into('<I',b,n,v&0xffffffff)
def w64(b,n,v):struct.pack_into('<Q',b,n,v&MAX)
def fb(v):return struct.unpack('<I',struct.pack('<f',v))[0]
def key(r):return r&0x010000ffffffffff
def load(nodes):
    result={}
    for n in nodes:
        data=base64.b64decode(n['records']);assert len(data)==((n['packed']&32767)>>2)*40
        result[n['key']]={'duplicate':bool(n['packed']&32768),'rows':[bytearray(data[i:i+40])for i in range(0,len(data),40)]}
    return result
def label(text):
    return next((i for i,t in alphabet.items()if t==text),65535)
bypass={label('\U000f0001'),label('\U000f0004'),label('\U000f0008'),label('\U000f0002'),label('\U000f0003'),u16(ak,107),u16(ak,109),u16(ak,111)}
created=reused=cases=rows_checked=0
for sample in samples:
    assert sample['nativeEligible']==0
    groups={g['id']:g for g in sample['groups']}
    for c in sample['cases']:
        pool=load(c['beforePool']);word=bytearray(base64.b64decode(c['beforeWord']));assert u64(word,8)==MAX
        residue=base64.b64decode(c['residue']);rows=base64.b64decode(c['rows'])
        def resolve(ref):return pool[key(ref)]['rows'][(ref>>40)&65535]
        def append(k,d,force):
            node=pool.setdefault(key(k),{'duplicate':False,'rows':[]})
            if not force and not node['duplicate']:
                for ordinal,r in enumerate(node['rows']):
                    if i32(r,0)==d:return key(k)|(ordinal<<40),True
            r=bytearray(residue);w32(r,0,d);w16(r,4,u16(r,4)&65520);w32(r,6,0xffff0000)
            w32(r,12,0x3f800000);w32(r,16,0xbf800000);w32(r,20,0);r[24:32]=bytes(8);w64(r,32,MAX)
            ref=key(k)|(len(node['rows'])<<40);node['rows'].append(r);node['duplicate']|=force
            if u64(word,8)==MAX:w64(word,8,ref)
            return ref,False
        previous=first=MAX;forcecarry=False;nc=nr=0
        for i in reversed(range(len(rows)//72)):
            row=rows[i*72:(i+1)*72];assert row[60]==0
            k=u64(row,48);d=i32(row,8)-sample['starts'][i32(row,0)]
            ref,old=append(k,d,bool(row[68]or k&(1<<39)or forcecarry))
            if old and previous!=MAX and i32(row,12)!=-1 and u64(resolve(ref),32)!=previous:ref,old=append(k,d,True)
            if old:nr+=1
            else:
                r=resolve(ref);w64(r,32,previous);forcecarry=True
                symbol=u16(row,58);emitted=symbol
                if symbol==193:emitted=label("'")
                elif symbol==194:emitted=label('"')
                if symbol==u16(ak,105)or((symbol+9)&65535)<=5:s0=s1=0x3f800000;cost=0
                else:
                    g=groups[sample['graph'][i32(row,0)][u16(row,4)]];lookup={s:(cost,score)for s,cost,score in g['candidates']}
                    score_symbol={193:label("'"),194:label('"')}.get(symbol,symbol)
                    cost,s1=lookup.get(score_symbol,(0x4000000,0));s0=fb(row[62]/255)
                    if score_symbol==u16(ak,99):cost=g['fallback']
                    if cost<0x4000000 and score_symbol not in bypass:cost=(cost+g['geometry'])&0xffffffff
                w16(r,4,((u16(r,4)&65528)&65527)|(8 if row[61]else 0));w16(r,6,0);w16(r,8,emitted)
                w32(r,12,s0);w32(r,16,s1);w32(r,20,cost)
                if first==MAX:first=ref
                nc+=1
            previous=ref;rows_checked+=1
        w64(word,8,previous)
        assert (first,previous,nc,nr)==(c['firstNew'],c['last'],c['created'],c['reused'])
        assert word==base64.b64decode(c['afterWord']),'WORD120 differs'
        expected=load(c['afterPool']);assert set(pool)==set(expected)
        for k in pool:assert pool[k]==expected[k],f'full40-byte owner {k:x} differs'
        created+=nc;reused+=nr;cases+=1
assert (len(samples),cases,created,reused)==(6,48,118,30)
print(f'PASS independent {cases} fullWORD/pool connections/{rows_checked} rows/{created} new/{reused} reused; upstream scalar composition, no NN/transcript claim.')
