"""DATA-only all275 publication audit; derive symbols from authentic raw AK.
No expected handwriting labels, source issuing, model evaluation or vendor load.
"""
import base64,copy,hashlib,importlib.util,json,pathlib,struct,sys
R,P,C,O=map(pathlib.Path,sys.argv[1:]);assert not O.exists()
plan=json.loads(P.read_bytes());pins={x['name']:x for x in plan['files']}
assert len(pins)==11
for pin in pins.values():
    b=pathlib.Path(pin['path']).read_bytes()
    assert len(b)==pin['bytes'] and hashlib.sha256(b).hexdigest()==pin['sha256']
ak=pathlib.Path(pins['ak']['path']).read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
spec=importlib.util.spec_from_file_location('all275_raw_alphabet',R/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);alphabet=module.read_alphabet(ak)
track=json.loads(pathlib.Path(pins['track']['path']).read_bytes());point_count=len(track['points'])
assert 2<=point_count<=4096
original=[(3,((point_count-1)<<16)|0xff05)]
D=lambda s:base64.b64decode(s,validate=True)
I32=lambda b,o:struct.unpack_from('<i',b,o)[0]
U16=lambda b,o:struct.unpack_from('<H',b,o)[0]
U32=lambda b,o:struct.unpack_from('<I',b,o)[0]

def position(value):
    if type(value)is int:return value
    assert type(value)is dict and set(value)=={'Value','SourceIndex','PointIndex','Fraction','Mode'}
    v=value['Value'];assert type(v)is int and 0<=v<2**64
    assert (value['SourceIndex'],value['PointIndex'],value['Fraction'],value['Mode'])==(v>>32,(v>>16)&65535,(v>>8)&255,v&255)
    return v

def ranges(rows):
    assert type(rows)is list
    return [(position(row['First']),position(row['Last'])) for row in rows]

def check(a):
    assert type(a)is dict and type(a['NNSourceInvocations'])is int and a['NNSourceInvocations']==1
    assert type(a['AcousticEvaluations'])is int and a['AcousticEvaluations']>0 and a['ActualBit8']>0
    before,after,t=a['BeforeText'],a['AfterText'],a['Text']
    assert before['Words']==after['Words'] and before['SelectedOrdinals']==after['SelectedOrdinals']==[0]
    assert before['First']==after['First'] and before['Last']==after['Last']
    assert t['SelectedWordIndex']==0 and t['SelectedTopCount']==1
    assert len(before['Words'])==len(t['Actual275'])==t['Actual275PayloadCount']==t['ChoiceCount']==2
    assert t['Actual275']==[row['Storage120'] for row in before['Words']]
    before_pool={n['Key']:D(n['Records']) for n in before['Pool']}
    after_pool={n['Key']:D(n['Records']) for n in after['Pool']}
    assert len(before_pool)==len(before['Pool']) and before_pool.keys()==after_pool.keys()
    for node in before['Pool']+after['Pool']:
        b=D(node['Records']);count=len(b)//40
        assert len(b)%40==0 and 0<count<=2046
        assert ((node['PackedCount']<<2)&65535)>>4==count
    for key,b in before_pool.items():assert after_pool[key][:len(b)]==b
    def raw(ref):
        assert type(ref)is int and 0<=ref<2**64
        key=ref&0x010000ffffffffff;index=(ref>>40)&65535
        assert key in before_pool and index<32768 and index<len(before_pool[key])//40
        return before_pool[key][index*40:index*40+40]
    choices=t['AllChoices'];assert len(choices)==2
    all_flags=0;total_refs=0;selected_bytes=None;shared={}
    for ordinal,(word,choice) in enumerate(zip(before['Words'],choices)):
        assert type(choice['WordOrdinal'])is int and choice['WordOrdinal']==ordinal and choice['IsComplete']is True
        refs=word['FlatReferences'];links=choice['Links'];assert 1<=len(refs)==len(links)<=4096
        word_bytes=D(word['Storage120']);assert len(word_bytes)==120
        assert I32(word_bytes,40)==len(refs) and I32(word_bytes,44)>=len(refs)
        expected=[];flags=0
        for ref,link in zip(refs,links):
            assert link['Reference']==ref
            source=raw(ref);assert U16(source,4)&7==0
            label=U16(source,8);assert label in alphabet
            text=alphabet[label]
            # Explicit current ordinary Cyrillic scalar fixture, not a generic
            # Unicode normalization or learned-language oracle.
            assert len(text)==1 and 0x400<=ord(text)<=0x4ff
            encoded=text.encode('utf-8');assert D(link['SelectedUtf8'])==encoded
            marker=link['Marker'];records=link['Records']
            assert type(marker)is int and 0<=marker<len(records)<=65536
            scalar24=D(link['Scalar24']);want24=bytearray(24);struct.pack_into('<i',want24,16,marker)
            assert scalar24==want24
            assert D(records[marker]['Utf8'])==encoded
            for row in records:
                scalar40=D(row['Scalar40']);assert len(scalar40)==40
                assert scalar40[:8]==bytes(8) and scalar40[12:32]==bytes(20) and scalar40[36:]==bytes(4)
                assert U32(scalar40,32)==len(D(row['Utf8']))
            marker_flags=U32(D(records[marker]['Scalar40']),8)
            # Admitted native Field40=-1 uses this same valid marker for the
            # default full OR; subsequent bit20 fold cannot clear prior flags.
            flags|=marker_flags
            identity=link['SourceIdentity'];assert type(identity)is int and identity>0
            if identity in shared:assert shared[identity]==records
            else:shared[identity]=records
            expected.append(encoded)
        assert type(choice['Flags'])is int and choice['Flags']==flags
        scalar48=D(choice['Scalar48']);want48=bytearray(48)
        struct.pack_into('<I',want48,8,flags);struct.pack_into('<ii',want48,24,len(refs),len(refs))
        assert scalar48==want48 and ranges(choice['OrdinaryRanges'])==original
        if ordinal==0:
            selected_bytes=b''.join(expected)
            assert t['ChoiceFlags']==flags
        all_flags|=flags;total_refs+=len(refs)
    assert t['Flags']&all_flags==all_flags
    assert t['AllWordNativeChoiceAndLinkLayoutBytes']==48*len(choices)+24*total_refs
    assert t['AllWordReferenceArrayCopyBytes']==8*total_refs
    assert 8160*total_refs<=t['AllWordEnumeratedArrayEnvelopeBytes']<=128*1024*1024
    assert D(t['SelectedUtf8'])==selected_bytes and t['SelectedText']==selected_bytes.decode('utf-8')
    assert len(t['Glyphs'])==len(choices[0]['Links'])
    for glyph,link in zip(t['Glyphs'],choices[0]['Links']):
        assert all(glyph[k]==link[k] for k in ('Reference','Marker','SelectedUtf8','Records'))
    for key in ('WordOriginalRanges','TextRanges','OriginalNullTextRanges'):assert ranges(t[key])==original
    lines=D(t['SourceLines56']);assert len(lines)>0 and len(lines)%56==0
    assert t['Pending'][-1]['Name']=='TEXT' and t['Pending'][-1]['SelectionId']==t['TextSelection']
    assert all(e.get('Pass',True)is True for e in a['Evidence'])
    assert t['Trace'].count('text-registered')==1
    for i in range(2):assert t['Trace'].count('word-begin:'+str(i))==t['Trace'].count('word-complete:'+str(i))==1
    return dict(status='PASS_DATA_ONLY',actualWords=2,selectedTop=1,characters=total_refs,
                sharedCharSources=len(shared),allRawAlphabetBindingsExact=True,originalRangesExact=True,
                existingWordsAndPoolPrefixesUnchanged=True,modelExecutions=0,
                scope='All current ordinary Cyrillic WORD choices under one TOP; not cursive accuracy or general normalization')

capture=json.loads(C.read_bytes());report=check(capture);rejected=[]
mutations=[
    ('choice-order',lambda q:q['Text']['AllChoices'].reverse()),
    ('choice-count',lambda q:q['Text'].__setitem__('ChoiceCount',1)),
    ('marker',lambda q:q['Text']['AllChoices'][1]['Links'][0].__setitem__('Marker',-1)),
    ('reference',lambda q:q['Text']['AllChoices'][1]['Links'][0].__setitem__('Reference',0)),
    ('text',lambda q:q['Text']['AllChoices'][1]['Links'][0].__setitem__('SelectedUtf8','AA==')),
    ('flags',lambda q:q['Text']['AllChoices'][1].__setitem__('Flags',q['Text']['AllChoices'][1]['Flags']^0x100000)),
    ('scalar',lambda q:q['Text']['AllChoices'][1].__setitem__('Scalar48','AA==')),
    ('range',lambda q:q['Text']['AllChoices'][1]['OrdinaryRanges'][0].__setitem__('First',0)),
    ('top',lambda q:q['Text'].__setitem__('SelectedTopCount',2)),
    ('layout',lambda q:q['Text'].__setitem__('AllWordNativeChoiceAndLinkLayoutBytes',0)),
    ('selected',lambda q:q['Text'].__setitem__('SelectedUtf8','AA==')),
    ('words',lambda q:q['AfterText']['Words'][1].__setitem__('Storage120','AA==')),
    ('NN',lambda q:q.__setitem__('NNSourceInvocations',2))]
for name,mutate in mutations:
    bad=copy.deepcopy(capture);mutate(bad)
    try:check(bad)
    except (AssertionError,KeyError,IndexError,struct.error):rejected.append(name)
    else:raise AssertionError('Accepted corruption: '+name)
report.update(captureSHA256=hashlib.sha256(C.read_bytes()).hexdigest(),tamperControls=rejected)
O.write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report))
