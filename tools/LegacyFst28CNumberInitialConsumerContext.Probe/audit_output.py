"""Independent raw number first-page/context-graft oracle.
MODELS CAPTURE COMPLETED_D0_CAPTURE. Full old19NN proof; rawAL/outgoing/costs,
independent height-AVL; strict currentBinding ->dcfb ->859a preimage auth.
No expected labels, fullword/history/parent/query claims or candidate tuning.
"""
import ast,copy,hashlib,json,runpy,struct,sys
from pathlib import Path
sha=lambda b:hashlib.sha256(b).hexdigest()
BINDING_SUCCESSOR_SHA='f57deaa2de12e7b38dc94aa99153103b9ee190c14320c48455fca9e0e213affe'
PRODUCTION_SHA='98f19fb593fce5701672e213c2049f60c10d4e4a8f1c6532b90ffac08e9bce61'
BINDING_REVERSE_TRANSFORMS=[{'beforeASCII': '        Action<string>? checkpoint=null)\n'
                 '    {\n'
                 '        token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(d0);',
  'afterASCII': '        Action<string>? checkpoint=null)\n'
                '        => CreateCore(d0,byteBudget,workBudget,token,checkpoint,null);\n'
                '\n'
                '    // The sole private30 path shares EVERY original validation/read/claim/copy\n'
                '    // stage and the SAME bootstrap claim with the unchanged default18 entry.\n'
                '    internal static LegacyFst28CNumberInitialQueryBinding CreateForConsumerPool(\n'
                '        LegacyFst28CNumberInitialD0 d0,LegacyFst28CNumberInitialConsumerContext.Pool pool,\n'
                '        CancellationToken token,Action<string>? checkpoint=null)\n'
                '    {\n'
                '        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(pool);\n'
                '        return CreateCore(d0,LogicalByteEnvelope,WorkEnvelope,token,checkpoint,pool);\n'
                '    }\n'
                '\n'
                '    private static LegacyFst28CNumberInitialQueryBinding CreateCore(\n'
                '        LegacyFst28CNumberInitialD0 d0,int byteBudget,int workBudget,CancellationToken token,\n'
                '        Action<string>? checkpoint,LegacyFst28CNumberInitialConsumerContext.Pool? pool)\n'
                '    {\n'
                '        token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(d0);'},
 {'beforeASCII': '        var header=boot.RetainInitialNumberHeaderIdentity(d0,token);\n        void Checkpoint',
  'afterASCII': '        var header=boot.RetainInitialNumberHeaderIdentity(d0,token);\n'
                '        pool?.ValidateBeforeBindingClaim(d0,header,token);\n'
                '        void Checkpoint'},
 {'beforeASCII': '        var context=new LegacySymbolContextFactory(18);\n',
  'afterASCII': '        var context=pool is null?new '
                'LegacySymbolContextFactory(18):pool.TakeEmptyContext(d0,header,token);\n'},
 {'beforeASCII': '        return '
                 'new(d0,header,context,nodes,preserved,current,secondary,labelNodes,labelSlots,auxiliaries,prefixNodes,reset);\n',
  'afterASCII': '        var result=new '
                'LegacyFst28CNumberInitialQueryBinding(d0,header,context,nodes,preserved,current,secondary,labelNodes,labelSlots,auxiliaries,prefixNodes,reset);\n'
                '        pool?.AttachInitialBinding(result,context,header,token);\n'
                '        return result;\n'}]

def binding_preimages(data):
 b=data.replace(b'\r\n',b'\n');digest=sha(b)
 assert digest in[BINDING_SUCCESSOR_SHA,'dcfb63ac94035f1c409c56ffa89d9284f557c2b5646deaa78277936df29006e8','859a58db8706c1c292362c28c80d08646f51dc86574d799359c76d5efd96fb8f']
 if digest==BINDING_SUCCESSOR_SHA:
  for t in reversed(BINDING_REVERSE_TRANSFORMS):
   after=t['afterASCII'].encode();before=t['beforeASCII'].encode();assert b.count(after)==1;b=b.replace(after,before)
  assert sha(b)=='dcfb63ac94035f1c409c56ffa89d9284f557c2b5646deaa78277936df29006e8'
 pagePreimage=b
 if sha(b)=='dcfb63ac94035f1c409c56ffa89d9284f557c2b5646deaa78277936df29006e8':
  start=b.index(b'    // Genuine private prefix handoff; detached diagnostics are not accepted.')
  end=b.index(b'    // Detached inspection cannot be accepted as a query/current-context receipt.',start);addition=b[start:end]
  assert len(addition.splitlines())==13 and sha(addition)=='a84f4d57852e015e525603e65f753495f12c7121f7a3ad63faa7ac1083159fc5'
  b=b[:start]+b[end:]
 assert sha(b)=='859a58db8706c1c292362c28c80d08646f51dc86574d799359c76d5efd96fb8f'
 return pagePreimage,b

def main():
    if len(sys.argv)!=4:raise SystemExit('MODELS CAPTURE COMPLETED_D0_CAPTURE')
    models=Path(sys.argv[1]);capturePath=Path(sys.argv[2]);d0Path=Path(sys.argv[3]);capture=json.loads(capturePath.read_bytes());d0=json.loads(d0Path.read_bytes());repo=Path(__file__).resolve().parents[2]
    binding=(repo/'src/InkRecognition/LegacyFst28CNumberInitialQueryBinding.cs').read_bytes().replace(b'\r\n',b'\n');pagePreimage,initialPreimage=binding_preimages(binding)
    assert binding_preimages(pagePreimage)[1]==initialPreimage and binding_preimages(initialPreimage)[1]==initialPreimage
    assert binding_preimages(binding.replace(b'\n',b'\r\n'))[1]==initialPreimage
    preimageNegatives=[]
    for name,raw in [('successor-byte',binding),('dcfb-byte',pagePreimage),('859a-byte',initialPreimage)]:
     bad=bytearray(raw);bad[100]^=1
     try:binding_preimages(bad)
     except AssertionError:preimageNegatives.append(name)
     else:raise AssertionError('tampered binding accepted')
    for i,t in enumerate(BINDING_REVERSE_TRANSFORMS):
     bad=binding.replace(t['afterASCII'].encode(),t['beforeASCII'].encode())
     try:binding_preimages(bad)
     except AssertionError:preimageNegatives.append('partial-reverse-'+str(i))
     else:raise AssertionError('partial transform accepted')
    # Historical native STATIC pins are unchanged, checked against authenticated
    # exact preimages, never against an unverified normalized/edited source.
    pins=json.loads((repo/'tools/handwriting/linguistic-fst28c-number-first-page-pins.json').read_bytes())
    for row in pins['runtimeReviewedLF']+pins['existingIndependentDefinitionsLF']:
     b=(repo/row['path']).read_bytes().replace(b'\r\n',b'\n')
     b=runpy.run_path(str(repo/'tools/LegacyFst28CNumberInitialConsumer.Probe/audit_output.py'))['source_preimage'](row['path'],b)
     if row['path'].endswith('LegacyFst28CNumberInitialQueryBinding.cs'):b=initialPreimage
     assert sha(b)==row['sha256LF'],row['path']
    assert sha(runpy.run_path(str(repo/'tools/LegacyFst28CNumberInitialConsumer.Probe/audit_output.py'))['source_preimage']('LegacyFst28CNumberInitialConsumerContext.cs',(repo/'src/InkRecognition/LegacyFst28CNumberInitialConsumerContext.cs').read_bytes()))==PRODUCTION_SHA
    old=sys.argv;sys.argv=[str(repo/'tools/LegacyFst28CNumberInitialD0.Probe/audit_output.py'),str(models),str(d0Path)]
    try:runpy.run_path(sys.argv[0],run_name='__main__')
    finally:sys.argv=old

    def defs(path,names):
     ns={'struct':struct};nodes=[n for n in ast.parse(path.read_text(encoding='utf-8-sig')).body if isinstance(n,(ast.FunctionDef,ast.ClassDef))and n.name in names];assert len(nodes)==len(names);exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),ns);return ns
    alphabet=defs(repo/'tools/LegacyFst28CPrimaryMap.Probe/audit_output.py',{'alphabet'})['alphabet'];parse=defs(repo/'tools/LegacyFst13AOutgoing.Probe/audit_output.py',{'parse'})['parse'];avl=defs(repo/'tools/LegacySymbolContext.Probe/reference_controls.py',{'Node','height','update','left','right','add','case'})
    lk=(models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert sha(lk)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'and sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
    sn,se,source,reverse=alphabet(lk,299110);an,ae,acoustic,_=alphabet(ak,99);assert(sn,se,an,ae)==(16,299573,192,5233)
    lookup={v:k for k,v in acoustic.items()};preserved=[65535 if reverse[i]==65535 else 65533 if source[i]==b'\0'else 65531 if source[i]==b' 'else lookup.get(source[i],65535)for i in range(sn)]
    keys=[v for v in preserved if v not in(65533,65535)];prefix=[65531,65529,65531,65532,65530];allocation=list(dict.fromkeys(keys+prefix));assert len(allocation)==18;slots={v:i for i,v in enumerate(allocation)};aux=[-1 if v in(65533,65535)else slots[v]for v in preserved];fffa=slots[65530]
    states,words,desc=parse(lk[299576:301580],299576);costs=runpy.run_path(str(repo/'tools/handwriting/reference_fst23c_outgoing.py'))['actual_main135_costs']();assert sha(struct.pack('<256i',*costs))=='6db9bc77ac744b8bb25b71d42d15f2ef088d9273b660b2950cb9bca788406ba3'
    def raw_rows(state):return [[t,desc[t][1],costs[desc[t][2]+128],desc[t][2],desc[t][3]]for t in words[desc[state][0]:desc[state+1][0]]]
    literal=runpy.run_path(str(repo/'tools/handwriting/reference_fst28c_number_first_page.py'))['first_page']
    def initial_nodes():
     tree=avl['case'](keys+prefix);rows={n[0]:n for n in tree['nodes']};out=[]
     for i,key in enumerate(allocation):
      _,left,right,balance=rows[key];out.append([i,key,-1 if left<0 else slots[left],-1 if right<0 else slots[right],balance,-1,-1,0,i])
     return out
    initial=initial_nodes()
    def grafted_nodes(symbols,capacity=30):
     out=copy.deepcopy(initial)
     if capacity==18:return out,[-1]*len(symbols)
     tree=avl['case'](symbols);rows={n[0]:n for n in tree['nodes']};ids={v:18+i for i,v in enumerate(symbols)}
     assert len(ids)==len(symbols)<=12
     out[fffa][6]=-1 if not symbols else ids[tree['root']]
     for key in symbols:
      i=ids[key];_,left,right,balance=rows[key];out.append([i,key,-1 if left<0 else ids[left],-1 if right<0 else ids[right],balance,fffa,-1,0,i])
     return out,[ids[v]for v in symbols]
    B=8954+42288+12*64+12*56+30*64+1024;W=22202+165168+4384+12*30*64+30*30*8+12*64+4096;assert(B,W)==(55626,226858)
    stages=['binding-after-claim','after-prefix','after-page','before-graft-0','after-graft-0','after-active-snapshot','before-publish']
    def verify(j):
     assert j['schema']==1 and j['scope']=='actual-initial-number-consumer-context-only'and j['query']is False and j['history']is False and j['completeWordStartup']is False
     assert(j['bytes'],j['work'])==(B,W)
     assert(j['canonicalD0Owners'],j['canonicalGlyphNN'],j['canonicalContextNN'])==(4,16,3)
     assert(j['additionalGuardD0Owners'],j['additionalGuardGlyphNN'],j['additionalGuardContextNN'])==(10,40,0)
     assert j['additionalGuardPurposes']==['cancel-'+s for s in stages]+['fault-after-graft-0','completed-default18','default18-vs-private30-race']
     assert len(j['cases'])==len(d0['cases'])==4
     for index,(row,old)in enumerate(zip(j['cases'],d0['cases'])):
      current=[v if v>=192 or old['permissions'][v]&1 else 65535 for v in preserved];assert current==old['primary']
      page=literal(raw_rows,current,aux);symbols=[r[3]for r in page['records']];nodes,ids=grafted_nodes(symbols)
      assert row['index']==index and row['sourceRecords']==page['records']
      assert(row['count'],row['initialNodes'],row['activeNodes'],row['pages'],row['sourceNamespace'],row['sourceHeader'],row['sourceKind40'])==(len(symbols),18,18+len(symbols),1,1,1,0)
      assert row['ownerDiagnostic']==[1]*8+[18,len(nodes),1,30,1]
      assert row['historicalInitialNodes']==initial and row['nodes']==nodes
      assert row['grafts']==[[i,sourceRow[1],ids[i],sourceRow[3],fffa,1]for i,sourceRow in enumerate(page['records'])]
     leaves=[preserved[i]for i in list(range(1,11))+[14,15]]
     specs=[('empty',30,[]),('one',30,leaves[:1]),('eight',30,leaves[:8]),('twelve',30,leaves),('twelve-reversed',30,list(reversed(leaves))),('old18-allocation-refusal',18,leaves)]
     assert len(j['preparedOnly'])==len(specs)
     for row,(name,cap,symbols)in zip(j['preparedOnly'],specs):
      nodes,ids=grafted_nodes(symbols,cap)
      assert row['name']==name and row['actualAuthority']is False and(row['capacity'],row['count'])==(cap,len(nodes))and row['nodes']==nodes and row['graftSlots']==ids
    verify(capture)
    guards=['null-d0','cancel-before-null']+['byte-domain-'+str(v)for v in[-1,55627,2147483647]]+['work-domain-'+str(v)for v in[-1,226859,2147483647]]+['one-byte-short','one-work-short','same-genuine-pool-validation-no-claim','foreign-genuine-pool-d0','four-private-header-identities','cancel-before-prefix-claim','before-prefix-claim-reached']
    for c in range(4):
     guards+=[s+'-'+str(c)for s in['same-genuine-owner-one-page','context-stage-only-budget','bounded-active-number-context','private-attach-header-pool-observable','diagnostic-defensive-copy','same-d0-consumer-replay','consumed-consumer-refuses-default18','source-leaf-not-replaced','immutable-graft-value','all-original18-historical-nodes','historical-snapshot-defensive-copy','readonly-active-node-list']]
     guards+=['graft-index-'+str(c)+'-'+str(v)for v in[-1,capture['cases'][c]['count']]]+['diagnostic-canceled-'+str(c)]
    for stage in stages:
     if stage==stages[0]:guards+=['equal-data-foreign-pool-owner','equal-data-foreign-pool-refused']
     guards+=[s+stage for s in['cancel-','reached-','poisoned-consumer-','poisoned-default18-','no-d0-mutation-']]
    guards+=['fault-after-graft-0','fault-shared-claim-poisoned','default18-stays18','historical-default18-not-upgraded','default18-unchanged-after-rejection','shared-default18-private30-claim-one-winner','parallel-immutable-active-inspection-1024']
    guards+=['prepared-only-'+n for n in['empty','one','eight','twelve','twelve-reversed','old18-allocation-refusal']]
    assert capture['guards']==guards and len(guards)==len(set(guards))
    negatives=[]
    def reject(name,change):
     bad=copy.deepcopy(capture);change(bad)
     try:verify(bad)
     except(AssertionError,KeyError,IndexError,ValueError):negatives.append(name);return
     raise AssertionError('correlated corruption accepted: '+name)
    reject('wrong-current-context-count',lambda j:j['cases'][0].__setitem__('activeNodes',18))
    reject('historical-mutated-childtree',lambda j:j['cases'][0]['historicalInitialNodes'][fffa].__setitem__(6,18))
    reject('foreign-header-bit',lambda j:j['cases'][0]['ownerDiagnostic'].__setitem__(5,0))
    reject('wrong-namespace-rootordinal',lambda j:j['cases'][0].__setitem__('sourceNamespace',5))
    reject('English-header-coercion',lambda j:j['cases'][0].__setitem__('sourceHeader',0xc0e))
    reject('source-node-parent-mutated',lambda j:j['cases'][0]['nodes'][0].__setitem__(5,fffa))
    reject('child-root-not-linked',lambda j:j['cases'][0]['nodes'][fffa].__setitem__(6,-1))
    reject('child-parent-not-linked',lambda j:j['cases'][0]['nodes'][18].__setitem__(5,-1))
    reject('wrong-AVL-balance',lambda j:j['cases'][0]['nodes'][18].__setitem__(4,9))
    reject('wrong-allocator-PageSlot',lambda j:j['cases'][0]['nodes'][18].__setitem__(8,29))
    reject('wrong-allocator-page',lambda j:j['cases'][0]['nodes'][18].__setitem__(7,1))
    reject('sourceID-reused-as-child',lambda j:j['cases'][0]['grafts'][0].__setitem__(2,j['cases'][0]['grafts'][0][1]))
    reject('row-order-swap',lambda j:j['cases'][0]['grafts'].reverse())
    reject('source-tag-mutated',lambda j:j['cases'][0]['sourceRecords'][0].__setitem__(0,0))
    reject('prepared-authority-escalation',lambda j:j['preparedOnly'][0].__setitem__('actualAuthority',True))
    print('PASS4raw-page/graft owners/',sum(r['count']for r in capture['cases']),'grafts/9-fieldAVL+original18/',len(guards),'guards/',len(negatives),'corruptions/',len(preimageNegatives),'source-tamper controls;19NN upstream,40extra guardglyphNN separate; noquery/history/fullword claim')

if __name__=='__main__':main()
