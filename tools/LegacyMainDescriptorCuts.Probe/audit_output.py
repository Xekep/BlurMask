"""Independent raw float, selected-domain, source-map and point-key audit.
This audits composition around the already ported cut arithmetic; it does not
claim an independent full loop/horizontal/baseline cut algorithm or NN accuracy.
"""
import base64,hashlib,json,math,pathlib,struct,sys
ak=pathlib.Path(sys.argv[1]).read_bytes();data=json.loads(pathlib.Path(sys.argv[2]).read_text())
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert ak[188216]==6 and struct.unpack_from('<f',ak,188217)[0]==4
assert ak[189395]==8 and list(ak[189728:189730])==[1,0]
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import parse_alphabet
alphabet=parse_alphabet(ak,99)
assert alphabet['label_count']==192 and '\U000f0009' not in alphabet['mapping'].values()
def f(x):return struct.unpack('<f',struct.pack('<I',x&0xffffffff))[0]
def bits(x):return struct.unpack('<i',struct.pack('<f',x))[0]
def i(b,o):return struct.unpack_from('<i',b,o)[0]
def flat(v):return [x for s in v for x in s]
records=maps=identities=separators=nonzero_prefix=inserts=0
for test in data['prepared']+data['actual']:
    current=[s[:] for s in test['source']];prior=test['priorMap'] or [test['factor']*n for n in range(len(flat(current)))];pending=[];next_physical=0
    ds=[base64.b64decode(v) for v in test['descriptors']];gs=[base64.b64decode(v) for v in test['guides']]
    assert len(current)==len(test['probabilities']) and len(ds)==len(test['steps'])
    #Keep the original before-map for an independent ceil-to-metadata lookup.
    key_by_metadata={row[3]:row[:5] for row in test['beforeIdentities']}
    for ordinal,(d,s) in enumerate(zip(ds,test['steps'])):
        assert s['ordinal']==ordinal and s['kind']==(struct.unpack_from('<H',d)[0]&15)
        assert s['beforeCounts']==list(map(len,current))
        if s['kind']==3:
            assert struct.unpack_from('<q',d,32)[0]==-1
            assert not s['records'] and not s['markers'] and not s['atoms'] and not s['incoming'] and not s['composed']
            assert s['afterCounts']==s['beforeCounts'];pending.append(ordinal);separators+=1;continue
        first,last,line=i(d,36),i(d,40),i(d,16)
        assert s['first']==first==next_physical and s['last']==last and s['line']==line
        assert s['baselineBits']==i(gs[line],0) and s['heightBits']==i(gs[line],4)
        assert s['pending']==pending;pending=[];next_physical=last+1
        before=flat(current);prefix=sum(map(len,current[:first]));end=sum(map(len,current[:last+1]));rows=s['records']
        assert len(rows)==sum(s['afterCounts'][first:last+1]);inserts+=len(rows)-(end-prefix)
        if prefix:nonzero_prefix+=1
        #No raw XY is lost or changed. Inserted records repeat a previous raw
        #index; every original selected point still occurs with its exact bits.
        assert rows[0][7]==prefix and rows[-1][7]==end-1
        for n in range(prefix,end):assert any(row[7]==n and row[:2]==before[n] for row in rows)
        assert [r[7] for r in rows]==sorted(r[7] for r in rows)
        expected_markers=[];at=0
        for physical in range(first,last+1):
            count=s['afterCounts'][physical]
            for local,row in enumerate(rows[at:at+count]):
                assert row[6]==physical
                if row[5]!=0 and not 11<=row[5]<=14:expected_markers.append([row[5],physical,local])
            at+=count
        assert s['markers']==expected_markers
        for n,row in enumerate(rows):
            if n+1==len(rows):expected=[0,0,0]
            else:
                dx=f(bits(f(rows[n+1][0])-f(row[0])));dy=f(bits(f(rows[n+1][1])-f(row[1])))
                expected=[bits(dx),bits(dy),bits(math.sqrt(dx*dx+dy*dy))]
            assert row[2:5]==expected,(ordinal,n,row[2:5],expected)
            records+=1
        expected_atoms=[]
        for n,(kind,physical,start) in enumerate(expected_markers):
            stop=expected_markers[n+1][2]+1 if n+1<len(expected_markers) and expected_markers[n+1][1]==physical else s['afterCounts'][physical]
            expected_atoms.append([kind,physical,start,stop])
        assert s['atoms']==expected_atoms
        incoming=list(range(prefix))+[r[7] for r in rows]+list(range(end,len(before)))
        assert s['incoming']==incoming
        #incomingFactor1 selects the previous source map directly. A later
        #descriptor addresses this newly composed domain, not initial XY offsets.
        prior=[prior[n] for n in incoming];assert s['composed']==prior;maps+=len(prior)
        xy=[r[:2] for r in rows];at=0
        for physical in range(first,last+1):
            count=s['afterCounts'][physical];current[physical]=xy[at:at+count];at+=count
        assert s['afterCounts']==list(map(len,current))
    assert next_physical==len(current) and test['trailing']==pending
    assert current==test['final'] and prior==test['finalMap']
    assert len(prior)==len(test['afterIdentities'])
    for tick,actual in zip(prior,test['afterIdentities']):
        whole,remainder=divmod(tick,test['factor']);index=whole+bool(remainder)
        if test['keys']:key=test['keys'][index];record_index=index
        else:
            #Actual source metadata is inherited from its genuine ingress. Do
            #not infer raw point ordinals from current/deduplicated ordinals.
            previous=key_by_metadata[index];key=previous[:3];record_index=previous[4]
        assert actual==key+[index,record_index,(test['factor']-remainder)&255 if remainder else 0]
        identities+=1
assert data['preparedCount']==192 and data['actualCount']==7 and data['multiple']>=1 and data['singleParity']>=1
assert data['guards']>=27 and separators and inserts and nonzero_prefix
print(f'PASS independent {records} raw-float records/{maps} map entries/{identities} point identities;{separators} pending separators/{nonzero_prefix} nonzero-prefix replacements/{inserts} insertions; actual source ownership remains separate from a global graph')
