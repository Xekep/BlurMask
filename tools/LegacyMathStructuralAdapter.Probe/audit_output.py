"""Independent actual-first-record token/source/strict syntax tree oracle.
Geometry from tracked native-coordinate helpers; no vendor execution, labels or
candidate fallback. Raw NN outputs are compared to a separate existing capture.
"""
from pathlib import Path
import ast,base64,hashlib,importlib.util,json,runpy,struct,sys
import numpy as np
root=Path(__file__).resolve().parents[2]
procedural_only='--procedural-only'in sys.argv
sys.argv=[a for a in sys.argv if a!='--procedural-only']
if len(sys.argv)not in(4,5):raise SystemExit('adapter.json actual-raw-corpus.json rawMath2 [native-adapter.json]')
data=json.loads(Path(sys.argv[1]).read_bytes());corpus=json.loads(Path(sys.argv[2]).read_bytes());raw=Path(sys.argv[3]).read_bytes()
assert corpus['resourceSHA256']==hashlib.sha256(raw).hexdigest()
# Independent source geometry setup only, before its separate fixture assertions.
saved=sys.argv[:];sys.argv=['audit',sys.argv[1],sys.argv[3]]
source=root/'tools/LegacyMathOriginalFieldSource.Probe/audit_output.py'
exec(compile(source.read_text(encoding='utf-8-sig').split('data = json.loads')[0],str(source),'exec'),globals())
sys.argv=saved
samples={s['id']:s for s in corpus['samples']}
groups={
 '√':('sqrt',['</sqrt>']),'<frac>':('frac',['<fracbar>','</frac>']),
 '<slantedfrac>':('slantedfrac',['<slantedfracbar>','</slantedfrac>']),
 '<sup>':('sup',['</sup>']),'<sub>':('sub',['</sub>']),
 '<subsup>':('subsup',['<subsup2>','</subsup>']),'<under>':('under',['<under2>','</under>']),
 '<underover>':('underover',['<underover2>','<underover3>','</underover>']),
 '<fence>':('fence',['</fence>']),'<table>':('table',['</table>'])}
fences={'(':')','[':']','{':'}','|':'|','‖':'‖','⟨':'⟩'}

def syntax(tokens):
 if not tokens:raise ValueError('empty')
 # Build a nested syntax tree first; assign IDs only in a separate preorder walk.
 def sequence(start,stop=None,table=False,depth=1):
  if depth>32:raise ValueError('depth')
  at=start;children=[]
  while at<len(tokens):
   text=tokens[at]
   if not text:raise ValueError('encoding/empty')
   if text==stop:return ('sequence',start,at,children),at+1
   if text in groups:
    kind,markers=groups[text]
    if kind in('sup','sub','subsup')and not children:raise ValueError('base')
    if kind=='fence'and(at+1==len(tokens)or tokens[at+1]not in fences):raise ValueError('left fence')
    begin=at;at+=1;parts=[]
    for marker in markers:
     subtree,at=sequence(at,marker,kind=='table',depth+1)
     if subtree[1]==subtree[2]:raise ValueError('empty operand')
     parts.append(subtree)
    if kind=='fence'and(at-begin<4 or tokens[at-2]!=fences[tokens[begin+1]]):raise ValueError('right fence')
    children.append((kind,begin,at,parts))
   else:
    separator=text in('<&>','<\\>')
    if separator and not table or not separator and len(text)>2 and text.startswith('<')and text.endswith('>'):raise ValueError('marker')
    children.append(('tableSeparator'if separator else'token',at,at+1,[]));at+=1
  if stop is not None:raise ValueError('unclosed')
  return ('sequence',start,at,children),at
 tree,end=sequence(0);assert end==len(tokens);flat=[]
 def visit(n):
  ident=len(flat);flat.append(None);children=[visit(c)for c in n[3]]
  flat[ident]=dict(id=ident,kind=n[0],first=n[1],end=n[2],children=children);return ident
 visit(tree);return flat

count=nodes=ranges=0
for sample in data['actual']:
 independent=samples[sample['id']];e=ns['expected'](independent['ink'])
 assert sample['expectedPostInference']==independent['expectedPostInference']
 assert len(sample['runs'])==3
 for width,run in zip((1,3,16),sample['runs'],strict=True):
  count+=1;assert run['width']==width;result=run['result'];prior=next(r for r in independent['runs']if r['width']==width)
  row=prior['formattedRecords'][0]
  assert result['policy']==('ColdDefault'if width==1 else'PreparedSearchDiagnostic')
  assert result['confidence']is None and result['admission']=='Uncalibrated'
  assert result['scoreBits']==row['scoreBits']and result['frames']==prior['frames']
  assert [t['unit']for t in result['tokens']]==row['units']
  assert [t['utf8']for t in result['tokens']]==row['utf8']
  assert run['positions']==e['positions']and run['ends']==e['ends']
  expected_ranges=[[]for _ in row['units']];previous=0
  for index,end in run['runMap']:
   assert 0<=previous<=end<=len(e['positions']);part=position.select_ranges(e['positions'],e['ends'],e['originals'],previous,end)
   if index>=0:assert index<len(expected_ranges);expected_ranges[index]=union(expected_ranges[index],part)
   previous=end
  texts=[]
  for token,want in zip(result['tokens'],expected_ranges,strict=True):
   assert token['ranges']==want;ranges+=len(want);value=base64.b64decode(token['utf8'])
   text=value.rstrip(b'\0').decode('utf8');assert token['text']==text;texts.append(text)
  try:expected=syntax(texts)
  except ValueError:
   assert result['unsupported']and result['nodes']==[]and result['latex']is None
  else:
   assert result['nodes']==expected
   assert result['unsupported']is None and result['latex']==row['diagnosticLatex']
   nodes+=len(expected)
  assert result['request']==(result['latex']is not None and result['latex'].rstrip().endswith('='))
  assert result['explicitCalculation']==row['diagnosticEqualsGatedResult']
for control in data['preparedSyntax']:
 try:expected=syntax(control['tokens'])
 except ValueError:assert not control['valid']
 else:assert control['valid']and control['nodes']==expected
sample_count=17 if procedural_only else 31
assert len(data['actual'])==sample_count and count==sample_count*3
if procedural_only:assert all(s['id'].startswith('procedural-')for s in data['actual'])
baseline=next(s for s in data['actual']if s['id']=='procedural-definite-integral-1-10-x-squared-dx-equals')['runs'][0]['result']
events=[('BeforeInputCopy',-1),('BeforeInference',-1),('AfterInference',-1),('BeforeResultCopy',-1)]+[('AfterToken',i)for i in range(len(baseline['tokens']))]+[('BeforeSyntax',-1),('AfterSyntax',-1),('Complete',-1)]
assert len(data['prefixes'])==len(events)*2
for i,event in enumerate(events):
 for canceled,prefix in zip((False,True),data['prefixes'][i*2:i*2+2],strict=True):
  assert(prefix['stage'],prefix['index'],prefix['canceled'],prefix['observations'])==(*event,canceled,i+1)
assert data['detachedSourceCollected']and data['sourceBackedRetained']and data['disposeRace']and data['serializedConcurrentRequests']==2
assert data['guards']==72
if len(sys.argv)==5:assert Path(sys.argv[1]).read_bytes()==Path(sys.argv[4]).read_bytes()
print(f'PASS{sample_count}/{sample_count*3} actual FIRST outputs/{nodes} full AST nodes/{ranges} exact source ranges/{len(data["prefixes"])} fault prefixes/72guards/GC/close race;confidence null, no app/native semantic claim.')
