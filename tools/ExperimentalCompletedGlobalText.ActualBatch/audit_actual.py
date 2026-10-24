"""Independent DATA audit; no inference, labels, or acceptance-policy changes."""
import base64,copy,hashlib,importlib.util,json,math,pathlib,struct,sys,unicodedata
R,P,C,O=map(pathlib.Path,sys.argv[1:]);assert not O.exists()
plan=json.loads(P.read_bytes());pins={p['name']:p for p in plan['files']}
assert len(pins)==19
for p in pins.values():
 b=pathlib.Path(p['path']).read_bytes();assert len(b)==p['bytes'] and hashlib.sha256(b).hexdigest()==p['sha256']
ak=pathlib.Path(pins['ak']['path']).read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
spec=importlib.util.spec_from_file_location('alphabet',R/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py');module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);alphabet=module.read_alphabet(ak)
def f(bits):return struct.unpack('<f',struct.pack('<i',bits))[0]
def check(x):
 assert x['CompletedPublication']is True and len(x['Samples'])==9
 accepted=[];rejected=[];glyphs=0;times=[]
 for i,s in enumerate(x['Samples']):
  track=json.loads(pathlib.Path(pins['track'+str(i)]['path']).read_bytes());n=len(track['points'])
  assert s['Index']==i and s['Points']==n and 2<=n<=1024
  d=s['Completed'];assert d['SourceInvocations']==1 and d['Route']=='augmentation'
  assert d['CachedAcousticEvaluations']==d['CurrentAcousticEvaluations']>0
  expected=[{'First':3,'Last':((n-1)<<16)|0xff05}]
  assert d['FullCoverage']is True and d['WordOriginalRanges']==d['FullInputRanges']==expected
  assert d['SelectedTopCount']==1 and 1<=d['Actual275PayloadCount']<=4096
  assert d['AllWordChoicesPublished']is True and type(d['PublishedWordChoiceCount'])is int
  assert d['PublishedWordChoiceCount']==d['Actual275PayloadCount']==2 and d['SelectedWordOrdinal']==0
  assert type(d['AllWordEnumeratedArrayEnvelopeBytes'])is int and 0<d['AllWordEnumeratedArrayEnvelopeBytes']<=128*1024*1024
  assert s['ExperimentalCandidateScore']is True and not d['Utf8CaptureTruncated']
  assert 1<=len(d['Glyphs'])<=128 and len(s['FirstChance'])<=256
  parts=[];scores=[];total=0
  for g in d['Glyphs']:
   row=base64.b64decode(g['PoolRecord40'],validate=True);assert len(row)==40
   assert struct.unpack_from('<H',row,4)[0]&7==0
   bits=struct.unpack_from('<i',row,16)[0];assert bits==g['NativeScore16Bits']
   score=f(bits);assert math.isfinite(score)
   label=struct.unpack_from('<H',row,8)[0];assert label in alphabet
   raw=base64.b64decode(g['SelectedTextUtf8'],validate=True);text=raw.decode('utf-8')
   assert text==alphabet[label] and text and not any(c.isspace()or unicodedata.category(c)in ['Cc','Cf','Co','Cs']for c in text)
   parts.append(text);scores.append(score);total+=len(raw);glyphs+=1
  assert d['CapturedUtf8Bytes']==total<=1024
  if s['Text']:
   assert d['Stage']=='admitted' and d['Reason']=='accepted' and all(.5<=v<=1 for v in scores)
   assert s['Text']==''.join(parts) and f(s['ScoreBits'])==min(scores) and s['Frames']>0
   accepted.append(i)
  else:
   assert d['Stage']=='glyph-host-policy' and d['Reason']=='glyph-score-outside-half-to-one'
   assert all(.5<=v<=1 for v in scores[:-1]) and not(.5<=scores[-1]<=1)
   assert s['ScoreBits']==0 and s['Frames']==0;rejected.append(i)
  assert d['ExceptionType']is None and d['ExceptionMessage']is None
  assert 0<s['Milliseconds']<2000;times.append(s['Milliseconds'])
 return dict(status='PASS_DATA_ONLY',samples=9,acceptedIndices=accepted,scoreRejectedIndices=rejected,sourceInvocations=9,noAdditionalAcousticEvaluations=True,fullOriginalCoverage=9,rawAlphabetGlyphsExact=glyphs,observedMinMilliseconds=min(times),observedMaxMilliseconds=max(times),scope='Fixed nine-recording completed public2s path; no targets/OCR accuracy or general latency claim')
x=json.loads(C.read_bytes());report=check(x);negative=[]
mutations=[('extra-inference',lambda q:q['Samples'][0]['Completed'].__setitem__('SourceInvocations',2)),('extra-acoustic',lambda q:q['Samples'][0]['Completed'].__setitem__('CurrentAcousticEvaluations',999)),('missing-tail',lambda q:q['Samples'][0]['Completed']['WordOriginalRanges'][0].__setitem__('Last',3)),('text-not-current',lambda q:q['Samples'][0].__setitem__('Text','x')),('confidence-bits',lambda q:q['Samples'][0].__setitem__('ScoreBits',0)),('native-score-bits',lambda q:q['Samples'][0]['Completed']['Glyphs'][0].__setitem__('NativeScore16Bits',0)),('reject-reason',lambda q:q['Samples'][1]['Completed'].__setitem__('Reason','accepted')),('top-vs-payload',lambda q:q['Samples'][0]['Completed'].__setitem__('SelectedTopCount',2))]
mutations += [('all-choice-mode',lambda q:q['Samples'][0]['Completed'].__setitem__('AllWordChoicesPublished',False)),('all-choice-count',lambda q:q['Samples'][0]['Completed'].__setitem__('PublishedWordChoiceCount',1)),('changed-selected-word',lambda q:q['Samples'][0]['Completed'].__setitem__('SelectedWordOrdinal',1)),('enumerated-envelope',lambda q:q['Samples'][0]['Completed'].__setitem__('AllWordEnumeratedArrayEnvelopeBytes',129*1024*1024))]
for name,mutate in mutations:
 bad=copy.deepcopy(x);mutate(bad)
 try:check(bad)
 except (AssertionError,KeyError):negative.append(name)
 else:raise AssertionError('Corruption accepted: '+name)
report['tamperControls']=negative;report['captureSHA256']=hashlib.sha256(C.read_bytes()).hexdigest()
report['allWordChoicesPublished']=9;report['wordChoiceCounts']=[s['Completed']['PublishedWordChoiceCount']for s in x['Samples']]
O.write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report))
