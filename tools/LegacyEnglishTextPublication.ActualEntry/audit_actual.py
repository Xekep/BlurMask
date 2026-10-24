"""Independent DATA replay: ONE fixed raw3-point EN all275/WORD/TEXT pending case, no NN."""
import argparse,ast,base64,copy,hashlib,json,math,struct
from pathlib import Path
HERE=Path(__file__).resolve().parent
ACTUAL='1bb645b6c029541fc9ce345d74718b836f404694ee9bf7ab646312a4bfc577d9'
PRIOR='ea0165ba621fa102441d3ae53f366c99dc9c678662d99aa5600e578d91b6c2bb'
RAW='d1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99'
SCOPE='ACTUAL_OWNING_ENGLISH_ALL275_WORD_GUIDE_NULL_SELECTED_PENDING_TEXT_NO_DOCUMENT_APPLY'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def b64(b):return base64.b64encode(b).decode()
def data(s,n=None):
 assert type(s)is str;b=base64.b64decode(s,validate=True)
 if n is not None:assert len(b)==n
 return b
def integer(n):assert type(n)is int;return n
def strict(a,b):
 assert type(a)is type(b),(type(a),type(b))
 if isinstance(b,dict):
  assert a.keys()==b.keys(),(a.keys(),b.keys())
  for k in b:strict(a[k],b[k])
 elif isinstance(b,list):
  assert len(a)==len(b),(len(a),len(b))
  for x,y in zip(a,b,strict=True):strict(x,y)
 else:assert a==b,(a,b)
def funcs(p,g):
 tree=ast.parse(p.read_bytes().decode('utf-8-sig'));exec(compile(ast.Module([n for n in tree.body if isinstance(n,ast.FunctionDef)],[]),str(p),'exec'),g)
def setup(raw,prior):
 assert sha(raw)==RAW and sha(prior)==PRIOR
 r=json.loads(raw.read_bytes())[4];assert r['Raw']==[[[0,0],[7,40],[14,0]]]and r['Resolution']==96
 p=json.loads(prior.read_bytes());assert p['Completed']is True and p['CHARCreated']is True and p['TEXTCreated']is False
 ranges=HERE/'reference_ranges.py';selection=HERE/'reference_selection.py'
 assert sha(ranges)=='dc5e998a42e4dcedbfda714d97dd58d06136af608c5f4aa5bc03bf70b89fb750'
 assert sha(selection)=='611c834b9aff303bfc1ae87cc6a7111bcbb44c7b1cee8d99ba8ac6aa477c7f89'
 g={'MASK':2**64-1};funcs(ranges,g);s={'base64':base64,'struct':struct};funcs(selection,s)
 #NULL-selected full ROOT input: no reference override, not nonnull-empty.
 key=[]
 for i,stroke in enumerate(r['Raw']):
  n=len(stroke);key=g['union'](key,[[(i<<32)|(0xff00 if n==1 else 0)|3,(i<<32)|((n-1)<<16)|0xff00|5]])
 flat=[x for pair in key for x in pair]
 def project26(encoded):
  blob=data(encoded);assert len(blob)==26 #this actual case has one complete source row.
  fields=struct.unpack_from('<6i',blob);assert fields==(0,0,0,0,0,2)and blob[24:]==bytes(2)
  #Fresh ordinary reference spans the complete original input: no override,
  #no cached/pending/known-text source. Source26 has separate24N+2N layout.
  first=fields[2];last=fields[5];n=len(r['Raw'][fields[1]])
  return [[(fields[1]<<32)|(first<<16)|3,(fields[4]<<32)|(last<<16)|(0xff00 if last==n-1 else 0)|5]]
 source_key=g['union']([],project26(p['SourceCompletion']['Source']['Storage26']))
 glyph_key=g['intersect'](project26(p['GlyphChoices'][0]['Source26']),source_key)
 source_flat=[v for pair in source_key for v in pair];glyph_flat=[v for pair in glyph_key for v in pair]
 assert p['Characters']['PerReferencePrefixes'][0]['SourcePairs']==glyph_flat
 assert p['SourceCompletion']['Source']['SourceInputs']['OriginalXY']==[[v for point in r['Raw'][0]for v in point]]
 refs=[w['FlatReferences']for w in p['SourceCompletion']['Source']['Words']]
 assert len(refs)==p['Top']['WordChoices']==2 and all(len(x)==1 for x in refs)
 assert p['Top']['SelectedWordOrdinal']==0 and len(p['Characters']['PerReferencePrefixes'])==1
 old=p['Characters']['PerReferencePrefixes'][0];assert old['Inserted']is True and old['Ordinary']is True
 #Independent native selection over inherited, previously audited owner40.
 owner=data(p['GlyphChoices'][0]['Owner40']);assert len(owner)%40==0
 choices=[];trace=['NULL-selected-inputs-prepared','document-choice32','word-intern','word-defaults'];last=None
 aggregate=0
 for ordinal,rr in enumerate(refs):
  reference=integer(rr[0]);assert reference&0x100000000000000==0
  canonical=reference&((1<<40)-1);assert canonical==refs[0][0]and canonical==p['GlyphChoices'][0]['Reference']
  marker=(reference>>40)&65535;marker=marker if marker<32768 else marker-65536;assert marker>=0
  result=s['oracle'](dict(owner=b64(owner),packed=len(owner)//40*4,marker=marker,initialCapacity=0,allocation=b64(bytes((len(owner)//40*3//2+2)*4))))
  assert result['succeeded'];selected=data(result['rows']);indices=struct.unpack('<'+'i'*(len(selected)//4),selected)
  assert len(indices)==len(old['Records40'])==2
  recordFlags=[struct.unpack_from('<I',data(z,40),8)[0]for z in old['Records40']]
  m=result['marker'];default=recordFlags[m];flags=(default&~0x100000)|((default|recordFlags[m])&0x100000)
  aggregate|=flags
  char=copy.deepcopy(old);char.update(Reference=reference,Inserted=ordinal==0,Marker=m)
  #Same key means interner reuse, not replaying registration; snapshots remain separate.
  ordinary=g['union']([],glyph_key);pairs=[x for z in ordinary for x in z]
  scalar=bytearray(48);struct.pack_into('<I',scalar,8,flags);struct.pack_into('<ii',scalar,24,1,1)
  link=bytearray(24);struct.pack_into('<i',link,16,m)
  choices.append(dict(Ordinal=ordinal,Flags=flags,OrdinaryPairs=pairs,Scalar48=b64(scalar),Links=[dict(SourceNode=old['SourceNode'],Marker=m,Scalar24=b64(link))],Characters=[char]))
  trace+=['word-choice48:'+str(ordinal),'word-choice-complete:'+str(ordinal)]
  glyph=copy.deepcopy(p['GlyphChoices'][0]);glyph.update(Reference=reference,Refreshed=p['GlyphChoices'][0]['Refreshed']if ordinal==0 else 0,Selected4=result['rows'],Marker=m)
  utf=copy.deepcopy(p['GlyphUtf8'][0]);utf.update(Reference=reference,RetainedSelection4=result['rows'],Marker=m)
  geom=copy.deepcopy(p['Geometry']);geom['Glyphs'][0].update(Reference=reference,OriginalDescriptor40=b64(owner[marker*40:(marker+1)*40])if ordinal else geom['Glyphs'][0]['OriginalDescriptor40'])
  last=dict(Geometry=dict(Glyphs=dict(Choices=dict(Top=dict(Source=p['SourceCompletion'],Top=p['Top']),Glyphs=[glyph]),Glyphs=[utf]),Geometry=geom),PerReferencePrefixes=[char],FinalNodes=p['Characters']['FinalNodes'],CharacterTreeRoot=p['Characters']['CharacterTreeRoot'],CharacterTree=p['Characters']['CharacterTree'],PendingCommands=p['Characters']['PendingCommands'])
 trace+=['word-registered']
 #Native clears200000 on every positive first-pass reference, THEN folds all48.
 flags=aggregate;degenerate=key[0][0]>>32==key[0][1]>>32 and(key[0][0]>>16)&65535==(key[0][1]>>16)&65535
 flags=(flags&~0x800000)|(0x800000 if degenerate else 0)
 simple=[dict(SelectionId=i,Name=name,CommandIdentity=i,PreviousCommandIdentity=i-1,SourcePairs=glyph_flat if i==1 else source_flat)for i,name in enumerate(['CHAR','WORD'],1)]
 word=dict(SourceNode=1,SelectedOrdinal=0,SelectionId=2,Flags=flags,Defaults=dict(Field08=-1,Field00=0,Field40=-1,Field48=-1),OriginalPairs=source_flat,Choices=choices,FinalCharacters=p['Characters']['FinalNodes'],PendingCommands=simple,Trace=trace)
 return p,flat,word,last,trace
def expected(x,p,flat,word,last,trace):
 #These are explicitly upstream INPUTS to the line replay, NOT independently
 #recomputed FIRST119 fit/deskew/shear outputs. No target-output channel is used.
 d=data(x['Text']['SourceDescriptor88'],88);guide=data(x['Text']['SourceGuide40'],40)
 assert(d[0]&15)==0 and struct.unpack_from('<i',d,16)[0]==0
 slope,baseline,height,cb,ch=[struct.unpack_from('<f',guide,k)[0]for k in [8,12,16,20,24]]
 assert all(math.isfinite(v)for v in [slope,baseline,height,cb,ch])
 #Actual original/no-corrected branch only. Prepared controls cover other branch.
 assert slope==0 and ch==0 and height>0
 line=bytearray(56)
 for k,v in [(8,-0.0*baseline),(16,float(baseline)),(24,0.0),(32,float(height)),(48,float(baseline))]:struct.pack_into('<d',line,k,v)
 struct.pack_into('<q',line,40,-1)
 def mask(n,pointer,tail=None):
  a=bytearray([255]*n);a[pointer:pointer+8]=bytes(8)
  if tail is not None:a[tail:]=bytes(n-tail)
  return b64(a)
 document=bytearray(32);struct.pack_into('<I',document,8,word['Choices'][word['SelectedOrdinal']]['Flags']&1023);struct.pack_into('<ii',document,24,1,1)
 link=bytearray(24);struct.pack_into('<i',link,16,word['SelectedOrdinal'])
 commands=[dict(SelectionId=i,Name=name,CommandIdentity=i,PreviousCommandIdentity=i-1,OperationCode=4,Enabled=1,Category=100,SourcePairs=flat)for i,name in enumerate(['CHAR','WORD','TEXT'],1)]
 text=dict(SelectionId=3,NextSelectionId=4,NullSelectedPairs=flat,NextWrapperLineOffset=1,SourceDescriptor88=b64(d),SourceGuide40=b64(guide),SourceLines56=b64(line),SourceLineKnown56=b64(bytes([255]*56)),DocumentChoice32=b64(document),DocumentChoiceKnown32=mask(32,16),WordLink24=b64(link),WordLinkKnown24=mask(24,8,21),WordChoiceKnown48=[mask(48,16)for _ in word['Choices']],CharacterLinkKnown24=[[mask(24,8)for _ in c['Links']]for c in word['Choices']],PendingCommands=commands,Trace=trace+['source-lines021AB0','TEXT-registered'],CharacterContextAliveAtRegistration=True,Scope='SAME_EN_ALL275_WORD_LINES_NULL_SELECTED_PENDING_TEXT_NO_DOCUMENT_APPLY')
 names=['Completed','InputSha','OriginalIndex','RawPoints','RawStrokes','Resolution','GraphNodes','ParentChildren','Stages','Metrics','SourceCompletion','Top','Vector10','ParentPreparationMayRunLineModels','NoExpectedLabels','Admission','GlobalReadinessGateUnchanged','TimeoutMs']
 want={k:copy.deepcopy(p[k])for k in names}
 want.update(Scope=SCOPE,Word=word,Text=text,LastChoiceCharacters=last,FirstChoiceCharacterPrefixes=p['Characters']['PerReferencePrefixes'],CHARCreated=True,WORDCreated=True,TEXTCreated=True,DocumentApplied=False,PriorCharSha=PRIOR,PriorCanonicalFieldsMatched=True,OldCharFirstChoicePrefixMatched=True,LaterFinalPoolEqualityClaimed=False,LifecycleScope='ONE_NORMAL_OWNING_PASS_NO_EXTRA_CANCEL_OR_POISON_INJECTION',NoRetry=True,CharacterContextAliveAtRegistrationSourceWitness=True)
 return want
def audit(x,state):strict(x,expected(x,*state))
def tampers(x,state):
 changes=[]
 def add(name,f):changes.append((name,f))
 for k,v in [('Scope','TEXT_ACCEPTED'),('DocumentApplied',True),('OriginalIndex',True),('RawPoints',3.0),('NoRetry',False),('LaterFinalPoolEqualityClaimed',True),('CharacterContextAliveAtRegistrationSourceWitness',False)]:add(k,lambda q,k=k,v=v:q.update({k:v}))
 add('source-prefix',lambda q:q['SourceCompletion']['Source'].update(Appended=0));add('NN-vector',lambda q:q.update(Vector10=[]));add('metrics',lambda q:q.update(Metrics={}))
 for k,v in [('SourceNode',2),('SelectedOrdinal',1),('SelectionId',1),('Flags',1),('OriginalPairs',[])]:add('WORD-'+k,lambda q,k=k,v=v:q['Word'].update({k:v}))
 add('WORD-default',lambda q:q['Word']['Defaults'].update(Field40=0));add('missing-choice',lambda q:q['Word']['Choices'].pop());add('extra-choice',lambda q:q['Word']['Choices'].append(copy.deepcopy(q['Word']['Choices'][0])))
 for k,v in [('Ordinal',0),('Flags',1),('OrdinaryPairs',[])]:add('choice-'+k,lambda q,k=k,v=v:q['Word']['Choices'][1].update({k:v}))
 for k,v in [('Reference',0),('Inserted',True),('Marker',0),('SourcePairs',[]),('SelectionId',2),('Records40',[]),('RecordKnown40',[]),('Utf8',[])]:add('reuse-'+k,lambda q,k=k,v=v:q['Word']['Choices'][1]['Characters'][0].update({k:v}))
 add('char-link-marker',lambda q:q['Word']['Choices'][1]['Links'][0].update(Marker=0));add('late-final-pool',lambda q:q['Word']['FinalCharacters'].pop());add('historical-prefix',lambda q:q['FirstChoiceCharacterPrefixes'][0].update(Inserted=False))
 for k,v in [('SelectionId',2),('NextSelectionId',3),('NullSelectedPairs',[]),('NextWrapperLineOffset',0),('CharacterContextAliveAtRegistration',False)]:add('TEXT-'+k,lambda q,k=k,v=v:q['Text'].update({k:v}))
 for k,v in [('OperationCode',5),('Enabled',0),('Category',99),('SelectionId',2),('PreviousCommandIdentity',0),('Name','WORD'),('SourcePairs',[])]:add('command-'+k,lambda q,k=k,v=v:q['Text']['PendingCommands'][2].update({k:v}))
 add('extra-command',lambda q:q['Text']['PendingCommands'].append(q['Text']['PendingCommands'][0]));add('command-order',lambda q:q['Text']['PendingCommands'].reverse())
 def byte(q,where,key,offset):
  obj=q['Text']if where=='Text'else q['Word']['Choices'][1]if where=='Choice'else q['Word']['Choices'][1]['Links'][0];b=bytearray(data(obj[key]));b[offset]^=255;obj[key]=b64(b)
 for offset in [8,16,24,32,40,48]:add('line-byte'+str(offset),lambda q,o=offset:byte(q,'Text','SourceLines56',o))
 for key,offset in [('DocumentChoice32',24),('WordLink24',16),('DocumentChoiceKnown32',16),('WordLinkKnown24',8),('WordLinkKnown24',21),('SourceLineKnown56',0)]:add(key+str(offset),lambda q,k=key,o=offset:byte(q,'Text',k,o))
 add('choice-scalar48',lambda q:byte(q,'Choice','Scalar48',24));add('link-scalar24',lambda q:byte(q,'Link','Scalar24',16))
 def maskbyte(q,k,a,b=None):
  obj=q['Text'][k];raw=bytearray(data(obj[a]if b is None else obj[a][b]));raw[16 if b is None else 8]^=255
  if b is None:obj[a]=b64(raw)
  else:obj[a][b]=b64(raw)
 add('choice-known-pointer',lambda q:maskbyte(q,'WordChoiceKnown48',1));add('CHAR-known-pointer',lambda q:maskbyte(q,'CharacterLinkKnown24',1,0))
 def guide(q):
  b=bytearray(data(q['Text']['SourceGuide40']));struct.pack_into('<f',b,12,41);q['Text']['SourceGuide40']=b64(b)
 add('guide-alone-no-line-update',guide)
 def descriptor(q):
  b=bytearray(data(q['Text']['SourceDescriptor88']));struct.pack_into('<i',b,16,1);q['Text']['SourceDescriptor88']=b64(b)
 add('wrong-assigned-line',descriptor)
 names=[]
 for name,mut in changes:
  q=copy.deepcopy(x);mut(q)
  try:audit(q,state)
  except(AssertionError,KeyError,TypeError,ValueError):names.append(name)
  else:raise AssertionError('accepted mutation '+name)
 return names
def main():
 p=argparse.ArgumentParser();p.add_argument('actual',type=Path);p.add_argument('prior_char',type=Path);p.add_argument('raw',type=Path);p.add_argument('report',type=Path);p.add_argument('--tamper',action='store_true');a=p.parse_args()
 assert not a.report.exists()and sha(a.actual)==ACTUAL and a.actual.stat().st_size==28374
 x=json.loads(a.actual.read_bytes());state=setup(a.raw,a.prior_char);audit(x,state)
 report=dict(Passed=True,Scope='ONE_PINNED_RAW3POINT_PENDING_WORD_TEXT_DATA_NOT_DOCUMENT_OR_QUALITY',ActualSha256=ACTUAL,PriorCanonicalAndHistoricalChoice0Matched=True,WordChoices=2,CharacterNodes=1,ReusedCharacterChoices=1,RegistrationOrder=['CHAR','WORD','TEXT'],LineBytes=56,WordScalarBytes=96,CharacterLinkBytes=48,DocumentAndWordLinkBytes=56,PointerKnownMasksVerified=True,NullSelectedSourcePairs=1,NN=False,DocumentApplied=False,
 NotIndependentlyVerified=['Upstream FIRST119 line-fit/deskew/shear and finaldescriptor88/guide40 production: initial FIRST published rows/postprocessing snapshots absent. Exported private frame is explicit INPUT to native1692B0/021AB0 formula; coupled valid guide+line corruption is outside this DATA oracle.','Live private owner/epoch/CHAR lifetime/cancellation/permanent poison/once semantics: source-reviewed, not provable from detached DATA booleans.','Exact actual count/fill call chronology, lazy refresh calls and unused cumulative-ledger totals: not exported success diagnostics.','Native pointers/stack padding/allocator origin; only masks are checked, scalar zero templates do not prove native pointer values.','New NN or handwriting-quality proof; inherited prior ea0165 is pinned independently-audited evidence, not replayed inference.','025E40 coverage/document apply/public transcription/admission/general Unicode/multiWORD/multi-input paths remain outside this scope.'])
 if a.tamper:report['TamperRefused']=tampers(x,state)
 a.report.write_bytes((json.dumps(report,indent=2)+'\n').encode());print(json.dumps(report))
if __name__=='__main__':main()
