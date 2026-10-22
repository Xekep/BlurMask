"""Independent raw number13A initial prefix oracle, no runtime/vendor import."""
import ast,copy,hashlib,json,runpy,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('MODELS CAPTURE')
models=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes());sha=lambda b:hashlib.sha256(b).hexdigest()
lk=(models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert sha(lk)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'and sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
# Independent serialized alphabet parser; no captured label/map is expected input.
parser=Path(__file__).resolve().parents[1]/'LegacyFst28CPrimaryMap.Probe/audit_output.py';tree=ast.parse(parser.read_text(encoding='utf8'));ns={'struct':struct}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef)and n.name=='alphabet'],type_ignores=[]),'raw-alphabet','exec'),ns)
sn,se,source,reverse=ns['alphabet'](lk,299110);an,ae,output,_=ns['alphabet'](ak,99)
assert(sn,se,an,ae)==(16,299573,192,5233)
lookup={token:label for label,token in output.items()};primary=[]
for label in range(16):
 token=source[label];primary.append(65535 if reverse[label]==65535 else 65533 if token==b'\0'else 65531 if token==b' 'else lookup.get(token,65535))
keys=[];source_slots=[]
for code in primary:
 if code in(65535,65533):source_slots.append(None);continue
 if code not in keys:keys.append(code)
 source_slots.append(keys.index(code))
slots=[[i,key,False,False]for i,key in enumerate(keys)]
assert len(slots)==15 and source_slots[0]is None
assert b'\xe2\x80\x8d'not in source.values()
assert struct.unpack_from('<HH',lk,299016)==(0x301,0x100)and struct.unpack_from('<i',lk,299080)[0]==-1
key=bytes.fromhex('14121977');name=bytes(b^key[i%4]for i,b in enumerate(lk[299024:299080])).split(b'\0',1)[0].decode('ascii')
assert name=='ru_RU-lk-text.number'
starts=[struct.unpack_from('<I',lk,299588+i*8)[0]for i in range(99)];degrees=[b-a for a,b in zip(starts,starts[1:])]
assert starts[0]==0 and starts[-1]==299 and all(0<=d<=12 for d in degrees)
capacity=struct.unpack_from('<I',lk,300380)[0];assert capacity==12 and degrees[0]==1
for i in range(299):assert struct.unpack_from('<I',lk,300384+i*4)[0]<98
# Reuse the ORIGINAL independent13A raw narrow/wide parser. Narrowraw5 is
#runtimebyte6; raw7 is opaque. The superseded initialSTATIC reference got this
#wrong; this oracle never introduces another inline descriptor interpretation.
outgoing_oracle=Path(__file__).resolve().parents[1]/'LegacyFst13AOutgoing.Probe/audit_output.py'
original=ast.parse(outgoing_oracle.read_text(encoding='utf8'));raw_ns={'struct':struct}
exec(compile(ast.Module(body=[n for n in original.body if isinstance(n,ast.FunctionDef)and n.name=='parse'],type_ignores=[]),'original-outgoing-parser','exec'),raw_ns)
states,words,descriptors=raw_ns['parse'](lk[299576:301580],299576)
assert states==98 and [r[0]for r in descriptors]==starts
costs=runpy.run_path(str(Path(__file__).resolve().parents[1]/'handwriting/reference_fst23c_outgoing.py'))['actual_main135_costs']()
assert sha(struct.pack('<256i',*costs))=='6db9bc77ac744b8bb25b71d42d15f2ef088d9273b660b2950cb9bca788406ba3'
root_target=words[descriptors[0][0]];_,label,signed,flag=descriptors[root_target]
secondary=primary[label]if primary[label]>=an else 65535
root_row=[root_target,label,primary[label],secondary,costs[signed+128],signed,flag,source_slots[label]]
assert root_row==[97,0,65533,65533,3454,51,0,None]
cursors=[[i,0xc8+i*0x50,0 if i==0 else -1,-1,-1,1 if i==0 else 0,0 if i==0 else -1,0 if i==0 else -1]for i in range(8)]
diagnostic_sequence=[4,7,4,0,65531,7,0];seen=[];ids=[]
for key in diagnostic_sequence:
 if key not in seen:seen.append(key)
 ids.append(seen.index(key))
guards=['same-typed-target-source-root','per-owner-private-cohort','equal-bytes-independent-retained-owners','diagnostic-cannot-acquire-primary','null-target','negative-byte-budget','excess-byte-budget','short-byte-budget','negative-work-budget','excess-work-budget','short-work-budget','cancel-create','cancel-null-first','cancel-slots','cancel-degrees']+['cursor-'+str(i)for i in[-1,8,2147483647]]+['source-label-'+str(i)for i in[16,192,65535]]+['private-key-helper-duplicate-existing-identities','full-existing-key-no-allocation','key-slot-capacity','oversize-private-key-domain','defensive-slot-degree-copies','nonnull-fifteen-slots-two-null-payloads','capacity12-degree1-next0','caller-mutation-gc','parallel-immutable-1024']
logical_bytes=2580+15*18+16*4+8*(80+12*12)+98*4+56+112+16+30
work=logical_bytes+99+299+16*16+8+98+56+1
assert(logical_bytes,work)==(5312,6129)
def validate(c):
 assert(c['schema'],c['byteEnvelope'],c['workEnvelope'])==(1,logical_bytes,work)
 assert c['rootSha']==sha(lk)and c['acousticSha']==sha(ak)and c['sourceName']==name
 assert c['capacity']==capacity and c['nextCursor']==0 and c['initialCallback']=='180240010'and c['initialAlternative']==-1
 assert c['slots']==slots and c['sourceSlots']==source_slots and c['cursors']==cursors and c['degrees']==degrees and c['rootRow']==root_row
 assert c['duplicateKeySequence']==diagnostic_sequence and c['duplicateKeyIds']==ids
 assert c['guards']==guards
 for key in['query28CImplemented','historyConsumerImplemented','emptyAuxiliaryAssumed','unusedNativeBytesExposed']:assert c[key]is False
validate(capture)
negative=[]
for name,mutate in[
 ('empty-auxiliary',lambda c:c.__setitem__('slots',[])),
 ('collapse-nul-slot',lambda c:c['sourceSlots'].__setitem__(0,0)),
 ('wrong-history-payload',lambda c:c['slots'][0].__setitem__(2,True)),
 ('wrong-cursor-next',lambda c:c.__setitem__('nextCursor',1)),
 ('capacity-as-rootdegree',lambda c:c['cursors'][0].__setitem__(5,12)),
 ('wrong-root-special',lambda c:c['rootRow'].__setitem__(2,0)),
 ('duplicate-key-new-id',lambda c:c['duplicateKeyIds'].__setitem__(2,4)),
 ('expose-unused-bytes',lambda c:c.__setitem__('unusedNativeBytesExposed',True))]:
 changed=copy.deepcopy(capture);mutate(changed)
 try:validate(changed)
 except AssertionError:negative.append(name)
 else:raise AssertionError(('negative accepted',name))
assert len(negative)==8
print(f'PASS independent15aux/16associations/8cursors/98degrees/state0degree1vs12capacity/{len(guards)}guards/8negativecaptures; rawdecodedname/root97/FFFD/sourceCost3454/tag51, noquery/history.')
