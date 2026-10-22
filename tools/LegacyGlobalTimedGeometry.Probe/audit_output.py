"""Independent ordinary2DB numeric reference from retained native operands.
No SDK execution, C# invocation, expected transcription input or capture-derived
expected geometry. Live CI independently checks the paired current raw child
and parent; historical static callers retain their exact frozen prerequisite.
"""
import argparse,base64,hashlib,json,math,runpy,struct,sys
from pathlib import Path
p=argparse.ArgumentParser(description=__doc__)
p.add_argument('recorded',type=Path);p.add_argument('parent',type=Path);p.add_argument('capture',type=Path)
p.add_argument('--native',type=Path);p.add_argument('--report',type=Path)
p.add_argument('--models',type=Path);p.add_argument('--child',type=Path);a=p.parse_args()
if (a.models is None)!=(a.child is None):p.error('--models and --child must be supplied together')
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
b32=lambda x:struct.unpack('<i',struct.pack('<f',x))[0]
b64=lambda x:struct.unpack('<q',struct.pack('<d',x))[0]
decode32=lambda x:struct.unpack('<f',struct.pack('<i',x))[0]
def exact32(actual,expected,label):assert actual is not None and b32(actual)==b32(expected),(label,actual,expected)
def exact64(actual,expected,label):assert actual is not None and b64(actual)==b64(expected),(label,actual,expected)
def rotate(point,c,s):
 x,y=point;return f(y*s+x*c),f(y*c-x*s)
def gaussian(m1,s1,m2,s2):
 if abs(s1)>=2**-23 and abs(s2)>=2**-23:
  v1,v2=s1*s1,s2*s2
  aa=1/(v1+v1)-1/(v2+v2);bb=m2/v2-m1/v1
  try:log=math.log(s2/s1)
  except ValueError:log=float('nan')
  dd=bb*bb-((m1*m1/(v1+v1)-m2*m2/(v2+v2))-log)*aa*4
  if dd>=0 and abs(aa)>=2**-23:
   root=math.sqrt(dd)
   for value in ((root-bb)/(aa+aa),(-bb-root)/(aa+aa)):
    if m1<value<m2:return value
 return(m1+m2)*.5

# Native scratch64 memory, not C# object fields or kernel output replay.
class Scratch:
 def __init__(self):self.data=bytearray(64);self.put('i',0,-1);self.put('i',4,-1)
 def get(self,fmt,at):return struct.unpack_from('<'+fmt,self.data,at)[0]
 def put(self,fmt,at,value):struct.pack_into('<'+fmt,self.data,at,value)
 def previous(self):return self.get('i',0)
 def following(self):return self.get('i',4)
 def mean(self):return self.get('d',8)
 def sd(self):return self.get('d',16)

def native_chain(scratch):
 # All admitted source rows have skip20=0. Keep the original direction
 # threshold, anchor/bridge state and native invalid-chain fallback.
 n=len(scratch);trend=0
 for j in range(1,n):
  old=scratch[j-1];now=scratch[j]
  if old.mean()+old.sd()*f(.8)<now.mean():trend=1;break
  if now.mean()<old.mean()-old.sd()*f(.8):trend=-1;break
 if not trend:return list(range(n)),False
 # Register names retain the independent native branch translation.
 r52=0;r21=-1;r36=-1;r32=-1;last=0
 for r42 in range(1,n):
  mean=scratch[r42].mean();prior=scratch[r52]
  if mean>prior.mean()+prior.sd()*f(.8):trend=1
  elif mean<prior.mean()-prior.sd()*f(.8):trend=-1
  branch='changed'
  if trend==last:branch='same'
  elif trend<0 and last and r21!=-1 and scratch[r21].mean()-scratch[r21].sd()*f(.8)<=mean:
   trend=last;branch='same'
  elif trend>=0 and last and r36!=-1 and scratch[r36].mean()-scratch[r36].sd()*f(.8)<=mean:
   trend=last;branch='same'
  if branch=='same':r36=r21 #native04EF15 BEFORE04EE66/04EF2D
  if trend<0:
   if branch=='changed':r36=-1;r32=r21;last=trend
   r21=r36
   destination=r52 if r32==-1 else r32
   scratch[r42].put('i',4,destination);scratch[destination].put('i',0,r42)
   if r32!=-1:r32=-1
   r36=r52;trend=last
  else:
   if branch=='changed':r21=r52;r32=r36;last=trend
   scratch[r42].put('i',0,r52);scratch[r52].put('i',4,r42)
   if r32!=-1:scratch[r42].put('i',4,r32);scratch[r32].put('i',0,r42)
   r36=r52;trend=last
  r52=r42
 heads=[i for i,s in enumerate(scratch)if s.previous()==-1]
 tails=[i for i,s in enumerate(scratch)if s.following()==-1]
 if len(heads)!=1 or len(tails)!=1:return list(range(n)),False
 order=[];cursor=heads[0]
 while len(order)<n:
  if cursor<0 or cursor>=n or cursor in order:return list(range(n)),False
  order.append(cursor);cursor=scratch[cursor].following()
 return order,tails[0]<heads[0]

def raw_views(raw,parent):
 result=[]
 specs=[(False,l['node'],l['processed'])for l in parent['links']]+[(True,parent['node'],parent['wordKey'])]
 for word,node,ranges in specs:
  for number,(start,end)in enumerate(ranges):
   first_source,last_source=start>>32,end>>32
   for obj in range(first_source,last_source+1):
    points=raw[obj];pa,fa,ma=(start>>16)&65535,(start>>8)&255,start&255
    pb,fb,mb=(end>>16)&65535,(end>>8)&255,end&255
    crop0=obj==first_source and(pa,fa,ma)!=(0,0,3);crop1=obj==last_source and fb!=255
    lo=pa if crop0 else 0;hi=pb if crop1 else len(points)-1
    frac0=f(fa*f(.005))if crop0 and fa!=255 else 0.;frac1=f(fb*f(.005))if crop1 else 0.
    def at(index,t):return [f(f(f(points[index+1][k]-points[index][k])*t)+points[index][k])for k in(0,1)]
    output=[at(k,frac0)if k==lo and frac0 else points[k]for k in range(lo,hi+1)]
    if frac1:output.append(at(hi,frac1))
    result.append(dict(word=word,node=node,range=number,object=obj,kind=0 if not crop0 and not crop1 else 1 if frac0==frac1==0 else 2,
     first=lo,last=hi,fraction0=b32(frac0),fraction1=b32(frac1),include0=not crop0 or ma==3,include1=not crop1 or mb==5,
     points=[[b32(x),b32(y)]for x,y in output]))
 return result

def weighted(points,center,height,cosine,sine,initial=None):
 count,sumx,square,lo,hi=initial if initial is not None else(0.,0.,0.,f(3.4028234663852886e38),-f(3.4028234663852886e38))
 values=[]
 for point in points:
  x,y=rotate(point,cosine,sine);distance=abs(f(y-center))
  weight=f(1 if distance<=height else .7 if distance<=f(height+height)else .3)
  values.append((x,weight));lo=min(lo,x);hi=max(hi,x)
 at=0
 while at+3<len(values):
  (x1,w1),(x2,w2),(x3,w3),(x4,w4)=values[at:at+4]
  p1,p2,p3,p4=w1*x1,w2*x2,w3*x3,w4*x4
  count=w4+w3+w2+w1+count;sumx=sumx+p1+p2+p3+p4
  square=p4*x4+p3*x3+p2*x2+p1*x1+square;at+=4
 for x,w in values[at:]:
  product=w*x;count=w+count;sumx=sumx+product;square=product*x+square
 return count,sumx,square,lo,hi

def geometry(sample,parent,raw,temporal):
 output=sample['result'];assert output['ready'] and not output['faulted']
 row=bytes.fromhex(parent['sourceLines56']);gx,gy,angle,height=struct.unpack_from('<4d',row,8)
 height=f(height);co,si=math.cos(angle),math.sin(angle)
 for key,value in [('meanAngle',angle),('cosine',co),('sine',si)]:exact64(output[key],value,key)
 views=raw_views(raw,parent);assert len(views)==len(sample['views'])
 for expected,actual in zip(views,sample['views']):
  obj=expected['object'];times=temporal[obj];t0=times['baseTag']
  if expected['kind']!=0:
   i=expected['first'];t0=times['originalSampleTimes'][i];fraction=decode32(expected['fraction0'])
   if expected['kind']==2 and fraction:
    next_time=times['originalSampleTimes'][i+1];t0=int(fraction*(float(next_time)-float(t0))+float(t0)+.5)
  expected['startTime']=t0;assert actual==expected,(sample['name'],'actual source view',actual,expected)
 points=[rotate([decode32(p[0]),decode32(p[1])],co,si)for v in views if v['word']for p in v['points']]
 n=sy=sy2=0.;xmin=ymin=f(3.4028234663852886e38);xmax=ymax=-xmin
 for x,y in points:
  xmin=min(xmin,x);xmax=max(xmax,x);ymin=min(ymin,y);ymax=max(ymax,y)
  n+=1.;sy+=y;sy2=y*y+sy2
 ym=f(sy/n);variance=(sy2*n-sy*sy)/(n*n);ys=f(math.sqrt(variance)if variance>0 else 0)
 tangent=math.sin(angle-angle)/math.cos(angle-angle)
 rx=f(co*f(gx)+si*f(gy));ry=f(co*f(gy)-si*f(gx));intercept=ry-rx*tangent
 baseline0=f(xmin*tangent+intercept);baseline1=f(xmax*tangent+intercept)
 lower,upper=(baseline1,baseline0)if tangent<0 else(baseline0,baseline1)
 top=min(ymin,f(lower-f(height*f(2.5))));bottom=max(ymax,f(f(height*f(1.5))+upper))
 vm=output['verticalMoments']
 for key,value in [('count',n),('sumY',sy),('sumY2',sy2)]:exact64(vm[key],value,'vertical '+key)
 for key,value in [('minimumX',xmin),('maximumX',xmax),('minimumY',ymin),('maximumY',ymax)]:exact32(vm[key],value,'vertical '+key)
 line=output['line'];assert(line['sourceLine'],line['characterStart'],line['characterEnd'])==(0,0,len(parent['links']))
 for key,value in [('minimumX',xmin),('maximumX',xmax),('meanY',ym),('standardDeviationY',ys),('baselineAtMinimumX',baseline0),('baselineAtMaximumX',baseline1)]:exact32(line[key],value,'line '+key)
 assert [b32(x)for x in output['verticalBoundaries']]==[b32(top),b32(bottom)]
 cursor=0;states=[];locals=[];selected=[];flags=[];utf8=b''
 for i,link in enumerate(parent['links']):
  rr=link['records'][link['marker']];scalar=bytes.fromhex(rr['scalar40']);sf=struct.unpack_from('<I',scalar,8)[0]
  selected.append(sf);flags.append(link['metadataFlags']);utf8+=bytes.fromhex(rr['utf8'])
  vc=sum((end>>32)-(begin>>32)+1 for begin,end in link['processed']);block=views[cursor:cursor+vc];cursor+=vc
  assert block and all(not v['word']and v['node']==link['node']for v in block)
  count=sx=sx2=0.;lo=f(3.4028234663852886e38);hi=-lo
  for v in block:
   pts=[[decode32(p[0]),decode32(p[1])]for p in v['points']]
   #Preserve per-view unroll4 groups;20-point C4B chunk boundaries are multiples4.
   count,sx,sx2,lo,hi=weighted(pts,ym,height,co,si,(count,sx,sx2,lo,hi))
  mean=sx/count;var=(sx2*count-sx*sx)/(count*count);std=math.sqrt(var)if var>0 else 0.
  std=max(std,height*.05);s=Scratch();s.put('d',8,mean);s.put('d',16,std);s.put('f',24,lo);s.put('f',28,hi)
  s.put('q',48,min(v['startTime']for v in block));s.put('q',56,max(v['startTime']for v in block));states.append(s)
  locals.append((count,sx,sx2));character=output['characters'][i]
  assert(character['sourceNode'],character['marker'],character['selectionId'],character['lastUtf8ByteIndex'],character['wordIndex'],character['sourceLine'])==(link['node'],link['marker'],link['selectionId'],len(utf8)-1,0,0)
  assert character['structureWritten'] and character['initialMetadata']==link['metadataFlags'] and character['selectedFlags']==sf
  for key,value in [('minimumX',lo),('maximumX',hi),('meanX',f(mean)),('standardDeviationX',f(std))]:exact32(character[key],value,'CHAR'+str(i)+' '+key)
  assert(character['minimumViewStart'],character['maximumViewStart'])==(s.get('q',48),s.get('q',56))
 assert base64.b64decode(output['selectedUtf8'])==utf8
 order,reversed_=native_chain(states);assert line['reversed']==reversed_
 def endpoint(i,right=False):
  st=states[i];mean,std=st.mean(),st.sd();margin=f(.25)if not flags[i]&0x1000000 and selected[i]&0xc0000 else f(.1)
  if right:
   value=f(f(margin*height)+st.get('f',28));limit=f(std*2.8+mean)
   if limit<=value:value=limit
   if selected[i]&0x10000:
    half=f(f(height*f(.5))+f(mean))
    if value<=half:value=half
  else:
   value=f(mean-std*2.8);limit=f(st.get('f',24)-f(margin*height))
   if value<=limit:value=limit
   if selected[i]&0x10000:value=min(value,f(f(mean)-f(height*f(.5))))
  return value
 coords=[endpoint(order[0])]+[f(gaussian(states[i].mean(),states[i].sd(),states[j].mean(),states[j].sd()))for i,j in zip(order,order[1:])]+[endpoint(order[-1],True)]
 assert [b['characterIndex']for b in output['horizontalBoundaries']]==order+[order[-1]]
 for at,value in enumerate(coords):exact32(output['horizontalBoundaries'][at]['coordinate'],value,'boundary'+str(at))
 widths=[f(coords[i+1]-coords[i])for i in range(len(order))]
 total=count=0.
 for index,width in zip(order,widths):
  if selected[index]&0x7000==0:count+=1.;total+=width
 mean_width=f(total/count)if count else 0.;spacing=f(mean_width*f(1.2))if 0<f(mean_width*f(.1))else 0.
 mutations=[]
 for index,width in zip(order,widths):
  if selected[index]&0x7000==0x1000 and width>f(mean_width*f(4)):
   old=flags[index];source=parent['links'][index]['node']
   for other,link in enumerate(parent['links']):
    if link['node']==source:flags[other]|=0x2000000
   mutations.append(dict(ordinal=len(mutations),characterIndex=index,sourceNode=source,before=old,after=flags[index],stage='04E0E0'))
 assert output['mutations']==mutations
 for i,s in enumerate(states):
  observed=output['scratch'][i];assert(observed['characterIndex'],observed['written'],observed['previous'],observed['next'])==(i,15,s.previous(),s.following())
  assert observed['skipped']is False and observed['special']is False and observed['gapClass']==bool(selected[i]&0x28000)
  for key,value in zip(('weightedCount','weightedSumX','weightedSumX2'),locals[i]):exact64(observed[key],value,'scratch'+str(i)+key)
  for key,value in [('meanX',s.mean()),('standardDeviationX',s.sd())]:exact64(observed[key],value,key)
  for key,value in [('minimumX',s.get('f',24)),('maximumX',s.get('f',28))]:exact32(observed[key],value,key)
  assert(observed['minimumViewStart'],observed['maximumViewStart'])==(s.get('q',48),s.get('q',56))
  assert output['characters'][i]['effectiveMetadata']==flags[i]
 word=output['word'];assert(word['sourceNode'],word['chosenIndex'],word['selectionId'],word['characterStart'],word['characterEnd'],word['lineStart'],word['lineEnd'])==(parent['node'],0,parent['wordSelection'],0,len(states),0,1)
 assert(word['minimumViewStart'],word['maximumViewStart'])==(min(s.get('q',48)for s in states),max(s.get('q',56)for s in states))
 for key,value in [('globalMinimumX',coords[0]),('globalMaximumX',coords[-1]),('meanWidth',mean_width),('meanSpacing',spacing)]:exact32(output[key],value,key)
 assert output['trace']==['structure','rotation','vertical','04D2C0','04E0E0','04DA20','04DD50','horizontal','ready']
 return len(states),len(views)

parent_raw=a.parent.read_bytes()
if a.models is None:
 # Historical static-closure callers retain their exact frozen prerequisite.
 assert hashlib.sha256(parent_raw).hexdigest()=='1f2d2f6e2bb57e6d249fc11325a5e9736f358a5cd9dce1fe2664dd8ecdebea58'
else:
 # Live CI validates the current parent against its independently audited raw
 # child, rather than pinning OCR alternatives from an older implementation.
 oracle=Path(__file__).resolve().parents[1]/'LegacyGlobalWordText.Probe/audit_output.py'
 saved=sys.argv
 try:
  sys.argv=[str(oracle),str(a.models),str(a.child),str(a.parent)]
  runpy.run_path(str(oracle),run_name='__main__')
 finally:sys.argv=saved
parents={s['name']:s for s in json.loads(parent_raw)['actual']}
m=[[0.,90.],[0.,10.],[25.,60.],[50.,10.],[50.,90.]]
raws={'M':[m],'MIR':[m,[[70.,10.],[70.,90.],[120.,10.],[120.,90.]],[[140.,90.],[140.,10.],[180.,10.],[190.,20.],[190.,40.],[180.,50.],[140.,50.]]],
 'I':[[[0.,10.],[0.,90.]]],'II':[[[0.,10.],[0.,90.]],[[40.,10.],[40.,90.]]]}
policies={name:[dict(baseTag=b,channelMs=[],kind=0)for b in bases]for name,bases in [('M',[0]),('MIR',[0,1_000_000,2_000_000]),('I',[0]),('II',[0,1_000_000])]}
manifest=json.loads(a.recorded.read_bytes());assert manifest['commit']=='c6629026b1fdef51fd08e3d2fddffd82f97e60a5'
for entry in manifest['samples']:
 if entry['kind']!='word':continue
 name=entry['file'];data=(a.recorded.parent/name).read_bytes();assert hashlib.sha256(data).hexdigest()==entry['trackSha256']
 record=json.loads(data);raws[name]=[[[f(x),f(-f(y))]for x,y in record['points']]]
 elapsed=0;times=[]
 for interval in record['intervalsMs']:elapsed+=interval;times.append(elapsed)
 policies[name]=[dict(baseTag=times[0]*1000,channelMs=[t-times[0]for t in times],kind=1)]
del manifest
capture=json.loads(a.capture.read_bytes());assert len(capture['samples'])==13 and capture['untimedParentsRefused']==13
characters=views=0
for sample in capture['samples']:
 name=sample['name'];assert sample['genuineMeasuredTrack']==name.endswith('.track.json')
 expected=policies[name]
 for i,(source,policy)in enumerate(zip(sample['physicalSources'],expected)):
  assert source['source']==i and source['points']==len(raws[name][i])and source['rateBits']==b32(1000.)and source['period']==1000
  assert all(source[key]==value for key,value in policy.items())
  assert(source['nativeChannel'],source['nativeMultiplier'])==((4,1000)if policy['kind']else(None,None))
  times=[policy['baseTag']+(policy['channelMs'][p]*1000 if policy['kind']else p*1000)for p in range(source['points'])]
  assert source['originalSampleTimes']==times
  policy['originalSampleTimes']=times
 n,v=geometry(sample,parents[name],raws[name],expected);characters+=n;views+=v
assert(characters,views)==(37,54)
controls=capture['preparedControls'];assert controls['guards']==73
assert len(controls['fractionalControls'])==175
for first,next_time,fraction,actual in controls['fractionalControls']:
 fraction=decode32(fraction);expected=first if fraction==0 else int(fraction*(float(next_time)-float(first))+float(first)+.5)
 assert actual==expected,('prepared native fractional timing',first,next_time,fraction,actual,expected)
for rate,actual in controls['periodControls']:assert actual==int(f(f(1_000_000./decode32(rate))+.5))
assert len(controls['weightedControls'])==30
for item in controls['weightedControls']:
 points=[(f((i%7-3)*.375),f((i%9-4)*.5))for i in range(item['n'])]
 count,total,square,lo,hi=weighted(points,.125,.5,1.,0.)
 for key,value in [('count',count),('sum',total),('sum2',square)]:exact64(item[key],value,'prepared weighted '+key)
 for key,value in [('min',lo),('max',hi)]:exact32(item[key],value,'prepared weighted '+key)
assert len(controls['orderControls'])==144
assert len(controls['gaussianControls'])==324
for first,sd1,second,sd2,actual in controls['gaussianControls']:
 exact64(actual,gaussian(first,sd1,second,sd2),'prepared Gaussian')
assert len(controls['endpointControls'])==120
for metadata,selected,mean,sd,actual_left,actual_right in controls['endpointControls']:
 margin=f(.25)if not metadata&0x1000000 and selected&0xc0000 else f(.1)
 left=f(mean-sd*2.8);limit=f(-2-f(margin*2))
 if left<=limit:left=limit
 if selected&0x10000:
  half=f(f(mean)-1)
  if not left<=half:left=half
 right=f(f(margin*2)+2);limit=f(sd*2.8+mean)
 if limit<=right:right=limit
 if selected&0x10000:
  half=f(1+f(mean))
  if right<=half:right=half
 exact32(actual_left,left,'prepared endpoint left');exact32(actual_right,right,'prepared endpoint right')
for number,item in enumerate(controls['orderControls']):
 states=[]
 for mean,std in item['rows']:
  state=Scratch();state.put('d',8,mean);state.put('d',16,std);states.append(state)
 order,reversed_=native_chain(states)
 assert(item['order'],item['reversed'])==(order,reversed_),('prepared chain',number,item,order,reversed_)
 assert item['links']==[[s.previous(),s.following()]for s in states],('prepared chain native memory',number)
assert len(controls['prefixes'])==12
prepared_horizontal={
 'equal-threshold':([[-7,7],[-1,1]],[0x1000,0]),
 'strict-greater':([[-7.25,7.25],[-1,1]],[0x1000,0]),
 'no-eligible':([[-1,1]],[0x1000]),
 'zero-equality':([[0,0],[0,0],[0,0]],[0,0x1000,0]),
 'reverse-insertion':([[9,11],[-17,-3]],[0,0x1000]),
 'alias-equal':([[-7,7],[-1,1]],[0x1000,0]),
 'alias-write':([[-7.25,7.25],[-1,1]],[0x1000,0]),
 'alias-reverse':([[9,11],[-17,-3]],[0,0x1000]),
 'distinct-equal-node-key':([[-7.25,7.25],[-1,1]],[0x1000,0])}
assert len(controls['preparedHorizontalControls'])==18
for item in controls['preparedHorizontalControls']:
 points,selected=prepared_horizontal[item['kind']];assert(item['points'],item['selected'])==(points,selected)
 aliases=item['kind'].startswith('alias-');assert item['aliasesExactPayload']==aliases
 states=[]
 for pair in points:
  count,total,square,lo,hi=weighted([(x,0)for x in pair],0,10,1,0)
  mean=total/count;variance=(square*count-total*total)/(count*count);std=max(math.sqrt(variance)if variance>0 else 0,.5)
  state=Scratch();state.put('d',8,mean);state.put('d',16,std);state.put('f',24,lo);state.put('f',28,hi);states.append(state)
 order,reversed_=native_chain(states);assert item['reversed']==reversed_
 left=states[order[0]];right=states[order[-1]]
 coords=[max(f(left.mean()-left.sd()*2.8),f(left.get('f',24)-f(f(.1)*10)))]+[f(gaussian(states[i].mean(),states[i].sd(),states[j].mean(),states[j].sd()))for i,j in zip(order,order[1:])]+[min(f(f(f(.1)*10)+right.get('f',28)),f(right.sd()*2.8+right.mean()))]
 assert [pair[1]for pair in item['boundaries']]==order+[order[-1]]
 for (observed,_),expected in zip(item['boundaries'],coords):exact32(observed,expected,'prepared horizontal boundary')
 widths=[f(coords[i+1]-coords[i])for i in range(len(order))]
 eligible=[width for index,width in zip(order,widths)if selected[index]&0x7000==0]
 mean_width=f(sum(eligible)/len(eligible))if eligible else 0
 expected_space=f(mean_width*f(1.2))if f(mean_width*f(.1))>0 else 0
 exact32(item['meanWidth'],mean_width,'prepared mean width');exact32(item['meanSpacing'],expected_space,'prepared mean space')
 flags=[0x41000000]*len(points);writes=[]
 for index,width in zip(order,widths):
  if selected[index]&0x7000==0x1000 and width>f(mean_width*4):
   writes.append([index,flags[index],flags[index]|0x2000000,'04E0E0'])
   for other in range(len(flags)):
    if aliases or other==index:flags[other]|=0x2000000
 assert item['effectiveMetadata']==flags and item['writes']==writes
 assert item['historicalInitialMetadata']==[0x41000000]*len(points)
 assert item['lazyStageMetadata']==[flags]*(1 if item['failAfterWidth']else 2)
 assert item['characterMeans']==[f(s.mean())for s in states]
 assert item['stages']==(['04D2C0','04E0E0','04DA20']if item['failAfterWidth']else['04D2C0','04E0E0','04DA20','04DD50'])
 if item['kind']in('equal-threshold','alias-equal'):assert widths[0]==f(mean_width*4)and not writes
 if item['kind']in('strict-greater','alias-write','distinct-equal-node-key'):assert writes==[[0,0x41000000,0x43000000,'04E0E0']]
 if item['kind']=='alias-write':assert flags==[0x43000000,0x43000000]
 if item['kind']=='distinct-equal-node-key':assert flags==[0x43000000,0x41000000]
 if item['kind']=='no-eligible':assert mean_width==0 and len(writes)==1
full=next(s['result']for s in capture['samples']if s['name']=='M')
stages=['structure','rotation','vertical','04E0E0','04DA20','04DD50']
for prefix in controls['prefixes']:
 boundary=prefix['boundary'];result=prefix['snapshot'];stage=stages[boundary-1]
 assert result['stage']==stage and result['faulted']and not result['ready']
 at=full['trace'].index(stage);assert result['trace']==full['trace'][:at+1]+['failed:'+stage]
 assert result['selectedUtf8']==full['selectedUtf8']
 assert result['word']==dict(full['word'],minimumViewStart=None,maximumViewStart=None)
 for key in ['meanAngle','cosine','sine']:assert result[key]==(full[key]if boundary>=2 else None)
 for key in ['verticalMoments','verticalBoundaries']:assert result[key]==(full[key]if boundary>=3 else None if key=='verticalMoments'else [])
 for char,complete in zip(result['characters'],full['characters']):
  expected=dict(complete)
  if boundary<4:
   expected.update(minimumX=None,maximumX=None,meanX=None,standardDeviationX=None,minimumViewStart=(1<<63)-1,maximumViewStart=-(1<<63))
  assert char==expected,('prepared prefix CHAR',boundary,char,expected)
 assert result['scratch']==(full['scratch']if boundary>=4 else [])
 assert result['horizontalBoundaries']==(full['horizontalBoundaries']if boundary>=4 else [])
 assert result['mutations']==(full['mutations']if boundary>=5 else [])
 for key in ['meanWidth','meanSpacing']:exact32(result[key],full[key]if boundary>=5 else 0,'prefix '+key)
 exact32(result['globalMinimumX'],f(3.4028234663852886e38),'prefix global min')
 exact32(result['globalMaximumX'],decode32(0x00800000),'prefix global max')
 line=full['line']if boundary>=3 else dict(full['line'],minimumX=0,maximumX=0,meanY=0,standardDeviationY=0,baselineAtMinimumX=0,baselineAtMaximumX=0,reversed=False)
 assert result['line']==line
if a.native:assert a.capture.read_bytes()==a.native.read_bytes()
report=dict(status='PASS independent actual geometry numeric oracle',samples=13,genuineMeasuredTracks=9,preparedUniformCases=4,characters=characters,views=views,
 parentSha256=hashlib.sha256(parent_raw).hexdigest(),captureSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),
 parentValidation='historical-frozen'if a.models is None else'paired-independent-raw-child-and-parent',vendorExecution=False,qualityClaim=False)
if a.report:a.report.write_text(json.dumps(report,indent=2)+'\n')
print('PASS independent13 ordinary2DB numeric cases/37CHAR/54views, actual measured9 distinct from prepared4; no quality claim')
