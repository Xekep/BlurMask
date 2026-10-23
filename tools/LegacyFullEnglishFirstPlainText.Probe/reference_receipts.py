"""STATIC full-source rich-glyph ingress receipts/budgets. No runtime materializer/NN/targets/vendor."""
from pathlib import Path
import argparse,ast,base64,hashlib,json,struct
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--b4',type=Path,required=True);p.add_argument('--b5',type=Path,required=True);p.add_argument('--a',type=Path,required=True);p.add_argument('--b1',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
def defs(path,names=None):
 t=ast.parse(path.read_text(encoding='utf-8-sig'));g={};exec(compile(ast.Module(body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))and(names is None or isinstance(n,(ast.Import,ast.ImportFrom))or n.name in names)],type_ignores=[]),str(path),'exec'),g);return g
Cases=defs(a.repo/'tools/LegacyFullEnglishFirstContext.Probe/audit_output.py',{'Cases'})['Cases'];raw=(a.models/'en-US/en_US-ak-cur.res').read_bytes();assert hashlib.sha256(raw).hexdigest()=='355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'
wire=defs(a.repo/'tools/LegacySource247Wire.Probe/audit_output.py');source=raw[8843:373957];sw,meta=wire['source'](source);fields={(v['owner'],v['destination']):base64.b64decode(v['raw'])for v in sw.fields};assert all(fields['parametersC0',n]==b'\0'for n in ['20','21','legacyThirdPresence','22']);assert fields['247','B9']==b'\0';processors=[c['Kind']for c in sw.children if c['destination'].startswith('parametersC0.58[')];assert processors==[0x24a,0x24c,0x265]
graph=next(c for c in sw.children if c['destination']=='247.138');gb=source[graph['Offset']:graph['End']];assert len(gb)==355 and gb[17]==1 and gb[350]==0
sampler,error=wire['sampler'](raw,dict(Offset=16952,End=18340,Version=0x108),217);assert error is None
pos=sampler['categoryOffset']+4
n=raw[pos];pos=(pos+4)&~3;first=list(struct.unpack_from('<'+'i'*n,raw,pos));pos+=4*n;fm=list(struct.unpack_from('<220b',raw,pos));pos+=220;n=raw[pos];pos=(pos+4)&~3;pos+=4*n;sm=list(struct.unpack_from('<220b',raw,pos));pos+=220;em=list(struct.unpack_from('<220b',raw,pos));composites=[];start=[]
for x,y,z in zip(fm,sm,em):
 mapped=[]
 for one,two in [(x,y),(x,z),(first.index(0),y),(first.index(0),z)]:
  if one<0 or two<0:mapped.append(-1);continue
  pair=(first[one],two)
  if pair not in composites:composites.append(pair)
  mapped.append(composites.index(pair))
 start.append(mapped[0])
assert len(composites)==42
f=lambda v:struct.pack('<f',v);totals=dict(actual=0,accepted=0,refused=0,children=0,traceCells=0,allocatable=0,hidden=0,missingCandidateScores=0);rows=[];oldA=Cases(a.a);oldB1=Cases(a.b1);b4=Cases(a.b4);b5=json.loads(a.b5.read_bytes())
for terminal,context,acoustic,confidence in zip(b5['Actual'],b4,oldA,oldB1):
 totals['actual']+=1;assert terminal['Name']==context['Name']==acoustic['Name']==confidence['Name']
 if terminal['Status']!='AcceptedTerminal':totals['refused']+=1;continue
 totals['accepted']+=1;C=len(terminal['Children']);T=G=P=V=Q=0;projections=[]
 assert len(context['Children'])==len(acoustic['Children'])==len(confidence['Children'])==C
 for term,ctx,ak,b1 in zip(terminal['Children'],context['Children'],acoustic['Children'],confidence['Children']):
  assert term['Descriptor']==ctx['Descriptor']==ak['Descriptor']==b1['Descriptor'];totals['children']+=1;P+=sum(len(s)for s in ctx['Raw']);ag={g['Group']:g for g in ak['Groups']};cg={g['Group']:g for g in ctx['Groups']};bc={g['Group']:g for g in b1['Groups']}
  chosen=term['Alternatives'][0]['Cells']if term['Alternatives']else[];tc=len(chosen);assert tc<=256;T+=tc;Q+=(tc+2)**2;groups=set();glyphs=[]
  for cell in chosen:
   n,r,col,arc,symbol,span,boundary,prev,pa=cell['Coordinates'];totals['traceCells']+=1
   group=ctx['Nodes'][n][arc]['Group'];groups.add(group)
   if symbol in [211,217]:totals['hidden']+=1;continue
   assert ag[group]['Final']==cg[group]['Costs'];values=bc[group]['Values'];assert len(values)==len(ag[group]['Candidates'])<=10 and list(map(f,values))==list(map(f,cg[group]['Confidence']))
   candidates=ag[group]['Candidates'];position=next((i for i,c in enumerate(candidates)if c[0]==symbol),None);score=values[position]if position is not None else 0.;totals['missingCandidateScores']+=position is None;firstScore=values[0]if values else 0.
   primary=base64.b64decode(ctx['Primary'][n]['Bytes'])[r*40:(r+1)*40];assert struct.unpack_from('<H',primary,12)[0]==symbol;composite=struct.unpack_from('<H',primary,14)[0]>>4&63;category=start[symbol];prototype=0 if category<0 else composites[category][1]
   glyphs.append(dict(node=n,arc=arc,symbol=symbol,group=group,primaryComposite=composite,publishedCategory=category,prototype=prototype,ordinal=65535,top=True,primitive=False,likelihoodC=f(primary[8]/255).hex(),confidence10=f(score).hex(),ownerFirst1C=f(firstScore).hex(),mismatch4=0 if symbol==180 or category==composite else 8,publication6=72 if symbol==209 else 64));totals['allocatable']+=1
  G+=len(groups)
  for group in groups:
   for s,lo,hi in cg[group]['Stored']:assert 0<=lo<hi<=len(ctx['Raw'][s]);V+=hi-lo
  projections.append(dict(descriptor=term['Descriptor'],traceCells=tc,rawLine=-1,derivedAssignedLine=0,glyphs=glyphs))
 assert C<=256 and T<=4096 and G<=4096 and P<=8192 and V<=1048576
 W=2*T;peak=8388608+8192*T+2048*G+2048*C+128*P+1024*W;work=16777216+32768*T+4096*G+4096*C+512*P+256*Q;visits=8192*T+1024*G+4096*C+64*Q+32*V
 assert peak<=64*1048576 and work<=512*1048576 and visits<=256*1048576
 rows.append(dict(name=terminal['Name'],C=C,T=T,G=G,P=P,V=V,Q=Q,peak=peak,work=work,visits=visits,children=projections))
assert next(b4,None)is None and next(oldA,None)is None and next(oldB1,None)is None and totals['actual']==len(b5['Actual'])
report=dict(status='STATIC_RECEIPTS_PASS',**totals,maximumTrace=max(c['traceCells']for r in rows for c in r['children']),maxima={k:max(r[k]for r in rows)for k in ['T','G','P','V','Q','peak','work','visits']},sourceSha256=hashlib.sha256(source).hexdigest(),graphSha256=hashlib.sha256(gb).hexdigest(),graph09=1,graph159=0,vendorExecuted=False,targetsRead=False,scope='Pre-implementation full B6 numeric ingress projection only, no published WORD/source720/default claim',rows=rows)
a.output.write_text(json.dumps(report,separators=(',',':'))+'\n');print(json.dumps({k:v for k,v in report.items()if k!='rows'}))
