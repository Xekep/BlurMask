"""Independent actual-compact two-domain test oracle; no candidate/NN replay or transcription."""
import base64,hashlib,importlib.util,json,struct,sys
from pathlib import Path
SCOPE='ACTUAL_NO_CANDIDATE_TWO_DOMAIN_TEST_NO_DISCARDED_BIT8'
PREPARED='PREPARED_TWO_DOMAIN_SCHEMA_CONTROLS_NOT_ACTUAL'
ENABLED='Enabled219580 candidate augmentation needs its actual218A30/218560/WORD owner; no candidate is discarded.'
PRODUCTION_LF='64234f92afd88191193388bbd0719a7fa7f4fb319dea5dc83eead92e3c35cdf6'

def integer(v,lo=0,hi=2**63-1):
 assert type(v)is int and lo<=v<=hi
 return v

def decoded(v):
 assert type(v)is str
 return base64.b64decode(v,validate=True)

def validate(data,scope=SCOPE):
 assert set(data)=={'scope','inputMode','inputs','guards','activeRows','completedInputs','refusedInputs','actualInputs'}
 assert data['scope']==scope and type(data['inputs'])is list
 n=len(data['inputs']);assert n>0
 if scope==SCOPE:
  assert n in(7,9)and data['inputMode']in('generated-seven','recorded')
  if data['inputMode']=='generated-seven':assert n==7
 else:assert scope==PREPARED and data['inputMode']=='prepared-schema'
 assert integer(data['actualInputs'])==n
 done=refused=rows=sources=guards=0
 fields={'input','branch','bit8Rows','count','compact','before','after','sources26','sourcesAfter26','sourceCount','sourceCountAfter','completionObjectCreated','completed','sourceRunCalls','validResourcePrepareCalls','guards','wordIndexBefore','wordIndexAfter','headUnchanged','sourcePrefixTopCompleted','references','afterReferences','refusals','completion'}
 for index,case in enumerate(data['inputs']):
  assert type(case)is dict and set(case)==fields
  assert integer(case['input'])==index
  count=integer(case['count'],1,65536);compact=decoded(case['compact']);assert len(compact)==count*40
  positive=sum(bool(compact[i*40+32]&8)for i in range(count))
  assert integer(case['bit8Rows'])==positive
  before=decoded(case['before']);after=decoded(case['after']);assert len(before)==len(after)==120
  refs=case['references'];afterrefs=case['afterReferences'];assert type(refs)is list and type(afterrefs)is list and 1<=len(refs)<=4096
  for value in refs+afterrefs:integer(value,0,2**64-1)
  assert afterrefs==refs and struct.unpack_from('<I',before,40)[0]==len(refs)
  sc=integer(case['sourceCount'],1,4096);assert integer(case['sourceCountAfter'])==sc
  source=decoded(case['sources26']);assert len(source)==sc*26 and decoded(case['sourcesAfter26'])==source
  #Native26*N allocation:24*N endpoint bytes THEN2*N residual bytes, not interleaved rows.
  for i in range(sc):
   a=struct.unpack_from('<iii',source,i*24);b=struct.unpack_from('<iii',source,i*24+12)
   assert a[0]==b[0]==0 and all(v>=0 for v in a[1:]+b[1:])
  assert case['headUnchanged']is True and case['sourcePrefixTopCompleted']is False
  assert integer(case['wordIndexBefore'],0,4095)==integer(case['wordIndexAfter'],0,4095)
  assert integer(case['validResourcePrepareCalls'])==2
  if positive:
   assert case['branch']=='positive-bit8-refused'and case['completionObjectCreated']is False and case['completed']is False
   assert case['completion']is None and integer(case['sourceRunCalls'])==0 and integer(case['guards'])==3
   assert before==after
   assert type(case['refusals'])is list and len(case['refusals'])==2
   for e in case['refusals']:
    assert type(e)is dict and set(e)=={'type','message'}and e['type']=='System.NotSupportedException'and e['message']==ENABLED
   refused+=1
  else:
   assert case['branch']=='zero-bit8-completed'and case['completionObjectCreated']is True and case['completed']is True
   assert integer(case['sourceRunCalls'])==4 and integer(case['guards'])==5 and case['refusals']==[]
   assert case['wordIndexAfter']==0
   assert case['completion']=={'sourcesMarked':True,'ranking':True,'spanScores':True,'checkbox':True,'faulted':False}
   assert all(type(v)is bool for v in case['completion'].values())
   expected=bytearray(before);struct.pack_into('<ff',expected,16,1.0,1.0);assert bytes(expected)==after
   done+=1
  rows+=count;sources+=sc;guards+=case['guards']
 assert integer(data['completedInputs'])==done and integer(data['refusedInputs'])==refused and done+refused==n
 assert integer(data['guards'])==guards and integer(data['activeRows'])==rows
 return {'Inputs':n,'CompletedZeroBit8':done,'RefusedPositiveBit8':refused,'ActiveCompactRows':rows,'SourceReferences':sources,'Guards':guards,'ZeroBit8CompletionExercised':done>0,'Scope':'TEST_DATA_COMPOSITION_NOT_NATIVE_AUGMENTATION_OR_TEXT'}

def source_and_alphabet(root):
 raw=(root/'src/InkRecognition/LegacyFreshMainNoCandidateSourceCompletion.cs').read_bytes().replace(b'\r\n',b'\n');assert hashlib.sha256(raw).hexdigest()==PRODUCTION_LF
 spec=importlib.util.spec_from_file_location('primary_al',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py');al=importlib.util.module_from_spec(spec);spec.loader.exec_module(al)
 resource=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
 assert hashlib.sha256(resource).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
 alphabet=al.read_alphabet(resource);assert not set('\u2610\u2611\u2612')&set(alphabet.values())

if __name__=='__main__':
 if len(sys.argv)!=2:raise SystemExit('capture.json')
 source_and_alphabet(Path(__file__).resolve().parents[2])
 print(json.dumps(validate(json.loads(Path(sys.argv[1]).read_text())),sort_keys=True))
