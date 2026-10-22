"""Independent raw-resource target14/range/secondary oracle. No vendor load.
Expected fields, all59 adjacency records and299 rows come from models, never
from captured output. Internal negative captures prove checking sensitivity.
"""
import ast,copy,hashlib,json,runpy,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('MODELS CAPTURE')
models=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes());sha=lambda b:hashlib.sha256(b).hexdigest()
lk=(models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert sha(lk)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'and sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<I',lk,299096)[0]==257 and struct.unpack_from('<IH',lk,299104)==(0x2b,0x103)
assert struct.unpack_from('<IH',ak,93)==(0x2b,0x103)and struct.unpack_from('<I',ak,5233)[0]==0x135
# Reuse only the independently raw serialized alphabet parser, not captured maps
# or production helper results. This parser predates target preparation.
parser=Path(__file__).resolve().parents[1]/'LegacyFst28CPrimaryMap.Probe/audit_output.py'
tree=ast.parse(parser.read_text(encoding='utf8'));ns={'struct':struct}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef)and n.name=='alphabet'],type_ignores=[]),'raw-alphabet-v103','exec'),ns)
sn,se,source,reverse=ns['alphabet'](lk,299110);an,ae,acoustic,_=ns['alphabet'](ak,99)
assert(sn,se,an,ae)==(16,299573,192,5233)
lookup={text:label for label,text in acoustic.items()};h=struct.unpack_from('<8H',ak,99)
field_by_slot={0xa0:lookup.get('\U000f0001'.encode(),65535),0xa2:h[4],0xa4:h[5],0xa6:h[6],0xa8:lookup.get('\U000f0002'.encode(),65535),0xaa:lookup.get('\U000f0003'.encode(),65535),0xac:lookup.get('\U000f0004'.encode(),65535),0xae:lookup.get('\U000f0008'.encode(),65535),0xb0:lookup.get('\ufffd'.encode(),65535),0xb2:lookup.get('\u00df'.encode(),65535),0xb4:lookup.get('\u00ad'.encode(),65535)}
def range_reference(table,slot,text):
 start=table.get(text[0].encode(),65535);rows=[]
 for i,c in enumerate(text[1:],1):
  actual=table.get(c.encode(),65535);rows.append([slot,ord(c),start+i,actual])
  if actual!=start+i:return 65535,rows
 return start,rows
ranges=[];range_negatives=0
for slot,text in[(0xb6,'0123456789'),(0xb8,'abcdefghijklmnopqrstuvwxyz'),(0xba,'ABCDEFGHIJKLMNOPQRSTUVWXYZ')]:
 value,rows=range_reference(lookup,slot,text);field_by_slot[slot]=value;ranges+=rows
 negative=dict(lookup);negative[text[-1].encode()]=65535
 assert range_reference(negative,slot,text)[0]==65535;range_negatives+=1
fields=sorted([[slot,value]for slot,value in field_by_slot.items()]);assert len(fields)==14 and len(ranges)==59
assert sha(struct.pack('<14H',*(row[1]for row in fields)))=='ae560b7f236be56cd4b1791eb049f6438f91d49006aed672e1fa5538f7a7dd5e'
primary=[]
for i in range(16):
 token=source[i];primary.append(65535 if reverse[i]==65535 else 65533 if token==b'\0'else 65531 if token==b' 'else lookup.get(token,65535))
def secondary_reference(c4):return[[i,v if v>=an and(not c4 or((v+7)&65533)!=0)else 65535]for i,v in enumerate(primary)]
secondary=secondary_reference(False);contrast=secondary_reference(True)
assert[i for i in range(16)if secondary[i]!=contrast[i]]==[11]
assert sha(struct.pack('<16H',*(r[1]for r in secondary)))=='de1d06a589978459fb5bb6e14dce606e472e91bd2a34da9fba7296dd00b10c02'
costs=runpy.run_path(str(Path(__file__).resolve().parents[1]/'handwriting/reference_fst23c_outgoing.py'))['actual_main135_costs']()
assert sha(struct.pack('<256i',*costs))=='6db9bc77ac744b8bb25b71d42d15f2ef088d9273b660b2950cb9bca788406ba3'
states=[];total=0
for state in range(98):
 first=struct.unpack_from('<I',lk,299588+state*8)[0];last=struct.unpack_from('<I',lk,299588+(state+1)*8)[0];rows=[]
 for ordinal,index in enumerate(range(first,last)):
  target=struct.unpack_from('<I',lk,300384+index*4)[0];label,tag,b7=struct.unpack_from('<BBB',lk,299588+target*8+4);signed=tag if tag<128 else tag-256
  rows.append([ordinal,target,label,primary[label],secondary[label][1],costs[signed+128],signed,b7]);total+=1
 states.append(dict(state=state,outputFlag=0,rows=rows))
assert total==299
guards=['exact-primary-source-root-owner','equal-content-independent-owner','fresh-policy-per-preparation','diagnostic-cannot-acquire-primary','foreign-root-actual-source','null-primary','wrong-ak-length']+['wrong-ak-byte-'+str(i)for i in[93,97,99,5233,len(ak)-1]]+['negative-budget','excess-budget','short-budget','cancel-create','cancel-null-first','cancel-invalid-first','cancel-fields','cancel-ranges','cancel-secondary']+['slot-'+str(i)for i in[0,0x9e,0xa1,0xbb,0xbc,0xff]]+['label-'+str(i)for i in[16,192,65535]]+['defensive-all-output-copies','14-fields-59-adjacencies-16-secondary','fresh-zero-c4-nul-space-not-digit0','caller-mutation-gc','parallel-immutable-1024']
def validate(c):
 assert c['schema']==1 and c['preparationWork']==572763+2580+192+59+16+14
 assert c['rootSha']==sha(lk)and c['acousticSha']==sha(ak)
 assert c['fields']==fields and c['ranges']==ranges and c['secondary']==secondary
 assert c['states']==states and c['rowCount']==299
 assert c['guards']==guards
 assert c['policy']=='NewLibraryOwnedConstructorOnlyFreshZeroC4'and c['configuredKnowledgeClaim']is False and c['query28CImplemented']is False and c['historyAssumedEmpty']is False
validate(capture)
negative_captures=[]
for name,mutate in[
 ('header-field',lambda c:c['fields'][1].__setitem__(1,0)),
 ('adjacency-delete',lambda c:c['ranges'].pop()),
 ('secondary-space',lambda c:c['secondary'][11].__setitem__(1,65535)),
 ('projection-secondary',lambda c:c['states'][0]['rows'][0].__setitem__(4,0)),
 ('query-claim',lambda c:c.__setitem__('query28CImplemented',True)),
 ('configured-policy',lambda c:c.__setitem__('policy','ConfiguredKnowledge'))]:
 changed=copy.deepcopy(capture);mutate(changed)
 try:validate(changed)
 except AssertionError:negative_captures.append(name)
 else:raise AssertionError(('negative capture accepted',name))
assert len(negative_captures)==6
print(f'PASS independent14fields/59adjacency/16secondary/98states/299projections/{len(guards)}guards;3range-refusals/6negativecaptures/C4counterfactual; noquery/history/configured knowledge')
