"""Independent raw root/case32/AVL/permission oracle; no targets/vendor code.
Prior full NN/D0 numerical evidence is explicitly hash-bound, never a word claim.
"""
from pathlib import Path
import argparse,ast,base64,copy,hashlib,json,re,runpy,struct,sys
p=argparse.ArgumentParser()
for k in ('repo','models','capture','output'):p.add_argument('--'+k,type=Path,required=True)
p.add_argument('--case-capture',type=Path);p.add_argument('--case-proof',type=Path)
a=p.parse_args();a.output.parent.mkdir(parents=True,exist_ok=True)
def digest(path):
 h=hashlib.sha256()
 with path.open('rb')as stream:
  for chunk in iter(lambda:stream.read(1048576),b''):h.update(chunk)
 return h.hexdigest()
prefix='eiq-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]
sys.argv=['reference',str(a.models),str(a.output.with_name(prefix+'-case.json'))]
ref=runpy.run_path(str(a.repo/'tools/LegacyEnglishCaseMain.Probe/reference_case_main.py'),run_name='__main__')
ac=ref['ac'];primary=ref['primary'];auxiliary=ref['auxiliary']
sys.argv=['reference',str(a.models),str(a.output.with_name(prefix+'-packed.json'))]
packed=runpy.run_path(str(a.repo/'tools/LegacyEnglishLkPacked102.Probe/reference_actual.py'),run_name='__main__')
#Reuse the independent HEIGHT AVL and raw stored-form classification reference,
#not library output or the balance-state implementation.
init=(a.repo/'tools/LegacyEnglishCaseInitialization.Probe/audit_output.py').read_text(encoding='utf-8-sig')
code=init[init.index('def forms('):init.index('for row in cap[\'Rows\']:')]
exec(compile(code,'independent-initial-AVL/classification','exec'),globals())
initial_tree,initial_nodes=copy.deepcopy((tree,nodes));initial_source68=source68.copy()
policy=json.loads((a.repo/'tools/LegacyEnglishCaseInitialization.Probe/reference_policy.json').read_bytes())
sp=policy['spans']['Policies'];pol=base64.b64decode(sp['base64'],validate=True)
assert len(pol)==420 and hashlib.sha256(pol).hexdigest()=='21b02f8f05c9f0d7dae744b1d3011cc913dde7a1cc228d1b4af22be07aa4c216'
selector=base64.b64decode(policy['spans']['Selectors']['base64'],validate=True)
assert hashlib.sha256(selector).hexdigest()=='43bd68ac941fd1061586016897539d64aa7aedbd067dd931a8631593e355e942'and struct.unpack_from('<i',selector)[0]==1
cap=json.loads(a.capture.read_bytes());assert cap['TargetsRead']is False
assert cap['Scope']=='Actual first case290 node0/span0 query prefix only; no word/history/winner'
assert len(cap['GuardNames'])==cap['Guards']and len(cap['CancelNames'])==cap['Cancellations']
assert {'same-owner-zero-state','equal-fields-foreign-cell','busy-score-no-reset','query-blocks-score','query-blocks-D0','query-blocks-snapshot'}<=set(cap['GuardNames'])
assert {'foreign-knowledge-does-not-consume-pool','second-pool-claim','foreign-current-D0-receipt','foreign-query-owner','historical-page-cannot-read'}<=set(cap['GuardNames'])
bound_upstream=False;upstream_rows=None
numeric_audit=None;Pool=None
def stream_rows(path):
 #One old owner at a time; never retain the 146/413MB full NN JSON in memory.
 decoder=json.JSONDecoder()
 with path.open(encoding='utf-8')as source:
  buffer=source.read(65536);marker=re.search(r'"Rows"\s*:\s*\[',buffer);assert marker is not None;buffer=buffer[marker.end():]
  while True:
   buffer=buffer.lstrip(' \r\n\t,')
   while not buffer:
    buffer=source.read(65536).lstrip(' \r\n\t,');assert buffer
   if buffer.startswith(']'):return
   assert buffer.startswith('{');depth=0;quoted=escaped=False;scan=0
   while True:
    finished=False
    while scan<len(buffer):
     ch=buffer[scan];scan+=1
     if quoted:
      if escaped:escaped=False
      elif ch=='\\':escaped=True
      elif ch=='"':quoted=False
     elif ch=='"':quoted=True
     elif ch=='{':depth+=1
     elif ch=='}':
      depth-=1
      if depth==0:finished=True;break
    if finished:break
    chunk=source.read(65536);assert chunk;buffer+=chunk
   #Decode each complete owner ONCE, rather than repeatedly parsing its prefix.
   row,end=decoder.raw_decode(buffer[:scan]);assert end==scan
   yield row;buffer=buffer[scan:]
if a.case_capture is not None or a.case_proof is not None:
 assert a.case_capture is not None and a.case_proof is not None
 proof=json.loads(a.case_proof.read_bytes());assert proof['status']=='PASS'and proof['captureSha256']==digest(a.case_capture)
 assert proof['inputSha256']==cap['InputSha256']and proof['owners']==len(cap['Rows'])and proof['targetsRead']is False and proof['vendorExecuted']is False
 upstream_rows=iter(stream_rows(a.case_capture));bound_upstream=True
 #A prior PASS is provenance, NEVER sufficient numerical authority. Recompute
 #all actual selected first-D0 providers from current raw matrices/features.
 #Only the old driver's full-JSON loading/control-loop is omitted; its exact
 #actual numerical statements and native arithmetic remain unchanged.
 nn_path=a.repo/'tools/LegacyEnglishMainAcoustic.Probe/audit_output.py';nn_source=nn_path.read_text(encoding='utf-8-sig')
 assert hashlib.sha256(nn_source.encode()).hexdigest()=='0b26ec0dee56f97450d44804fc2a0a294b3b2fd20de8f1ba179729a3573ebbec'
 nn_prefix=ast.parse(nn_source[:nn_source.index('for control in capture.get("PreparedAssemblyControls",[]):')]);replaced=0
 for item in nn_prefix.body:
  if isinstance(item,ast.Assign)and any(isinstance(t,ast.Name)and t.id=='capture'for t in item.targets):
   item.value=ast.Dict(keys=[ast.Constant('TargetsRead')],values=[ast.Constant(False)]);replaced+=1
 assert replaced==1
 sys.argv=[str(nn_path),str(a.models/'en-US/en_US-ak-cur.lite.res'),str(a.output.with_name(prefix+'-case.json')),'unused/no-capture-load']
 nn_scope={'__file__':str(nn_path),'__name__':'first_query_independent_current_numeric'}
 exec(compile(ast.fix_missing_locations(nn_prefix),str(nn_path),'exec'),nn_scope)
 actual_loop=next(item for item in ast.parse(nn_source).body if isinstance(item,ast.For)and isinstance(item.target,ast.Name)and item.target.id=='row')
 group_loop=actual_loop.body[0];assert isinstance(group_loop,ast.For)
 body=copy.deepcopy(group_loop.body);assert isinstance(body[-1],ast.AugAssign)and body[-1].target.id=='checked';body=body[:-1]+[ast.Return(value=ast.Name(id='final',ctx=ast.Load()))]
 function=ast.FunctionDef(name='audit_actual_selected_group',args=ast.arguments(posonlyargs=[],args=[ast.arg(arg='row'),ast.arg(arg='g')],kwonlyargs=[],kw_defaults=[],defaults=[]),body=body,decorator_list=[])
 exec(compile(ast.fix_missing_locations(ast.Module(body=[function],type_ignores=[])),str(nn_path),'exec'),nn_scope);numeric_audit=nn_scope['audit_actual_selected_group']
 pool_path=a.repo/'tools/LegacyEnglishMainD0.Probe/audit_output.py';pool_tree=ast.parse(pool_path.read_text(encoding='utf-8-sig'));pool_tree.body=[item for item in pool_tree.body if isinstance(item,ast.ClassDef)and item.name=='Pool'];pool_scope={};exec(compile(pool_tree,str(pool_path),'exec'),pool_scope);Pool=pool_scope['Pool']
elif len(cap['Rows'])in(100,134):raise AssertionError('Full actual corpus requires complete raw-NN/D0 evidence binding')
reverse={text:label for label,text in ac['text'].items()};reserved=list(ac['header'][4:7])+[reverse.get(s,65535)for s in ['\U000f0000','\u200c','\u200d']]
totals=dict(pages=0,records=0,selectedRows=0,contexts=0,upstreamCosts=0,currentRawNnGroups=0)
for captured in cap['Rows']:
 assert captured['InitialTag']==0x0000080000000000 and captured['ContextCapacity']==2048 and captured['LogicalCeiling']==90987785
 assert captured['ResetCount']==captured['EnteredQueries']==1
 if upstream_rows is not None:
  old=next(upstream_rows);assert old['Name']==captured['Name']and old['Source720']==captured['Source720']
  first_pass=[r for r in old['Passes']if r['Anchor']==0 and r['Span']==0]
  if first_pass:assert first_pass[0]['Selected']==captured['Selected']and first_pass[0]['Permissions']==captured['Permissions']and first_pass[0]['Working']==captured['Working']
  else:assert not captured['Selected']and not any(span==0 for group,span in old['Graph'][0])
  selected_ordinals=[i for i,(group,span)in enumerate(old['Graph'][0])if span==0];chosen=[old['Graph'][0][i][0]for i in selected_ordinals];providers={}
  original_groups={group['Group']:group for group in old['Groups']}
  for group in dict.fromkeys(chosen):
   provider=original_groups[group];assert provider['Span']==0;costs=numeric_audit(old,provider)
   providers[group]=(costs,provider['PowerScale']);totals['currentRawNnGroups']+=1
  callbacks,current,computed,reused=Pool(providers).run(chosen,0,present={group for group,span in old['Graph'][0]})
  assert [costs for costs,scale in callbacks]==current and len(current)==len(captured['Selected'])
  for ordinal,group,costs,actual_selected in zip(selected_ordinals,chosen,current,captured['Selected']):
   assert(actual_selected['Ordinal'],actual_selected['Group'],actual_selected['Costs'])==(ordinal,group,costs)
  totals['upstreamCosts']+=220*len(captured['Selected'])
 #Actual first-span0 has power1. Strict thresholds and ordered native case OR.
 permissions=[0]*217
 for label in reserved:
  if label!=65535:permissions[label]=3
 for selected in captured['Selected']:
  assert len(selected['Costs'])==220
  for symbol,cost in enumerate(selected['Costs'][:217]):permissions[symbol]|=(3 if cost<0x4000000 else 0)|(4 if cost<40000 else 0)
 expanded=permissions.copy()
 for first_label,second_label,third_label in ac['triplets']:
  if first_label!=65535:
   if second_label!=65535 and expanded[second_label]:expanded[first_label]|=expanded[second_label]
   if third_label!=65535 and expanded[third_label]:expanded[first_label]|=expanded[third_label]
  if second_label!=65535 and third_label!=65535 and expanded[third_label]:expanded[second_label]|=expanded[third_label]
  if first_label!=65535 and expanded[first_label]and second_label!=65535:expanded[second_label]|=expanded[first_label]
 working=[key if key>=217 or expanded[key]&1 else 65535 for key in primary]
 assert permissions==captured['RawPermissions']and expanded==captured['Permissions']and working==captured['Working']
 tree,nodes=copy.deepcopy((initial_tree,initial_nodes));rows=[];block=[];blocks=[]
 for child in range(packed['starts'][0]+1,packed['starts'][0]+packed['counts'][0]):
  lexical=packed['labels'][child]&packed['labelMask'];symbol=working[lexical]
  if symbol==65535:continue
  assert symbol<217 and primary[lexical]==symbol
  if len(block)>=124:blocks.append(block);block=[]
  original=bytearray(32);struct.pack_into('<I',original,0,child);original[4]=1;original[5]=1|int(bool(packed['labels'][child]&packed['terminalBit']))*2
  struct.pack_into('<Q',original,8,initial_source68[lexical]);struct.pack_into('<H',original,20,symbol)
  kind=second[symbol+29];both=(original[5]&3)==3;emitted=[]
  def emit(column,form=-1,delta=0):
   global tree
   value=original.copy();value[7]=(value[7]&3)|((pol[column]&31)<<2)
   if form>=0:
    mapped=expectedForms[symbol][2][form];struct.pack_into('<H',value,20,mapped)
    tree=insert(tree,mapped);struct.pack_into('<Q',value,8,nodes[mapped].slot+1)
   struct.pack_into('<i',value,16,delta);emitted.append(value)
  has=lambda column:pol[column]!=255
  if kind==1:
   if has(3):emit(3)
   if has(6):
    emit(6,1,1)
    if both:emit(11,1,1)
  elif kind==2:
   if has(3):emit(3)
   for column,form,terminal_column in [(7,1,12),(8,2,13)]:
    if has(column):
     emit(column,form,1)
     if both:emit(terminal_column,form,1)
  elif kind==3:
   if has(4):emit(4)
   if has(10):emit(10,0,8192)
  elif kind==4:
   if has(5):emit(5)
   if has(9):emit(9,2,1)
   #Native localcost is updated by the preceding title-form branch and
   #retained by the following lower-form branch, rather than reset per Emit.
   if has(10):emit(10,0,8192+int(has(9)))
  elif kind==5:emit(0)
  elif kind==6:emit(1)
  elif kind==7:emitted.append(original)
  else:emit(2)
  block+=emitted;rows+=emitted
 if block:blocks.append(block)
 assert len(rows)<=400
 #Outer128 traversal consumes native233090 cap-4 blocks, preserving leftovers.
 pages=[];block_cursor=row_cursor=accepted=0;loaded=None
 while True:
  output=[];calls=0
  while len(output)<128:
   if loaded is None or row_cursor==len(loaded):
    loaded=blocks[block_cursor]if block_cursor<len(blocks)else[];block_cursor+=1;row_cursor=0;calls+=1
    if not loaded:break
   take=min(128-len(output),len(loaded)-row_cursor);output+=loaded[row_cursor:row_cursor+take];row_cursor+=take
  skip=accepted;accepted+=len(output);pages.append((skip,len(output),accepted,calls,b''.join(output).hex()))
  if not output:break
 assert len(pages)==len(captured['Pages'])<=5
 for expected,actual in zip(pages,captured['Pages']):
  assert expected==(actual['Skip'],actual['Count'],actual['Accepted'],actual['BackendCalls'],actual['Records']),(captured['Name'],'full page/cost/tag/flags/cache/skip',expected,actual)
  assert actual['Slot']==0 and actual['Epoch']==1
 assert len(rows)==captured['ConsumedRows']and export(tree)==captured['Nodes']and tree.slot+1==captured['Root']
 assert captured['InitialSource68']==initial_source68
 assert captured['PageCounts']==[min(102,len(nodes)-i)for i in range(0,len(nodes),102)]and captured['PageCursor']==(len(nodes)-1)//102
 assert captured['PageCapacity']==(1 if len(nodes)<=102 else 2 if len(nodes)<=204 else 4 if len(nodes)<=408 else 8 if len(nodes)<=816 else 16)
 totals['pages']+=len(pages);totals['records']+=len(rows);totals['contexts']+=len(nodes);totals['selectedRows']+=len(captured['Selected'])
if upstream_rows is not None:
 try:next(upstream_rows);raise AssertionError('Unmatched upstream owner')
 except StopIteration:pass
if 'PreparedPoolControl'in cap:
 control=cap['PreparedPoolControl'];assert control['Scope']=='PREPARED allocator only; never a query/ready owner'
 tree,nodes=copy.deepcopy((initial_tree,initial_nodes))
 for key in control['InsertedKeys']:tree=insert(tree,key)
 assert len(nodes)==105 and export(tree)==control['Nodes']and control['Root']==tree.slot+1 and control['Public104NullAllocations']==1
 assert(control['Pages'],control['PageCapacity'],control['Cursor'])==([102,3],2,1)
report=dict(status='PASS',scope='Actual first English case source-query pages only; no word/history/winner',owners=len(cap['Rows']),refused=len(cap['MultilineRefused']),**totals,currentRawSelectedNnNetworks=totals['currentRawNnGroups']*4,currentRawSelectedNnVerified=numeric_audit is not None,guards=cap['Guards'],cancellations=cap['Cancellations'],boundPriorFullRawNnD0Evidence=bound_upstream,captureSha256=digest(a.capture),inputSha256=cap['InputSha256'],targetsRead=False,vendorExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
