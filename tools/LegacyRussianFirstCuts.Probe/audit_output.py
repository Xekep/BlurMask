from pathlib import Path
import ast,json,hashlib,sys
repo=Path(sys.argv[1]);capture=Path(sys.argv[2]);ref=repo/'tools/LegacyCursiveProducer.Probe/reference_controls.py';tree=ast.parse(ref.read_text());prefix=[]
for node in tree.body:
 if isinstance(node,ast.Assign)and any(isinstance(t,ast.Name)and t.id=='rng'for t in node.targets):break
 prefix.append(node)
namespace={'__file__':str(ref)};exec(compile(ast.Module(prefix,type_ignores=[]),str(ref),'exec'),namespace)
fn=next(n for n in tree.body if isinstance(n,ast.FunctionDef)and n.name=='make');fn.name='evaluate';fn.args=ast.arguments(posonlyargs=[],args=[ast.arg(arg=x)for x in ['strokes','height','baseline','confidence','whole','cap']],vararg=None,kwonlyargs=[],kw_defaults=[],kwarg=None,defaults=[ast.Constant(False),ast.Constant(None)])
body=[];seen=False
for node in fn.body:
 names=[t.id for t in node.targets if isinstance(t,ast.Name)]if isinstance(node,ast.Assign)else[]
 if 'tags'in names:seen=True
 if not seen or any(x in names for x in ['height','baseline','confidence','whole','cap']):continue
 body.append(node)
fn.body=body;module=ast.fix_missing_locations(ast.Module([fn],type_ignores=[]));exec(compile(module,str(ref),'exec'),namespace);evaluate=namespace['evaluate'];p=json.loads(capture.read_bytes());assert p['expectedLabelsRead']is False and p['inputSha256']=='c8fe150f3ca960cf7216da88c1dd90e1045aed2ef4652a8fa66c9e7a85353dce'
points=markers=0
for sample in p['samples']:
 source=[[[namespace['f'](v)for v in point]for point in stroke]for stroke in sample['source']]
 expected=evaluate(source,namespace['f'](sample['height']),namespace['f'](sample['baseline']),[namespace['f'](v)for v in sample['confidence']])
 assert expected['counts']==sample['counts'],('counts',sample['id'],expected['counts'],sample['counts'])
 assert len(expected['expected'])==len(sample['records']),('records',sample['id'])
 for i,(a,b)in enumerate(zip(expected['expected'],sample['records'])):
  if a!=b:raise AssertionError(('point',sample['id'],i,[(j,x,y)for j,(x,y)in enumerate(zip(a,b))if x!=y]))
 assert expected['markers']==sample['markers'],('markers',sample['id'],expected['markers'],sample['markers']);points+=len(sample['records']);markers+=len(sample['markers'])
 if int(sample['id'].split('-')[-1])%20==0:print('oracle',sample['id'],points,flush=True)
assert len(p['samples'])==p['samplesCount']==261 and points==p['recordsCount']
report=dict(status='PASS',scope='Independent scalar full191950stage-order record/marker audit on actual261 FIRST inputs; no target labels',samples=261,records=points,floatComponents=points*5,tagAndIdentityFields=points*3,markers=markers,expectedLabelsRead=False,captureSha256=hashlib.sha256(capture.read_bytes()).hexdigest());capture.with_name('cut-audit.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
