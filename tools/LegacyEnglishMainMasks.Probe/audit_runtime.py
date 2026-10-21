"""Actual EN MAIN119 ownership/masks oracle. No expected text is read; raw models only."""
from pathlib import Path
import argparse,subprocess,sys,json,struct,hashlib,tempfile
p=argparse.ArgumentParser();p.add_argument('--models',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
with tempfile.TemporaryDirectory()as td:
 out=Path(td)/'raw.json';subprocess.run([sys.executable,'-B',str(Path(__file__).with_name('reference_actual.py')),str(a.models),str(out)],check=True);raw=json.loads(out.read_bytes())
c=json.loads(a.capture.read_bytes());rows=c['Rows'];assert len(rows)==134 and len(c['MultilineRefused'])==7 and len({r['Name']for r in rows})==134
assert c['Guards']==569 and c['Cancellations']==16 and c['MaximumLogicalCopyBytes']<=1048576
m=raw['sourceMaps'];mask=raw['maskDiagnostic'];seen={};context=[]
for value,enabled in zip(m['primary'],m['contextMapNonzero']):
 if not enabled:context.append(0)
 else:
  if value not in seen:seen[value]=len(seen)+1
  context.append(seen[value])
f32=lambda v:struct.pack('<f',v)
for row in rows:
 assert row['Source720Before']==row['Source720After']and len(row['Source720Before'])==64
 r=row['Masks'];assert r['Scope']=='independent-main13B-only/nonbypassed/no-user-restriction'
 for field,expected in [('PrimaryMap',m['primary']),('AuxiliaryMap',m['auxiliary']),('ContextMap',context),('Seed',mask['seed']),('Primary',mask['primary']),('Secondary',mask['secondary'])]:assert r[field]==expected,(row['Name'],field)
 for field,expected in [('DescriptorFlags',3086),('LanguageOrder',3),('SelectedPriority',1024),('SelectedSourceIndex',0),('ContextCount',86),('AcousticSymbolCount',217),('NeuralClassCount',182),('CacheWidth',220),('HasContextualForms',False),('WidePairCount',0),('RelatedCaseRelaxationEnabled',True),('SizeCostsEnabled',True)]:assert r[field]==expected,(row['Name'],field)
 assert 0<r['LogicalCopyWorkBytes']<=c['MaximumLogicalCopyBytes']
 assert len(r['Groups'])==7
 for actual,expected in zip(r['Groups'],raw['groups']):assert actual['Symbols']==expected['symbols']and f32(actual['Weight'])==f32(expected['weight'])
 for field,name in [('FirstRelatedCaseMultiplier','84'),('SecondRelatedCaseMultiplier','88'),('DotSizeDivisor','BC'),('PunctuationSizeDivisor','C0'),('CachePower','100')]:assert f32(r[field])==f32(raw['values'][name])
result=dict(status='PASS',actualOwners=134,multilineRefused=7,guards=c['Guards'],deterministicCancellation=c['Cancellations'],captureSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),oracle='Raw119 groups/parameters, raw104/217 map, relational86 context sharing, rawScript-derived noforms/wide, exact ordered seed/primary/secondary allarrays',scope='Independent main13B-only masks; no mainNN/fullroot/winner/accuracy claim')
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result))
