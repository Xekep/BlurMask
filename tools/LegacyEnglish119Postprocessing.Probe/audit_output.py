"""Independent actual EN raw wire/IRLS/deskew/shear/EM/dense spline audit.
No SDK/research code, expected labels or output-based parameter tuning.
"""
from pathlib import Path
import ast,base64,hashlib,importlib.util,json,math,struct,sys
import numpy as np
repo=Path(__file__).resolve().parents[2];tools=repo/'tools';f=lambda x:float(np.float32(x));dec=base64.b64decode

def load(folder):
 p=tools/folder/'reference_controls.py';spec=importlib.util.spec_from_file_location(folder,p);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
transfer=load('LegacyFirstResultLine.Probe');irls=load('LegacyCharacterLineFit.Probe');shear=load('LegacyOrdinaryShear.Probe');gap=load('LegacyOrdinaryGap.Probe');mixture=load('LegacyOrdinaryGapMixture.Probe')
#Only independent function definitions from the old dense solve/key oracle.
p=tools/'LegacyOrientationSmoothing.Probe/reference_controls.py';tree=ast.parse(p.read_text());env={};defs=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))or isinstance(n,ast.Assign)and isinstance(n.value,ast.Lambda)];exec(compile(ast.Module(body=defs,type_ignores=[]),str(p),'exec'),env)
p=tools/'LegacySmoothingPointKeys.Probe/reference_controls.py';tree=ast.parse(p.read_text());exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef)],type_ignores=[]),str(p),'exec'),env)
#Native021110's absolute determinant gate is NOT a conditioning test.
#Compute it independently from the weighted design matrix: native expression
#equals Q * det(A.T @ A + slope regularizer). Keep the dense NumPy solve.
original_dense_solve=irls.solve
def guarded_dense_solve(rows,bounds,force):
 design=[]
 for x,t,b,pt,pb,w in rows:
  root=math.sqrt(w);design.extend([[root,x*root,(pt+pb)*.5*root],[0.,0.,(pb-pt)*root]])
 matrix=np.asarray(design,dtype=np.float64).T@np.asarray(design,dtype=np.float64);matrix[1,1]+=1e-12
 if abs(float(np.linalg.det(matrix)*matrix[0,2]))<=1e-10:force=True
 return original_dense_solve(rows,bounds,force)
irls.solve=guarded_dense_solve
keys_oracle=env['keys'];i=lambda b,a:struct.unpack_from('<i',b,a)[0];u=lambda b,a:struct.unpack_from('<H',b,a)[0];fl=lambda b,a:struct.unpack_from('<f',b,a)[0]
if len(sys.argv)not in(3,4):raise SystemExit('EN-AK capture.json [RU-AK for prepared profile control]')
ak=Path(sys.argv[1]).read_bytes();assert hashlib.sha256(ak).hexdigest()=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657';got=json.loads(Path(sys.argv[2]).read_bytes());assert len(got['Actual'])==len(got['Post'])==141
#Independently decode this actual MAIN AL104 (not first AL103 or RU IDs).
p=99;header=list(struct.unpack_from('<8H',ak,p));p+=16;n=u(ak,p);p+=2
for inc in[32,64,1]:
 table=struct.unpack_from('<'+'H'*n,ak,p);p+=n*2;n=next(x for x in reversed(table)if x!=65535)+inc
spans=struct.unpack_from('<'+'H'*(n+1),ak,p);p+=(n+1)*2;entries=[struct.unpack_from('<II',ak,p+8*k)for k in range(spans[-1]+1)];p+=8*len(entries);blob=ak[p:p+entries[-1][0]];labels={blob[a:b].decode():lab&65535 for(a,lab),(b,_)in zip(entries,entries[1:])};markers=[labels.get(s,65535)for s in['\U000f0002','\U000f0003','-','_','\u30fc','\u4e00','\ufffd']]
first_forms=shear.alphabet(ak);assert len(first_forms)==header[-1]==217
node=ak[182191:182230];minimum=f(1e-6);low,high=struct.unpack_from('<2f',node,28)
shear_node=ak[182266:182304];min_g,max_g=shear_node[16:18];floor,normalizer,a,b,step=struct.unpack_from('<5f',shear_node,18);lo=f(math.tan(a));hi=f(math.tan(b));delta=f(f(hi-lo)/f(f(b-a)/step));shear_profile=[min_g,max_g,floor,normalizer,lo,hi,delta]
gapnode=ak[182304:182362];gap_profile=[*gapnode[40:42],*struct.unpack_from('<4f',gapnode,42)];gaussian=list(struct.unpack_from('<6f',gapnode,16));smoothing=list(struct.unpack_from('<8f',ak,182378));assert smoothing[2]==0
counts=dict(parents=0,guides=0,rows=0,points=0,wireFields=0,nonzeroShear=0,activeMixture=0,filteredPhysicalDeskew=0)
def close(a,b,what):np.testing.assert_allclose(a,b,rtol=2e-5,atol=3e-4,err_msg=what)
def ranges(record):return[(a[1],a[2],z[1],z[2])for a,z in record['Ranges']]
def check_rows(observed,expected,stage):
 assert len(observed)==len(expected)
 for a,b in zip(observed,expected):
  x,y=dec(a['Storage']),bytes(b);assert x[:24]==y[:24]and x[40:]==y[40:],stage;close(struct.unpack_from('<4f',x,24),struct.unpack_from('<4f',y,24),stage+'bbox')
for parent,post in zip(got['Actual'],got['Post']):
 assert parent['Name']==post['Name'];counts['parents']+=1;snaps={x['Stage']:x for x in post['Snapshots']};wire=post['Wire'];assert wire['CacheWidth']==220 and wire['PrefixEnd']==545497 and wire['SamplerOffset']==182412
 next_at=184155
 for field in wire['Fields']:
  size=field['Size']*field['Count'];raw=ak[next_at:next_at+size];assert field['Offset']==next_at and len(raw)==size and hashlib.sha256(raw).hexdigest().upper()==field['Sha256'];assert field['Bytes']is None and size>4096 or field['Bytes']is not None and dec(field['Bytes'])==raw;next_at+=size;counts['wireFields']+=1
 assert next_at==545680
 assert[(n['Inputs'],n['Outputs'])for n in wire['Networks']]==[(228,182),(210,182),(285,182),(285,182)]
 for n in wire['Networks']:assert hashlib.sha256(ak[n['Offset']:n['End']]).hexdigest().upper()==n['Sha256']
 original=parent['Retained'];segments=sum(len(st)-1 for st in original);orthogonal=sum(a[0]==b[0]or a[1]==b[1]for st in original for a,b in zip(st,st[1:]));corners=sum((a[0]==b[0]and b[1]==c[1])or(a[1]==b[1]and b[0]==c[0])for st in original for a,b,c in zip(st,st[1:],st[2:]));metrics=[f(orthogonal/segments)if segments else 0,f(corners/(segments-len(original)))if segments>len(original)else 0];assert struct.pack('<2f',*metrics)==struct.pack('<2f',*post['Metrics'])
 ds=[bytearray.fromhex(x)for x in parent['Descriptors']];rows=[bytearray(dec(x['Storage']))for x in parent['Rows']];pts=[[list(map(f,p))for p in st]for st in parent['Current']];guides=[];ordinary=[]
 minheight=f(f(f(parent['Resolution'])/f(25.4))*f(.75))if parent['Resolution']else f(1e-6)
 for ordinal,d in enumerate(ds):
  if u(d,0)&15==3:continue
  first,last=i(d,36),i(d,40);start,end=i(d,52),i(d,56);items=[]
  for row in rows[start:end]:
   item=transfer.transfer(row,markers);n=i(item,4);candidates=[]
   for k in range(n):
    weight,coefficient=struct.unpack_from('<2f',item,24+k*8);kind=i(item,184+k*4);top,atop=struct.unpack_from('<2f',item,264+k*8);bottom,abottom=struct.unpack_from('<2f',item,424+k*8);candidates.append([weight,coefficient,kind,top,atop,bottom,abottom])
   items.append(dict(known=bool(item[0]),clip=bool(item[1]),marker=bool(item[2]),bounds=list(struct.unpack_from('<4f',item,8)),candidates=candidates))
  profile=[node[36],node[37],low,high,minheight,float(np.finfo(np.float32).max),0];
  try:result=irls.run(dict(items=items,profile=profile,force=False,incoming=float(np.finfo(np.float64).max)))
  except ZeroDivisionError:
   #Independent design solve yielded zero height; native021110
   #returns no primary before the following normalization division.
   assert all(x['bounds'][2]==x['bounds'][3]for x in items)
   result=dict(score=0,output=[0,0,0])
  points=[p for st in pts[first:last+1]for p in st];minx,maxx=min(p[0]for p in points),max(p[0]for p in points);miny,maxy=min(p[1]for p in points),max(p[1]for p in points);fraction=f(f(maxy-miny)*f(.05));base,height,slope=result['output'];accepted=result['score']>0
  if not accepted or height<fraction:
   base=maxy if not accepted and f(maxx-minx)<minheight and f(maxy-miny)<minheight else f(f(maxy+miny)*f(.5));height=max(fraction,minheight);slope=0
  guide=bytearray(40);struct.pack_into('<5f',guide,0,base,height,slope,base,height);struct.pack_into('<ii',guide,32,-1,-2147483648);struct.pack_into('<i',d,16,len(guides));counts['guides']+=1
  cosine=f(1/f(math.sqrt(float(f(slope*slope))+1)));sine=f(math.sqrt(float(f(1-f(cosine*cosine)))));sine=-sine if slope>0 else sine;cx=f(f(maxx+minx)*f(.5));cy=f(f(cx*slope)+base);tx=f(f(f(cy*sine)-f(cx*cosine))+cx);ty=f(f(f(-sine*cx)-f(cy*cosine))+cy);newbase=f(f(f(base*cosine)+f(sine*0))+ty);newheight=max(minheight,f(f(f(f(f(base+height)*cosine)+f(sine*0))+ty)-newbase));struct.pack_into('<2f',guide,0,newbase,newheight)
  def transform(p):return[f(f(f(cosine*p[0])-f(sine*p[1]))+tx),f(f(f(sine*p[0])+f(cosine*p[1]))+ty)]
  for st in range(first,last+1):pts[st]=[transform(p)for p in pts[st]]
  for n in range(start,end):
   selected=[pts[s][k]for a,p,z,q in ranges(parent['Rows'][n])for s in range(a,z+1)for k in range(p if s==a else 0,(q if s==z else len(pts[s])-1)+1)];assert selected
   struct.pack_into('<4f',rows[n],24,min(p[0]for p in selected),max(p[0]for p in selected),min(p[1]for p in selected),max(p[1]for p in selected))
  guides.append(guide);snapshot=snaps['24B:'+str(ordinal)];assert snapshot['Descriptors']==[d.hex().upper()for d in ds];close(np.array([struct.unpack('<10f',bytes.fromhex(g))for g in snapshot['Guides']],dtype=float)[:,:8],np.array([struct.unpack('<10f',bytes(g))for g in guides],dtype=float)[:,:8],'24Bguide '+parent['Name']);check_rows(snapshot['Rows'],rows,'24B')
  for a,b in zip(snapshot['Points'],pts):close(a,b,'24B all physical')
  ordinary.append((len(guides)-1,start,end,first,last));counts['rows']+=end-start
 assigned=snaps['24A:assigned'];assert assigned['Guides']==snaps['24B:'+str(max(i for i,d in enumerate(ds)if u(d,0)&15!=3))]['Guides']
 #Actual pre-shear captured points are independently checked above. Consume
 #that exact float image to avoid multiplying harmless IRLS CRT tolerance.
 before=assigned['Points'];actualrows=assigned['Rows'];rr=[dict(ranges=ranges(row),box=list(struct.unpack_from('<4f',dec(row['Storage']),24)),label=struct.unpack_from('<h',dec(row['Storage']),40)[0],weight=fl(dec(row['Storage']),80))for row in actualrows];gg=[struct.unpack_from('<2f',bytes.fromhex(x))for x in assigned['Guides']];expected=shear.oracle(before,ordinary,gg,rr,[-1]*len(before),shear_profile,first_forms);close(post['Shear'],[expected[x]for x in['blend','gap','direction','shear']],'24Dscalar');assert post['Grid']==expected['iterations'];counts['nonzeroShear']+=expected['shear']!=0
 for a,b in zip(snaps['24D']['Points'],expected['points']):close(a,b,'24Dphysical')
 for row,box in zip(snaps['24D']['Rows'],expected['boxes']):close(struct.unpack_from('<4f',dec(row['Storage']),24),box,'24Dbbox')
 intervals=[d[:3]for d in ordinary];heights=[g[1]for g in gg];boxes=[list(struct.unpack_from('<2f',dec(x['Storage']),24))for x in snaps['24D']['Rows']];basic=gap.oracle(boxes,intervals,heights,-1,gap_profile)
 if basic is None:basic=mixture.active(gap_profile,gaussian,boxes,intervals,heights);counts['activeMixture']+=1
 close(post['Gap'],basic['gap'],'29Bgap');close(post['Gaps'],basic['observations'],'29Bobservations')
 before=snaps['24D']['Points'];out=[];ticks=[];offset=0
 for line,start,end,a,z in ordinary:
  xy,keys,_,_=keys_oracle(before[a:z+1],smoothing,heights[line],metrics);out.extend(xy);ticks.extend(k+offset*200 for k in keys);offset+=sum(map(len,before[a:z+1]))
 last=snaps['post265'];assert last['Keys']==ticks and last['Rows']==snaps['24D']['Rows'],'265 rewrote publication';counts['points']+=len(ticks)
 for a,b in zip(last['Points'],out):close(a,b,'dense EN265')
 #Compose retained current chronology independently using returned local200
 #positions; original ending metadata remains independently retained.
 flat=[m for st in parent['Metadata']for m in st];actual=[m for st in post['Metadata']for m in st]
 for tick,meta in zip(ticks,actual):
  whole,rem=divmod(tick,200);src=flat[whole if not rem else whole+1];assert meta['Key']==src['Key']and meta['End']==src['End']
 assert post['Trace'][-1]=='complete'
assert got['ReservationWinners']==got['ReservationLosers']==1
for fail in got['PreparedFailures']:assert fail['Trace'][-1]=='fault'and'complete'not in fail['Trace']

assert got['PreparedItemCap']==256 and got['PreparedRangeCap']==65536
for comparison in got['PreparedProfileComparisons']:
 post=next(x for x in got['Post']if x['Name']==comparison['Name']);stage=next(x for x in post['Snapshots']if x['Stage']=='24D');height=fl(bytes.fromhex(stage['Guides'][0]),4)
 ru=Path(sys.argv[3]if len(sys.argv)==4 else repo/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert hashlib.sha256(ru).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
 for name,profile in [('EN',smoothing),('RU',list(struct.unpack_from('<8f',ru,188124)))]:
  coords,ticks,_,weight=keys_oracle(stage['Points'],profile,height,post['Metrics']);close(comparison[name+'Weight'],weight,'prepared '+name+' weight');assert comparison[name+'Keys']==ticks
  for a,b in zip(comparison[name+'Points'],coords):close(a,b,'prepared '+name+' spline')
 assert comparison['ENWeight']!=comparison['RUWeight']
control=got['PreparedFilteredDeskew'];guide=bytes.fromhex(control['InputGuide']);base,height,slope=struct.unpack_from('<3f',guide);points=[p for st in control['Before']for p in st]
cosine=f(1/f(math.sqrt(float(f(slope*slope))+1)));sine=-f(math.sqrt(float(f(1-f(cosine*cosine)))));cx=f(f(max(p[0]for p in points)+min(p[0]for p in points))*f(.5));cy=f(f(cx*slope)+base);tx=f(f(f(cy*sine)-f(cx*cosine))+cx);ty=f(f(f(-sine*cx)-f(cy*cosine))+cy)
for before,after in zip(control['Before'],control['After']):
 expected=[[f(f(f(cosine*p[0])-f(sine*p[1]))+tx),f(f(f(sine*p[0])+f(cosine*p[1]))+ty)]for p in before];close(after,expected,'prepared wholephysical projection')
assert control['Before'][1]!=control['After'][1]
#An explicit synthetic guide tests the shared numeric projection; natural
#filtered captures retain their real identity guide and are not relabeled.
print('PASS separately named prepared EN/RU265 profiles, filtered physical projection, boundary/refusal and cancellation/race controls')
print('PASS independent actual EN wire/fullfield partition/AL217/IRLS/allphysicaldeskew/24D/29B/dense265/mapping',counts)

#V2: arithmetic-only adversarial controls; no large arrays are allocated.
assert got['ProductionComparisons']==141 and got['FailureComparisons']==7
budget=got['MaximumCopyBudget']
image=lambda d,o,s,r:512+d*160+o*112+s*144+4096*8+4096*8+o*8+r*8
assert budget['ImageBytes']==image(511,256,256,4096)==248160
assert budget['ProductionPeak']==2*budget['ImageBytes']==496320<=512*1024
assert budget['Images']==516 and budget['CopyWork']==budget['Images']*budget['ImageBytes']<=128*1024*1024
assert budget['CopyWork']>32*1024*1024
assert budget['DiagnosticBoundaryOrdinary']==72
assert budget['DiagnosticBoundaryWork']==image(511,72,256,4096)*(2*72+4)<=32*1024*1024
assert image(511,73,256,4096)*(2*73+4)>32*1024*1024
assert got['Guards']==1040
print('PASS V2 production/diagnostic141 finals+7 fault prefixes and conservative bounded copy-work arithmetic')
