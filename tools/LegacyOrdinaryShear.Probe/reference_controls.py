"""Independent NumPy24D oracle, actual AL stored relationships, no vendor execution."""
import base64,json,math,random,struct,sys
from pathlib import Path
import numpy as np
f=np.float32
def alphabet(data):
 at=99
 def u(fmt):
  nonlocal at
  v=struct.unpack_from('<'+fmt,data,at)[0];at+=struct.calcsize('<'+fmt);return v
 count=[u('H') for _ in range(8)][-1];n=u('H')
 for inc in [32,64,1]:n=next(v for v in reversed([u('H') for _ in range(n)]) if v!=65535)+inc
 spans=[u('H') for _ in range(n+1)];records=[(u('I'),u('I')) for _ in range(spans[-1]+1)]
 at+=records[-1][0]+2*count
 if u('B'):at+=(count+7)//8
 triplets=u('H');forms=[(u('H'),u('H'),u('H')) for _ in range(triplets)];flag=u('I') if triplets else 0
 maps=[[u('b' if triplets<128 else 'h') for _ in range(count)] if flag&(1<<m) else None for m in range(3)]
 bits=[any(m is not None and m[i]!=-1 for m in maps) for i in range(count)]
 runs=[]
 for i,bit in enumerate(bits):
  if bit and (i==0 or not bits[i-1]):runs.append([i,i])
  elif bit:runs[-1][1]=i
 assert len(runs)<=2
 first=[]
 for label in range(count):
  if maps[0] is None or maps[0][label]==-1:first.append(False);continue
  row=list(forms[maps[0][label]])
  if label not in row:
   if row[0]==65535:row[0]=label
   elif row[1]==65535:row[1]=label
  first.append(any(a<=label<=b for a,b in runs) and row[0]==label)
 return first
def oracle(points,descriptors,guides,rows,associations,profile,first_forms):
 points=[[np.array(p,dtype=np.float32) for p in s] for s in points]
 minimum,maximum,floor,normalizer,low,high,step=profile
 gaps=sum(d[2]-d[1]-1 for d in descriptors);blend=f(max(0,min(1,f(f(gaps-minimum)/f(maximum-minimum)))))
 total=f(0);first_weight=f(0)
 for _,start,end,_,_ in descriptors:
  for row in rows[start:end]:
   label=row['label'];weight=f(row['weight'])
   if label==-1:continue
   total=f(total+weight)
   if first_forms[label]:first_weight=f(first_weight+weight)
 if total!=0:blend=f(blend*f(max(floor,min(1,f(first_weight/f(total*normalizer))))))
 low=f(blend*low);high=f(blend*high);chosen=f(0);best=f(-np.finfo(np.float32).max);candidate=low;iterations=0
 def visit(row):
  for a,p,z,q in row['ranges']:
   for s in range(a,z+1):
    for index in range(p if s==a else 0,(q if s==z else len(points[s])-1)+1):yield points[s][index]
 while candidate<high:
  iterations+=1;gap_sum=f(0);count=0
  for line,start,end,_,_ in descriptors:
   base,height=guides[line];anchor=f(candidate*base);front=f(-np.finfo(np.float32).max)
   for row in rows[start:end]:
    if not row['ranges']:continue
    xs=[f(f(f(-candidate*p[1])+p[0])+anchor) for p in visit(row)];left=min(xs);right=max(xs)
    if front!=-np.finfo(np.float32).max:gap_sum=f(gap_sum+f(f(left-front)/height));count+=1
    front=max(front,right)
  if count and best<gap_sum:best=gap_sum;chosen=candidate
  candidate=f(candidate+step)
 length=0.;weighted=0.
 for _,_,_,start,end in descriptors:
  for s in range(start,end+1):
   if associations[s]!=-1:continue
   for p,q in zip(points[s],points[s][1:]):
    dx=f(q[0]-p[0]);dy=f(q[1]-p[1])
    if dy<0:dx=f(-dx);dy=f(-dy)
    if dy<=f(1e-10):continue
    ratio=f(dx/dy)
    if ratio<low or ratio>high:continue
    segment=math.sqrt(float(f(f(dy*dy)+f(dx*dx))));length+=segment;weighted+=float(ratio)*segment
 direction=f(weighted/length) if length>=1e-10 else f(0);shear=f(f(direction+chosen)*f(.5));boxes=[]
 for line,start,end,s0,s1 in descriptors:
  anchor=f(shear*guides[line][0])
  for s in range(s0,s1+1):
   for p in points[s]:p[0]=f(f(f(-shear*p[1])+p[0])+anchor)
  for row in rows[start:end]:
   if row['ranges']:
    ps=list(visit(row));box=[min(p[0] for p in ps),max(p[0] for p in ps),min(p[1] for p in ps),max(p[1] for p in ps)]
   else:
    l,r,t,b=map(f,row['box']);cy=f(f(b+t)*f(.5));hy=f(f(b-t)*f(.5));hx=f(f(r-l)*f(.5));cx=f(f(f(f(r+l)*f(.5))+f(cy*f(-shear)))+anchor)
    box=[f(cx-hx),f(hx+cx),f(cy-hy),f(hy+cy)]
   boxes.append(list(map(float,box)))
 return dict(blend=float(blend),low=float(low),high=float(high),gap=float(chosen),direction=float(direction),shear=float(shear),iterations=iterations,
             boxes=boxes,points=[[[float(v) for v in p] for p in s] for s in points])
def encode(d,g,rows):
 descriptors=[];guides=[]
 for line,start,end,s0,s1 in d:
  b=bytearray(88)
  for offset,value in [(16,line),(36,s0),(40,s1),(52,start),(56,end)]:struct.pack_into('<i',b,offset,value)
  descriptors.append(base64.b64encode(b).decode())
 for base,height in g:
  b=bytearray(40);struct.pack_into('<ff',b,0,base,height);guides.append(base64.b64encode(b).decode())
 for row in rows:
  b=bytearray(720);struct.pack_into('<q',b,8,len(row['ranges']));struct.pack_into('<ffff',b,24,*row['box']);struct.pack_into('<h',b,40,row['label']);struct.pack_into('<f',b,80,row['weight']);row['storage']=base64.b64encode(b).decode()
 return descriptors,guides
if __name__=='__main__':
 if len(sys.argv)!=3:raise ValueError('AK.res output.json')
 ak=Path(sys.argv[1]).read_bytes();flags=alphabet(ak);raw=ak[188012:188050];assert struct.unpack_from('<I',raw,8)[0]==0x24d
 minimum,maximum=raw[16:18];floor,normalizer,a,b,angle=struct.unpack_from('<5f',raw,18)
 low=f(math.tan(a));high=f(math.tan(b));step=f(f(high-low)/f(f(f(b)-f(a))/f(angle)))
 profile=[minimum,maximum,floor,normalizer,float(low),float(high),float(step)];rng=random.Random(0x177280);cases=[]
 for n in range(96):
  phys=1+n%6;points=[]
  for s in range(phys):
   points.append([[float(f(s*75+p*12+rng.uniform(-5,5))),float(f((p%3)*30+rng.uniform(-2,2)+(s%2)*160))] for p in range(3+n%11)])
  split=phys//2 if n%3==0 and phys>1 else phys;intervals=[(0,split-1)] if split==phys else [(0,split-1),(split,phys-1)]
  rows=[];d=[];g=[]
  for line,(s0,s1) in enumerate(intervals):
   start=len(rows);count=1+(n+line)%9
   for r in range(count):
    s=s0+r%(s1-s0+1);p=(r*2)%len(points[s]);q=min(len(points[s])-1,p+2)
    ranges=[] if (n+r)%7==0 else [[s,p,s,q]]
    if (n+r)%13==0 and s<s1:ranges=[[s,p,s+1,min(q,len(points[s+1])-1)]]
    label=[-1,0,65,120,134,100,20][(n+r)%7]
    rows.append(dict(ranges=ranges,label=label,weight=float(f(.1+((n+r)%9)*.1)),box=[s*75+r*5,s*75+r*5+12,r*5,r*5+20]))
   d.append((line,start,len(rows),s0,s1));g.append((float(f(90+line*160)),float(f(30+n%40))))
  associations=[-1 if (n+s)%5 else 3 for s in range(phys)]
  expectation=oracle(points,d,g,rows,associations,profile,flags);db,gb=encode(d,g,rows)
  cases.append(dict(points=points,descriptors=db,guides=gb,rows=rows,associations=associations,expected=expectation))
 for dy in [np.nextafter(f(1e-10),f(0)),f(1e-10),np.nextafter(f(1e-10),f(1))]:
  points=[[[0.,0.],[float(f(dy*f(.1))),float(dy)]]];d=[(0,0,3,0,0)];g=[(1.,1.)];associations=[-1]
  rows=[dict(ranges=[[0,0,0,1]],label=0,weight=1.,box=[0.,1.,0.,1.]) for _ in range(3)]
  expectation=oracle(points,d,g,rows,associations,profile,flags);db,gb=encode(d,g,rows)
  cases.append(dict(points=points,descriptors=db,guides=gb,rows=rows,associations=associations,expected=expectation))
 Path(sys.argv[2]).write_bytes((json.dumps(dict(profile=list(profile),firstForms=flags,cases=cases),separators=(',',':'))+'\n').encode())
 print('Generated99 independent native24D cases, genuine239-label relationships, empty/crossphysical ranges, associations and float direction-threshold neighbours.')
