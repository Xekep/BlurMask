import base64,copy,json,math,struct,sys,unicodedata
from pathlib import Path

def need(v,m):
 if not v:raise AssertionError(m)
def obj(x,keys):need(type(x)is dict and set(x)==set(keys),'object schema '+str(keys))
def integer(x,lo,hi):need(type(x)is int and lo<=x<=hi,'integer/range');return x
def boolean(x):need(type(x)is bool,'boolean');return x
def number(x):need(type(x)in(int,float) and math.isfinite(x),'finite number');return x
def string(x):need(type(x)is str,'string');x.encode('utf-8','strict');return x
def array(x,lo,hi):need(type(x)is list and lo<=len(x)<=hi,'array extent');return x
def same(a,b):
 need(type(a)is type(b),'canonical JSON type changed')
 if type(a)is dict:
  need(a.keys()==b.keys(),'canonical keys changed')
  for k in a:same(a[k],b[k])
 elif type(a)is list:
  need(len(a)==len(b),'canonical length changed')
  for x,y in zip(a,b):same(x,y)
 else:need(a==b,'canonical value changed')
def pairs(rows):
 out={}
 for k,v in rows:need(k not in out,'duplicate JSON key');out[k]=v
 return out
def load(p):return json.loads(Path(p).read_text(encoding='utf-8-sig'),object_pairs_hook=pairs,parse_constant=lambda x:(_ for _ in()).throw(AssertionError('nonfinite JSON')))
def b64(s):
 string(s);b=base64.b64decode(s,validate=True);need(base64.b64encode(b).decode()==s,'canonical base64');return b
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def norm(x):return x&~0xff00 if x>>8&255==255 else x
def join(a,b):return (a>>32)+1==b>>32 and (((a&65535)+0xfd)&0xfffd)==0 and b&0xffffffff in(3,0xff03)
def union(rows,more):
 rows=[r.copy() for r in rows]
 def find(key,start,end,before=0,after=0):
  while start<end:
   mid=(start+end)//2;a,b=rows[mid]
   if norm(a)-before<=key:
    if key<=norm(b)+after:return mid,True
    start=mid+1
   else:end=mid
  return start,False
 for a,b in more:
  need(a<=b,'range reversal');i,ok=find(norm(a),0,len(rows),after=2)
  if ok:
   j,end=find(norm(b),i,len(rows),before=2)
   if end:rows[i]=[rows[i][0],rows[j][1]];del rows[i+1:j+1];continue
   rows[i]=[rows[i][0],b];del rows[i+1:j]
   if i+1<len(rows) and join(b,rows[i+1][0]):rows[i][1]=rows[i+1][1];del rows[i+1]
   continue
  k,end=find(norm(b),i,len(rows),before=2)
  if end:
   rows[k][0]=a;del rows[i:k]
   if i>0 and join(rows[i-1][1],a):rows[i-1][1]=rows[i][1];del rows[i]
  elif i>0 and join(rows[i-1][1],a):
   if k<len(rows) and join(b,rows[k][0]):rows[i-1][1]=rows[k][1];del rows[i:k+1]
   else:rows[i-1][1]=b;del rows[i:k]
  elif k<len(rows) and join(b,rows[k][0]):rows[k][0]=a;del rows[i:k]
  elif i==k:rows.insert(i,[a,b])
  else:rows[i]=[a,b];del rows[i+1:k]
 return rows

def ranges(value,raw):
 rows=[]
 for r in array(value,1,4096):
  obj(r,['First','Last']);a=integer(r['First'],0,2**64-1);b=integer(r['Last'],0,2**64-1)
  need(0<=a>>32<=b>>32<len(raw),'source index bounds')
  for v in(a,b):
   need((v&255)in(1,3,5,7),'source mode');need((v>>16&65535)<len(raw[v>>32]),'source point bounds');need((v>>8&255)<=199 or(v>>8&255)==255,'fraction bound')
  need(a<=b,'ordered source range');rows.append([a,b])
 return rows

def geometry(rows,raw):
 points=[];reads=0
 for a,b in rows:
  first,last=a>>32,b>>32
  for i in range(first,last+1):
   values=raw[i];pf=i==first and ((a>>16&65535)!=0 or (a>>8&255)!=0 or(a&255)!=3);pl=i==last and(b>>8&255)!=255
   start=(a>>16&65535)if pf else 0;end=(b>>16&65535)if pl else len(values)-1
   fa=f((a>>8&255)*f(.005))if pf and(a>>8&255)!=255 else 0;fb=f((b>>8&255)*f(.005))if pl else 0
   need(0<=start<=end<len(values) and 0<=fa<1 and 0<=fb<1,'selected extent')
   def at(n,t):
    x,y=map(f,values[n]);
    if not t:return x,y
    need(n+1<len(values),'fraction endpoint')
    xx,yy=map(f,values[n+1]);return f(x+f(f(xx-x)*t)),f(y+f(f(yy-y)*t))
   points.append(at(start,fa));points.extend(tuple(map(f,p))for p in values[start+1:end+1]);points.append(at(end,fb));reads+=end-start+2+(fa>0)+(fb>0)
 box=dict(zip(['Left','Top','Right','Bottom'],[min(p[0]for p in points),min(p[1]for p in points),max(p[0]for p in points),max(p[1]for p in points)]))
 need(box['Left']<box['Right']and box['Top']<box['Bottom'],'positive bounds');return box,reads

def expected(old,input):
 raw=array(input['Raw'],1,256);integer(input['Resolution'],1,9600);need(sum(len(array(stroke,1,4096))for stroke in raw)<=4096,'bounded raw points')
 for stroke in raw:
  for point in stroke:
   array(point,2,2);need(all(abs(number(v))<=1000000 for v in point),'finite raw coordinates')
 pool={integer(p['Key'],0,2**64-1):b64(p['Storage40'])for p in old['AfterText']['Pool']};uses=[];coverage=[];total=0;utf8total=0;glyphs=0
 linebytes=b64(old['SourceLines56']);need(len(linebytes)==56,'line56');line={'Index':0,**{k:struct.unpack_from('<d',linebytes,n)[0]for k,n in [('OriginX',8),('OriginY',16),('Angle',24),('XHeight',32),('UnadjustedBaseline',48)]},'OwnerTag':struct.unpack_from('<q',linebytes,40)[0]}
 need(all(math.isfinite(v)for v in line.values())and line['XHeight']>0,'line finite')
 nodes=old['AfterBatch']['Nodes'];need(len(nodes)==len(old['Words']),'node/WORD use count')
 for i,(word,node)in enumerate(zip(old['Words'],nodes)):
  choices=word['Choices'];selected=integer(word['SelectedIndex'],0,len(choices)-1);links=choices[selected]['Links'];score=1;policy=True;parts=[]
  for link in links:
   ref=integer(link['Reference'],0,2**64-1);key=ref&0x010000ffffffffff;index=ref>>40&65535;need(index<32768 and key in pool,'actual pool reference');row=pool[key][index*40:(index+1)*40];need(len(row)==40,'actual pool40')
   bs=b64(link['Utf8']);text=bs.decode('utf-8','strict');parts.append(text);utf8total+=len(bs);glyphs+=1
   v=struct.unpack_from('<f',row,16)[0];need(math.isfinite(v),'nonfinite native score cannot be JSON output');score=min(score,v)
   whitespace=lambda c:c in '\t\n\v\f\r\u0085' or unicodedata.category(c)in('Zs','Zl','Zp')
   policy &= (struct.unpack_from('<H',row,4)[0]&7)==0 and .5<=v<=1 and bool(text)and not any(whitespace(c)or unicodedata.category(c)in('Cc','Cf','Co','Cs')for c in text)
  need(1<=len(links)<=128 and utf8total<=1024 and glyphs<=128,'whole glyph/UTF8 limit')
  text=''.join(parts);need(text==b64(word['SelectedUtf8']).decode('utf-8','strict'),'literal selected word bytes')
  owner=[o for o in old['AfterBatch']['Owners']if(o['First'],o['Last'])==(node['First'],node['Last'])];need(len(owner)==1,'same span source owner');owner=owner[0]
  si=integer(node['WordIndex'],0,len(owner['Words'])-1)
  r=ranges(word['OriginalRanges'],raw);coverage=union(coverage,r);box,reads=geometry(r,raw);total+=reads
  uses.append(dict(Index=i,SourceSelectedIndex=si,ProcessedSelectedIndex=selected,SourceChoiceCount=len(owner['Words']),ProcessedChoiceCount=len(choices),SelectedUtf8=text,CandidateScore=score,MeetsExperimentalGlyphPolicy=bool(policy),Line=line.copy(),OriginalRanges=copy.deepcopy(word['OriginalRanges']),Bounds=box,PointReads=reads))
 full=union([],[[j<<32|3,j<<32|(len(stroke)-1)<<16|0xff05]for j,stroke in enumerate(raw)])
 need(total<=1048576 and len(uses)<=128,'whole projection bound')
 return dict(WordCount=len(uses),FullOriginalCoverage=coverage==full,MeetsExperimentalGlyphPolicy=all(u['MeetsExperimentalGlyphPolicy']for u in uses),ProjectedPointReads=total,MaximumPointReads=1048576,SameOriginalDocument=True,Coverage=[dict(First=a,Last=b)for a,b in coverage],Uses=uses)

def validation_delta(old):
 #NARROW native source construction: one publisher from pinned factory, one TOP/use,
 #one unique275 owner. P is source-construction fact, not DATA authority.
 nodes=old['AfterBatch']['Nodes'];owners=old['AfterBatch']['Owners'];need(len(nodes)==len(owners)==1,'fixed single-WORD audit scope')
 P=1;W=len(nodes);U=len(owners);A=sum(len(o['Words'])for o in owners);K=sum(len(o['SelectedOrdinals'])for o in owners)
 need(A==2 and K==1,'fixed actual two-choice/one-selection scope')
 G=sum(len(w['Choices'][w['SelectedIndex']]['Links'])for w in old['Words'])
 complete=P+W+2*U #Validate P+W+U then RequireCompleted spans U
 queue=complete+U+A+K+W
 binding=complete+queue
 #Reader: one binding; two binding validations in CopySelectedUtf8 per glyph.
 #Queue calls: posttext2 +initial count1 +final read1 +public final1;
 #per WORD two passes7+7, final scalar getters2+2+3+2.
 reader=(1+2*G)*binding+(5+23*W)*queue
 #Preexisting e759 reuse added ReservePreparationCopy once, and two
 #per-use cursor getters FirstPayloadOrdinal/ReusePlan before publication.
 inherited=(1+2*W)*queue
 return reader+inherited

def audit(old,new,input):
 need(type(new)is dict and 'StructuredWordConsumer'in new,'new consumer required');prior={k:v for k,v in new.items()if k!='StructuredWordConsumer'};prior=copy.deepcopy(prior);before=integer(old['ValidationLoopBodies'],0,2**63-1);after=integer(prior['ValidationLoopBodies'],0,2**63-1);need(after-before==validation_delta(old),'explained cumulative validation delta');prior['ValidationLoopBodies']=before;same(old,prior)
 s=new['StructuredWordConsumer'];e=expected(old,input);obj(s,e)
 for k in ['FullOriginalCoverage','MeetsExperimentalGlyphPolicy','SameOriginalDocument']:need(boolean(s[k])is e[k],k)
 for k in ['WordCount','ProjectedPointReads','MaximumPointReads']:need(integer(s[k],0,1048576)==e[k],k)
 same(s['Coverage'],e['Coverage']);ranges(s['Coverage'],input['Raw']);array(s['Uses'],1,128);need(len(s['Uses'])==len(e['Uses']),'use denominator')
 for u,v in zip(s['Uses'],e['Uses']):
  obj(u,v)
  for k in ['Index','SourceSelectedIndex','ProcessedSelectedIndex','SourceChoiceCount','ProcessedChoiceCount','PointReads']:need(integer(u[k],0,1048576)==v[k],k)
  need(string(u['SelectedUtf8'])==v['SelectedUtf8'],'selected UTF8');need(f(number(u['CandidateScore']))==v['CandidateScore'],'pool+16 minimum');need(boolean(u['MeetsExperimentalGlyphPolicy'])is v['MeetsExperimentalGlyphPolicy'],'glyph host policy')
  same(u['OriginalRanges'],v['OriginalRanges']);obj(u['Bounds'],v['Bounds'])
  for k in u['Bounds']:need(f(number(u['Bounds'][k]))==v['Bounds'][k],'bound '+k)
  obj(u['Line'],v['Line'])
  for k in ['Index','OwnerTag']:need(type(u['Line'][k])is int and u['Line'][k]==v['Line'][k],'line int')
  for k in ['OriginX','OriginY','Angle','XHeight','UnadjustedBaseline']:need(number(u['Line'][k])==v['Line'][k],'line scalar')
 return {'Words':e['WordCount'],'UTF8Bytes':sum(len(u['SelectedUtf8'].encode('utf-8'))for u in e['Uses']),'PointReads':e['ProjectedPointReads'],'FullCoverage':e['FullOriginalCoverage'],'Policy':e['MeetsExperimentalGlyphPolicy']}

if __name__=='__main__':
 import argparse,hashlib
 p=argparse.ArgumentParser(description='Strict DATA-only oneTOP/oneowner/twochoice consumer audit. No model loading or source authority.')
 p.add_argument('baseline',type=Path);p.add_argument('capture',type=Path);p.add_argument('input',type=Path)
 for name in ['baseline','capture','input']:p.add_argument('--'+name+'-sha256',required=True)
 a=p.parse_args()
 for name in ['baseline','capture','input']:
  expected_pin=getattr(a,name+'_sha256');need(len(expected_pin)==64 and all(c in '0123456789abcdef'for c in expected_pin),'lowercase SHA256 pin');need(hashlib.sha256(getattr(a,name).read_bytes()).hexdigest()==expected_pin,'immutable '+name+' pin')
 print(json.dumps({'PASS':audit(load(a.baseline),load(a.capture),load(a.input))},ensure_ascii=True))
