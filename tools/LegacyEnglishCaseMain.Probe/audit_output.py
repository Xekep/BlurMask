"""Raw-case M -> all NN/features/cache -> pool -> ordered case source68 oracle.
No labels/targets or vendor executable code. Actual NN arithmetic is reused
unchanged from the independent raw oracle; only its prepared-control block is
omitted here because the OLD-probe regression audits those separate controls.
"""
from pathlib import Path
import argparse,ast,hashlib,json,struct,subprocess,sys,tempfile,itertools
p=argparse.ArgumentParser();p.add_argument('--models',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();root=Path(__file__).resolve().parents[2]
sha=lambda b:hashlib.sha256(b).hexdigest()
payload=a.capture.read_bytes();capture=json.loads(payload);assert capture['TargetsRead']is False
assert(len(capture['Rows']),len(capture['MultilineRefused']),capture['GroupCount'],capture['ArcCount'])in[(134,7,4066,4083),(100,0,11330,11472)]
with tempfile.TemporaryDirectory(prefix='case-main-oracle-',dir=a.output.parent)as temporary:
 refPath=Path(temporary)/'raw-case.json'
 subprocess.run([sys.executable,'-I','-B',str(Path(__file__).with_name('reference_case_main.py')),str(a.models),str(refPath)],check=True)
 ref=json.loads(refPath.read_bytes());primary=ref['sourceMaps']['primary'];expected=ref['maskDiagnostic'];assert sum(expected['primary'])==93 and len(ref['caseTokens'])==544
 for row in capture['Rows']:
  masks=row['CaseMasks']
  assert masks['Tokens']==ref['caseTokens']and masks['Preserved']==primary and masks['Auxiliary']==ref['sourceMaps']['auxiliary']and masks['Descriptor']==ref['sourceMaps']['descriptor']==0xc0e
  for key,reference in [('Seed','seed'),('Primary','primary'),('Secondary','secondary')]:assert masks[key]==expected[reference],(row['Name'],key)
 # Execute every original actual-input numerical oracle statement unchanged.
 # The raw reference already contains the independently computed CASE mask.
 oldPath=root/'tools/LegacyEnglishMainAcoustic.Probe/audit_output.py';old=oldPath.read_text(encoding='utf-8-sig')
 assert sha(old.encode())=='0b26ec0dee56f97450d44804fc2a0a294b3b2fd20de8f1ba179729a3573ebbec'
 start=old.index('for control in capture.get("PreparedAssemblyControls",[]):');end=old.index('checked=0',start)
 actualCode=old[:start]+old[end:];assert old[:start]==actualCode[:start]
 sys.argv=[str(oldPath),str(a.models/'en-US/en_US-ak-cur.lite.res'),str(refPath),str(a.capture)]
 scope={'__file__':str(oldPath),'__name__':'case_actual_numeric_oracle'};exec(compile(actualCode,str(oldPath),'exec'),scope)
 forms=scope['forms']
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
# Independent literal234840, including conditional loads after preceding writes.
def case_permissions(values,rows,version=512):
 d=list(values)
 for first,second,third in rows:
  if first!=65535:
   if second!=65535 and d[second]!=0:d[first]=d[first]|d[second]
   if third!=65535 and d[third]!=0:d[first]=d[first]|d[third]
  if second!=65535 and third!=65535 and d[third]!=0:d[second]=d[second]|d[third]
  if version==512 and first!=65535 and d[first]!=0 and second!=65535:d[second]=d[second]|d[first]
 return d
# Independent AVL by subtree heights. Portable IDs are resolved to SYMBOLS,
# never approximated as the native pointer or copied from expected C# links.
def check_contexts(masks):
 class Node:
  def __init__(self,key,slot):self.key=key;self.left=self.right=None;self.slot=slot
 def height(n):return 0 if n is None else 1+max(height(n.left),height(n.right))
 def rotate_left(n):right=n.right;n.right=right.left;right.left=n;return right
 def rotate_right(n):left=n.left;n.left=left.right;left.right=n;return left
 nodes={}
 def insert(n,key):
  if n is None:new=Node(key,len(nodes));nodes[key]=new;return new
  if key<n.key:n.left=insert(n.left,key)
  elif key>n.key:n.right=insert(n.right,key)
  else:return n
  balance=height(n.right)-height(n.left)
  if balance<-1:
   if key>n.left.key:n.left=rotate_left(n.left)
   return rotate_right(n)
  if balance>1:
   if key<n.right.key:n.right=rotate_right(n.right)
   return rotate_left(n)
  return n
 tree=None
 for key in primary:
  if key not in (65535,65533):tree=insert(tree,key)
 actual=masks['Nodes'];assert len(actual)==len(nodes)==86 and masks['Pages']==[86]
 ids={row[0]:row[5]for row in actual};assert len(ids)==len(actual)and 0 not in ids
 assert ids[masks['Root']]==tree.key
 for ident,parent,child,left,right,key,balance,page,slot in actual:
  expected=nodes[key];assert parent==child==0 and page==expected.slot//102 and slot==expected.slot%102
  assert (None if left==0 else ids[left])==(None if expected.left is None else expected.left.key)
  assert (None if right==0 else ids[right])==(None if expected.right is None else expected.right.key)
  assert balance==height(expected.right)-height(expected.left)
 for value,ident in zip(primary,masks['InitialSource68']):assert (ident==0 if value in (65535,65533)else ids[ident]==value)
# Reuse the independently implemented native pool, NOT the C# implementation.
oldPool=root/'tools/LegacyEnglishMainD0.Probe/audit_output.py';tree=ast.parse(oldPool.read_text(encoding='utf-8-sig'));tree.body=[x for x in tree.body if isinstance(x,ast.ClassDef)and x.name=='Pool'];poolScope={};exec(compile(tree,str(oldPool),'exec'),poolScope);Pool=poolScope['Pool']
ak=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes();header=struct.unpack_from('<8H',ak,99);labels=scope['labels'];reserved=list(header[4:7])+[labels.get(s,65535)for s in ['\U000f0000','\u200c','\u200d']]
passes=selectedCount=empty=reused=0
for row in capture['Rows']:
 check_contexts(row['CaseMasks']);groups={g['Group']:g for g in row['Groups']};providers={k:(v['Costs'],f(v['PowerScale']))for k,v in groups.items()};state=Pool(providers)
 coordinates=[(node,span)for node,arcs in enumerate(row['Graph'])for span in sorted({arc[1]for arc in arcs})]
 assert [(r['Anchor'],r['Span'])for r in row['Passes']]==coordinates+list(reversed(coordinates))+[(0,7),coordinates[0]]
 for epoch,result in enumerate(row['Passes'],1):
  anchor,span=result['Anchor'],result['Span'];arcs=row['Graph'][anchor];ordinals=[i for i,arc in enumerate(arcs)if arc[1]==span];chosen=[arcs[i][0]for i in ordinals]
  callbacks,current,computed,used=state.run(chosen,span,present={arc[0]for arc in arcs})
  assert [c for c,s in callbacks]==current and result['Computed']==computed and result['Reused']==used and result['Epoch']==epoch
  assert len(chosen)==len(result['Selected']);permission=[0]*217;eligibility=[0]*220
  for label in reserved:
   if label!=65535:permission[label]=3
  for ordinal,group,(costs,scale),recorded in zip(ordinals,chosen,callbacks,result['Selected']):
   assert recorded['Ordinal']==ordinal and recorded['Group']==group and recorded['Costs']==costs
   threshold=int(f(f(scale)*40000))
   for symbol,cost in enumerate(costs):
    if cost<0x4000000:eligibility[symbol]=1
    if symbol<217:permission[symbol]|=(3 if cost<0x4000000 else 0)|(4 if cost<threshold else 0)
  expanded=case_permissions(permission,forms);working=[key if key>=217 or expanded[key]&1 else 65535 for key in primary]
  assert permission==result['RawPermissions']and expanded==result['Permissions']and working==result['Working']and eligibility==result['Eligibility'],(row['Name'],anchor,span)
  passes+=1;selectedCount+=len(chosen);empty+=not chosen;reused+=used
assert passes==capture['PassCount']and selectedCount==capture['SelectedRowCount']
assert capture['Guards']==54 and capture['Cancellations']==26
out=dict(status='PASS',scope='Actual raw-case M/NN/cache/pool/one source68 component; not complete290 query or word',owners=len(capture['Rows']),groups=capture['GroupCount'],networks=capture['GroupCount']*4,passes=passes,selectedRows=selectedCount,emptySelections=empty,reusedRows=reused,initialContextNodes=len(capture['Rows'])*86,guards=capture['Guards'],cancellations=capture['Cancellations'],captureSha256=sha(payload),inputSha256=capture['InputSha256'],targetsRead=False,vendorExecuted=False)
a.output.write_text(json.dumps(out,indent=2)+'\n');print(json.dumps(out))
