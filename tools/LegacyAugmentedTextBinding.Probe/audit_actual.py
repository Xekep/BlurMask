"""DATA-only current-selected TEXT audit. No network or expected OCR labels."""
import base64,copy,hashlib,importlib.util,json,pathlib,struct,sys
R,P,C,O=map(pathlib.Path,sys.argv[1:]);assert not O.exists()
plan=json.loads(P.read_bytes());pins={x['name']:x for x in plan['files']}
for p in pins.values():
 b=pathlib.Path(p['path']).read_bytes();assert len(b)==p['bytes'] and hashlib.sha256(b).hexdigest()==p['sha256']
ak=pathlib.Path(pins['ak']['path']).read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
spec=importlib.util.spec_from_file_location('raw_alphabet',R/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py');al=importlib.util.module_from_spec(spec);spec.loader.exec_module(al);alphabet=al.read_alphabet(ak)
D=lambda s:base64.b64decode(s,validate=True)
U16=lambda b,o:struct.unpack_from('<H',b,o)[0]
U64=lambda b,o:struct.unpack_from('<Q',b,o)[0]
def check(a):
 assert a['NNSourceInvocations']==1 and a['AcousticEvaluations']>0 and a['ActualBit8']>0
 before,after,t=a['BeforeText'],a['AfterText'],a['Text']
 assert before['Words']==after['Words'] and before['SelectedOrdinals']==after['SelectedOrdinals']
 assert before['First']==after['First'] and before['Last']==after['Last']
 assert t['SelectedWordIndex']==0 and t['SelectedTopCount']==1
 assert t['Actual275PayloadCount']==len(before['Words'])==len(t['Actual275'])
 assert t['Actual275']==[w['Storage120']for w in before['Words']]
 pools={node['Key']:D(node['Records'])for node in before['Pool']}
 later={node['Key']:D(node['Records'])for node in after['Pool']}
 for key,records in pools.items():assert key in later and later[key][:len(records)]==records
 refs=before['Words'][0]['FlatReferences'];assert refs and len(refs)==len(t['Glyphs'])<=4096
 def row(ref):
  key=ref&0x010000ffffffffff;index=(ref>>40)&65535
  assert key in pools and index<32768 and index<len(pools[key])//40
  return pools[key][index*40:index*40+40]
 expected=[]
 for ref,g in zip(refs,t['Glyphs']):
  assert g['Reference']==ref
  raw=row(ref);assert U16(raw,4)&7==0
  label=U16(raw,8);assert label in alphabet
  text=alphabet[label]
  # Explicit ordinary scalar subset; this is not a generalized normalization oracle.
  assert len(text)==1 and 0x400<=ord(text)<=0x4ff
  encoded=text.encode('utf-8');assert D(g['SelectedUtf8'])==encoded
  assert 0<=g['Marker']<len(g['Records'])
  assert D(g['Records'][g['Marker']]['Utf8'])==encoded
  expected.append(encoded)
 utf8=b''.join(expected);assert D(t['SelectedUtf8'])==utf8 and t['SelectedText']==utf8.decode('utf-8')
 # WORD choice cardinality is distinct from its linked character count.
 assert t['ChoiceCount']==1
 def position(p):
  assert type(p)is dict and set(p)=={'Value','SourceIndex','PointIndex','Fraction','Mode'}
  v=p['Value'];assert type(v)is int and 0<=v<2**64
  assert (p['SourceIndex'],p['PointIndex'],p['Fraction'],p['Mode'])==(v>>32,(v>>16)&65535,(v>>8)&255,v&255)
  return v
 ranges=lambda name:[(position(r['First']),position(r['Last']))for r in t[name]]
 assert ranges('WordOriginalRanges')==[(3,4849413)]
 assert ranges('TextRanges')==ranges('OriginalNullTextRanges')==[(3,4849413)]
 assert len(D(t['SourceLines56']))>0 and len(D(t['SourceLines56']))%56==0
 assert t['Pending'][-1]['Name']=='TEXT' and t['Pending'][-1]['SelectionId']==t['TextSelection']
 assert all(e.get('Pass',True)is True for e in a['Evidence']) and len(a['Evidence'])>=15
 return dict(status='PASS_DATA_ONLY',actual275Payloads=len(before['Words']),selectedGlyphs=len(refs),selectedUtf8Bytes=len(utf8),ordinaryRawAlphabetExact=True,originalRangesExact=True,wordAndExistingPoolBytesUnchanged=True,NNExecutions=0,scope='Current selected ordinary Cyrillic scalar TEXT binding only; not all alternatives or handwriting quality')
a=json.loads(C.read_bytes());report=check(a);negative=[]
mutations=[('selected-utf8',lambda q:q['Text'].__setitem__('SelectedUtf8','AA==')),('glyph-reference',lambda q:q['Text']['Glyphs'][0].__setitem__('Reference',0)),('selected-marker',lambda q:q['Text']['Glyphs'][0].__setitem__('Marker',-1)),('payload-count',lambda q:q['Text'].__setitem__('Actual275PayloadCount',0)),('payload-data',lambda q:q['Text']['Actual275'].__setitem__(0,'')),('word-data',lambda q:q['AfterText']['Words'][0].__setitem__('Storage120','')),('word-range',lambda q:q['Text']['WordOriginalRanges'][0].__setitem__('First',0)),('text-range',lambda q:q['Text']['TextRanges'][0].__setitem__('Last',0)),('top-count',lambda q:q['Text'].__setitem__('SelectedTopCount',2)),('extra-invocation',lambda q:q.__setitem__('NNSourceInvocations',2))]
for name,mutate in mutations:
 bad=copy.deepcopy(a);mutate(bad)
 try:check(bad)
 except (AssertionError,KeyError,IndexError):negative.append(name)
 else:raise AssertionError('Corruption accepted: '+name)
report['tamperControls']=negative;report['captureSHA256']=hashlib.sha256(C.read_bytes()).hexdigest()
O.write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report))
